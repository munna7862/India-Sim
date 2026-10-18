using System;
using System.Collections.Generic;
using System.Linq;
using WorldState.Domain.Data;
using WorldState.Domain.Map;
using WorldState.Domain.Models;

namespace WorldState.Domain.ViewModels;

public record CountryVisualState(
    string CountryId,
    string Name,
    ColorRgba FillColor,
    ColorRgba BorderColor,
    float BorderWidth,
    bool IsSelected,
    bool IsHovered,
    bool IsPlayer,
    string StanceText,
    int RelationScore,
    bool IsAtWar,
    bool HasNap,
    IReadOnlyList<IReadOnlyList<(double X, double Y)>> ProjectedPolygons,
    (double X, double Y) ProjectedCentroid
);

public class WorldMapViewModel
{
    private readonly WorldMapData _mapData;
    private readonly GameSession _session;
    private readonly CountryRegistry? _registry;

    private double _viewportWidth = 1280.0;
    private double _viewportHeight = 720.0;
    private double _zoom = 1.0;
    private double _panX = 0.0;
    private double _panY = 0.0;

    private string? _selectedCountryId;
    private string? _hoveredCountryId;
    private MapPalette _palette = MapThemeRegistry.TacticalDark;

    // Cache of projected geometry
    private bool _cacheDirty = true;
    private readonly Dictionary<string, (IReadOnlyList<IReadOnlyList<(double X, double Y)>> Polygons, (double X, double Y) Centroid)> _projectionCache = new();

    public event Action? OnStateChanged;

    public double ViewportWidth => _viewportWidth;
    public double ViewportHeight => _viewportHeight;
    public double Zoom => _zoom;
    public double PanX => _panX;
    public double PanY => _panY;
    public string? SelectedCountryId => _selectedCountryId;
    public string? HoveredCountryId => _hoveredCountryId;
    public MapPalette ActivePalette => _palette;

    public WorldMapViewModel(WorldMapData mapData, GameSession session, CountryRegistry? registry = null, string? paletteName = null)
    {
        _mapData = mapData ?? throw new ArgumentNullException(nameof(mapData));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _registry = registry;
        _palette = MapThemeRegistry.GetPalette(paletteName);

        // Default selection to player country
        var player = _session.GetPlayerCountry();
        if (player != null)
        {
            _selectedCountryId = player.Id;
        }
    }

    public void SetViewportSize(double width, double height)
    {
        if (width <= 0 || height <= 0) return;
        if (Math.Abs(_viewportWidth - width) > 0.001 || Math.Abs(_viewportHeight - height) > 0.001)
        {
            _viewportWidth = width;
            _viewportHeight = height;
            _cacheDirty = true;
            OnStateChanged?.Invoke();
        }
    }

    public void SetPalette(string? paletteName)
    {
        _palette = MapThemeRegistry.GetPalette(paletteName);
        OnStateChanged?.Invoke();
    }

    public void CycleNextPalette()
    {
        var palettes = MapThemeRegistry.GetAllPalettes().ToList();
        int idx = palettes.FindIndex(p => p.Name.Equals(_palette.Name, StringComparison.OrdinalIgnoreCase));
        int nextIdx = (idx + 1) % palettes.Count;
        _palette = palettes[nextIdx];
        OnStateChanged?.Invoke();
    }

    public bool SelectCountry(string? countryId)
    {
        if (_selectedCountryId == countryId) return false;
        _selectedCountryId = countryId;
        OnStateChanged?.Invoke();
        return true;
    }

    public void SetHoveredCountry(string? countryId)
    {
        if (_hoveredCountryId != countryId)
        {
            _hoveredCountryId = countryId;
            OnStateChanged?.Invoke();
        }
    }

    public void Pan(double deltaX, double deltaY)
    {
        _panX += deltaX;
        _panY += deltaY;

        // Apply soft bounds clamp based on zoom
        double maxPanX = (_viewportWidth * _zoom) / 1.5;
        double maxPanY = (_viewportHeight * _zoom) / 1.5;
        _panX = Math.Clamp(_panX, -maxPanX, maxPanX);
        _panY = Math.Clamp(_panY, -maxPanY, maxPanY);

        _cacheDirty = true;
        OnStateChanged?.Invoke();
    }

    public void ZoomBy(double factor, double? cursorX = null, double? cursorY = null)
    {
        double oldZoom = _zoom;
        double newZoom = Math.Clamp(_zoom * factor, MapProjection.MinZoom, MapProjection.MaxZoom);

        if (Math.Abs(oldZoom - newZoom) < 0.0001) return;

        if (cursorX.HasValue && cursorY.HasValue)
        {
            // Zoom towards cursor
            double cX = _viewportWidth / 2.0;
            double cY = _viewportHeight / 2.0;
            double offsetX = cursorX.Value - cX - _panX;
            double offsetY = cursorY.Value - cY - _panY;

            double scaleRatio = newZoom / oldZoom;
            _panX -= offsetX * (scaleRatio - 1.0);
            _panY -= offsetY * (scaleRatio - 1.0);
        }

        _zoom = newZoom;
        _cacheDirty = true;
        OnStateChanged?.Invoke();
    }

    public void ResetCamera()
    {
        _zoom = 1.0;
        _panX = 0.0;
        _panY = 0.0;
        _cacheDirty = true;
        OnStateChanged?.Invoke();
    }

    public bool FocusCountry(string countryId)
    {
        if (_mapData.TryGet(countryId, out var geom) && geom != null)
        {
            _selectedCountryId = countryId;
            // Project centroid at zoom 1 to determine pan offset
            var (projX, projY) = MapProjection.Project(geom.Centroid, _viewportWidth, _viewportHeight, 1.0, 0.0, 0.0);
            double cX = _viewportWidth / 2.0;
            double cY = _viewportHeight / 2.0;

            _panX = -(projX - cX) * _zoom;
            _panY = -(projY - cY) * _zoom;
            _cacheDirty = true;
            OnStateChanged?.Invoke();
            return true;
        }
        return false;
    }

    public string? HandleClick(double screenX, double screenY)
    {
        var geoPt = MapProjection.Unproject(screenX, screenY, _viewportWidth, _viewportHeight, _zoom, _panX, _panY);
        string? hitId = _mapData.HitTest(geoPt);
        if (!string.IsNullOrEmpty(hitId))
        {
            SelectCountry(hitId);
            return hitId;
        }
        return null;
    }

    public void HandleHover(double screenX, double screenY)
    {
        var geoPt = MapProjection.Unproject(screenX, screenY, _viewportWidth, _viewportHeight, _zoom, _panX, _panY);
        string? hitId = _mapData.HitTest(geoPt);
        SetHoveredCountry(hitId);
    }

    public IReadOnlyList<CountryVisualState> GetCountryVisualStates()
    {
        EnsureProjectionsCached();

        var player = _session.GetPlayerCountry();
        string playerCountryId = player?.Id ?? string.Empty;

        var list = new List<CountryVisualState>();

        foreach (var geom in _mapData.Countries)
        {
            string cid = geom.CountryId;
            bool isPlayer = string.Equals(cid, playerCountryId, StringComparison.OrdinalIgnoreCase);
            bool isSelected = string.Equals(cid, _selectedCountryId, StringComparison.OrdinalIgnoreCase);
            bool isHovered = string.Equals(cid, _hoveredCountryId, StringComparison.OrdinalIgnoreCase);

            int relationScore = 0;
            bool isAtWar = false;
            bool hasNap = false;
            string stanceText = isPlayer ? "Sovereign Homeland" : "Neutral";

            if (!isPlayer && player != null)
            {
                var stance = player.GetRelationsWith(cid);
                relationScore = (int)Math.Round(stance.Relations);
                isAtWar = stance.IsAtWar;
                hasNap = stance.HasActiveNonAggressionPact(_session.Clock.CurrentDate);

                if (isAtWar)
                {
                    stanceText = "State of War";
                }
                else if (hasNap)
                {
                    stanceText = "Non-Aggression Pact";
                }
                else if (relationScore >= 50)
                {
                    stanceText = $"Friendly ({relationScore:+0;-#})";
                }
                else if (relationScore <= -50)
                {
                    stanceText = $"Hostile ({relationScore:+0;-#})";
                }
                else
                {
                    stanceText = $"Neutral ({relationScore:+0;-#})";
                }
            }

            var (fill, border, width) = MapThemeRegistry.GetCountryColors(
                _palette,
                isPlayer,
                isSelected,
                isHovered,
                relationScore,
                isAtWar,
                hasNap
            );

            var (projectedPolygons, projectedCentroid) = _projectionCache[cid];

            string displayName = geom.Name;
            if (_registry != null && _registry.TryGet(cid, out var def) && def != null)
            {
                displayName = def.Name;
            }

            list.Add(new CountryVisualState(
                CountryId: cid,
                Name: displayName,
                FillColor: fill,
                BorderColor: border,
                BorderWidth: width,
                IsSelected: isSelected,
                IsHovered: isHovered,
                IsPlayer: isPlayer,
                StanceText: stanceText,
                RelationScore: relationScore,
                IsAtWar: isAtWar,
                HasNap: hasNap,
                ProjectedPolygons: projectedPolygons,
                ProjectedCentroid: projectedCentroid
            ));
        }

        return list;
    }

    public string? GetHoverTooltipText()
    {
        if (string.IsNullOrEmpty(_hoveredCountryId)) return null;

        var geom = _mapData.Countries.FirstOrDefault(c => string.Equals(c.CountryId, _hoveredCountryId, StringComparison.OrdinalIgnoreCase));
        if (geom == null) return null;

        var player = _session.GetPlayerCountry();
        bool isPlayer = player != null && string.Equals(player.Id, _hoveredCountryId, StringComparison.OrdinalIgnoreCase);

        _session.Countries.TryGetValue(_hoveredCountryId, out var countryState);
        string capital = "N/A";
        string name = geom.Name;

        if (_registry != null && _registry.TryGet(_hoveredCountryId, out var def) && def != null)
        {
            capital = def.Capital;
            name = def.Name;
        }

        if (isPlayer && countryState != null)
        {
            return $"{name} [{_hoveredCountryId}]\n" +
                   $"Capital: {capital} • Sovereign Homeland\n" +
                   $"Treasury: {HudViewModel.FormatCurrency(countryState.Treasury)} | Pop: {HudViewModel.FormatNumber(countryState.Population)}\n" +
                   $"Approval: {countryState.ApprovalRating:F1}% | Living Std: {countryState.QualityOfLiving:F1}%";
        }

        if (player != null && !isPlayer)
        {
            var stance = player.GetRelationsWith(_hoveredCountryId);
            bool hasNap = stance.HasActiveNonAggressionPact(_session.Clock.CurrentDate);
            string relationDesc = stance.IsAtWar ? "WAR" : (hasNap ? "Allied / NAP" : $"Score: {(int)Math.Round(stance.Relations):+0;-0}");

            string stats = countryState != null
                ? $"Treasury: {HudViewModel.FormatCurrency(countryState.Treasury)} | Pop: {HudViewModel.FormatNumber(countryState.Population)}"
                : "Foreign Territory";

            return $"{name} [{_hoveredCountryId}]\n" +
                   $"Capital: {capital} • Status: {relationDesc}\n" +
                   $"{stats}\n" +
                   "Click to open Foreign Affairs";
        }

        return $"{name} [{_hoveredCountryId}]\nCapital: {capital}";
    }

    private void EnsureProjectionsCached()
    {
        if (!_cacheDirty && _projectionCache.Count == _mapData.Countries.Count)
        {
            return;
        }

        _projectionCache.Clear();

        foreach (var country in _mapData.Countries)
        {
            var polyList = new List<IReadOnlyList<(double X, double Y)>>();
            foreach (var poly in country.Polygons)
            {
                var ringPts = new List<(double X, double Y)>(poly.Ring.Count);
                foreach (var pt in poly.Ring)
                {
                    ringPts.Add(MapProjection.Project(pt, _viewportWidth, _viewportHeight, _zoom, _panX, _panY));
                }
                polyList.Add(ringPts);
            }

            var centroidPt = MapProjection.Project(country.Centroid, _viewportWidth, _viewportHeight, _zoom, _panX, _panY);
            _projectionCache[country.CountryId] = (polyList, centroidPt);
        }

        _cacheDirty = false;
    }
}
