using WorldState.Domain.Data;
using WorldState.Domain.Map;

namespace WorldState.Domain.ViewModels;

public class CountrySelectionViewModel
{
    private readonly CountryRegistry _registry;
    private readonly WorldMapData? _mapData;
    private string _searchQuery = string.Empty;
    private string? _selectedCountryId;
    private string? _hoveredCountryId;

    public double Zoom { get; private set; } = 1.0;
    public double PanX { get; private set; } = 0.0;
    public double PanY { get; private set; } = 0.0;

    public string SearchQuery => _searchQuery;
    public string? HoveredCountryId => _hoveredCountryId;
    public CountryDefinition? SelectedCountry =>
        !string.IsNullOrEmpty(_selectedCountryId) && _registry.TryGet(_selectedCountryId, out var def) ? def : null;

    public bool CanStartGame => SelectedCountry is { IsPlayable: true };

    public IReadOnlyList<CountryDefinition> FilteredCountries { get; private set; }

    public event Action? OnStateChanged;

    public CountrySelectionViewModel(CountryRegistry registry, WorldMapData? mapData = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _mapData = mapData;
        FilteredCountries = _registry.GetAll();

        // Default to India (IND) if present
        if (_registry.TryGet("IND", out _))
        {
            SelectCountry("IND");
        }
        else if (FilteredCountries.Count > 0)
        {
            SelectCountry(FilteredCountries[0].Id);
        }
    }

    public void SetSearchQuery(string query)
    {
        _searchQuery = query ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_searchQuery))
        {
            FilteredCountries = _registry.GetAll();
        }
        else
        {
            FilteredCountries = _registry.GetAll()
                .Where(c => c.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            c.Id.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase) ||
                            c.Capital.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        OnStateChanged?.Invoke();
    }

    public bool SelectCountry(string countryId)
    {
        if (string.IsNullOrWhiteSpace(countryId))
        {
            return false;
        }

        if (_registry.TryGet(countryId, out var def))
        {
            _selectedCountryId = def!.Id;

            // If map data is available, optionally pan towards country centroid
            if (_mapData != null && _mapData.TryGet(_selectedCountryId, out var geom) && geom != null)
            {
                // Centroid coordinates
                // We keep current zoom, or reset if too far
            }

            OnStateChanged?.Invoke();
            return true;
        }

        return false;
    }

    public void SetHoveredCountry(string? countryId)
    {
        if (_hoveredCountryId != countryId)
        {
            _hoveredCountryId = countryId;
            OnStateChanged?.Invoke();
        }
    }

    public bool HandleMapClick(double screenX, double screenY, double viewportWidth, double viewportHeight)
    {
        if (_mapData == null) return false;

        var geoPt = MapProjection.Unproject(screenX, screenY, viewportWidth, viewportHeight, Zoom, PanX, PanY);
        string? hitCountryId = _mapData.HitTest(geoPt);

        if (!string.IsNullOrEmpty(hitCountryId))
        {
            return SelectCountry(hitCountryId);
        }

        return false;
    }

    public void HandleMapHover(double screenX, double screenY, double viewportWidth, double viewportHeight)
    {
        if (_mapData == null)
        {
            SetHoveredCountry(null);
            return;
        }

        var geoPt = MapProjection.Unproject(screenX, screenY, viewportWidth, viewportHeight, Zoom, PanX, PanY);
        string? hitCountryId = _mapData.HitTest(geoPt);
        SetHoveredCountry(hitCountryId);
    }

    public void Pan(double deltaX, double deltaY)
    {
        PanX += deltaX;
        PanY += deltaY;
        OnStateChanged?.Invoke();
    }

    public void ZoomBy(double factor, double viewportWidth = 1280, double viewportHeight = 720)
    {
        Zoom = Math.Clamp(Zoom * factor, MapProjection.MinZoom, MapProjection.MaxZoom);
        OnStateChanged?.Invoke();
    }

    public void ResetViewport()
    {
        Zoom = 1.0;
        PanX = 0.0;
        PanY = 0.0;
        OnStateChanged?.Invoke();
    }
}
