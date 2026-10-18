using System;
using System.Collections.Generic;
using Godot;
using WorldState.Domain.Map;
using WorldState.Domain.ViewModels;

namespace WorldState.Desktop.Scenes;

public partial class WorldMapControl : Control
{
    private WorldMapViewModel? _viewModel;
    private bool _isDragging;
    private Vector2 _lastDragPos;
    private Vector2 _pressStartPos;
    private const float ClickDragThreshold = 6.0f;

    [Signal]
    public delegate void CountrySelectedEventHandler(string countryId);

    [Signal]
    public delegate void CountryHoveredEventHandler(string countryId, string tooltipText);

    public WorldMapViewModel? ViewModel => _viewModel;

    public void Initialize(WorldMapViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnStateChanged -= OnViewModelStateChanged;
        }

        _viewModel = viewModel;
        _viewModel.OnStateChanged += OnViewModelStateChanged;
        QueueRedraw();
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        ClipContents = true;
    }

    public override void _Notification(int what)
    {
        base._Notification(what);
        if (what == NotificationResized && _viewModel != null)
        {
            _viewModel.SetViewportSize(Size.X, Size.Y);
            QueueRedraw();
        }
    }

    private void OnViewModelStateChanged()
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (_viewModel == null) return;

        _viewModel.SetViewportSize(Size.X, Size.Y);
        var palette = _viewModel.ActivePalette;

        // 1. Draw Ocean Background
        DrawRect(new Rect2(0, 0, Size.X, Size.Y), ToGodotColor(palette.OceanColor));

        // 2. Draw Tactical Geographic Coordinate Grid
        DrawTacticalGrid(palette);

        // 3. Draw Country Polygons
        var visualStates = _viewModel.GetCountryVisualStates();

        // 3a. Fills
        foreach (var vs in visualStates)
        {
            var fillColor = ToGodotColor(vs.FillColor);
            foreach (var poly in vs.ProjectedPolygons)
            {
                if (poly.Count >= 3)
                {
                    var points = new Vector2[poly.Count];
                    for (int i = 0; i < poly.Count; i++)
                    {
                        points[i] = new Vector2((float)poly[i].X, (float)poly[i].Y);
                    }
                    DrawColoredPolygon(points, fillColor);
                }
            }
        }

        // 3b. Borders & Outlines
        foreach (var vs in visualStates)
        {
            var borderColor = ToGodotColor(vs.BorderColor);
            float borderWidth = vs.BorderWidth;

            foreach (var poly in vs.ProjectedPolygons)
            {
                if (poly.Count >= 2)
                {
                    var points = new Vector2[poly.Count];
                    for (int i = 0; i < poly.Count; i++)
                    {
                        points[i] = new Vector2((float)poly[i].X, (float)poly[i].Y);
                    }
                    DrawPolyline(points, borderColor, borderWidth, true);
                }
            }

            // 3c. Country Centroid Labels (ISO 3-letter code)
            if (vs.ProjectedCentroid.X >= 0 && vs.ProjectedCentroid.X <= Size.X &&
                vs.ProjectedCentroid.Y >= 0 && vs.ProjectedCentroid.Y <= Size.Y)
            {
                var labelPos = new Vector2((float)vs.ProjectedCentroid.X - 12f, (float)vs.ProjectedCentroid.Y + 4f);
                var labelColor = vs.IsSelected || vs.IsHovered
                    ? ToGodotColor(palette.HoverBorder)
                    : ToGodotColor(palette.TextColor);

                DrawString(
                    ThemeDB.FallbackFont,
                    labelPos,
                    vs.CountryId,
                    HorizontalAlignment.Center,
                    -1,
                    11,
                    labelColor
                );
            }
        }
    }

    private void DrawTacticalGrid(MapPalette palette)
    {
        if (_viewModel == null) return;

        var gridColor = ToGodotColor(palette.GridColor);

        // Parallels (Latitudes: -60 to 60 step 30)
        for (double lat = -60.0; lat <= 60.0; lat += 30.0)
        {
            var pStart = MapProjection.Project(new MapPoint(-180.0, lat), Size.X, Size.Y, _viewModel.Zoom, _viewModel.PanX, _viewModel.PanY);
            var pEnd = MapProjection.Project(new MapPoint(180.0, lat), Size.X, Size.Y, _viewModel.Zoom, _viewModel.PanX, _viewModel.PanY);

            if ((pStart.Y >= 0 && pStart.Y <= Size.Y) || (pEnd.Y >= 0 && pEnd.Y <= Size.Y))
            {
                DrawLine(
                    new Vector2((float)pStart.X, (float)pStart.Y),
                    new Vector2((float)pEnd.X, (float)pEnd.Y),
                    gridColor,
                    1.0f
                );
            }
        }

        // Meridians (Longitudes: -180 to 180 step 45)
        for (double lon = -180.0; lon <= 180.0; lon += 45.0)
        {
            var pStart = MapProjection.Project(new MapPoint(lon, 85.0), Size.X, Size.Y, _viewModel.Zoom, _viewModel.PanX, _viewModel.PanY);
            var pEnd = MapProjection.Project(new MapPoint(lon, -85.0), Size.X, Size.Y, _viewModel.Zoom, _viewModel.PanX, _viewModel.PanY);

            DrawLine(
                new Vector2((float)pStart.X, (float)pStart.Y),
                new Vector2((float)pEnd.X, (float)pEnd.Y),
                gridColor,
                1.0f
            );
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (_viewModel == null) return;

        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.WheelUp && mb.Pressed)
            {
                _viewModel.ZoomBy(1.15, mb.Position.X, mb.Position.Y);
                AcceptEvent();
                return;
            }
            if (mb.ButtonIndex == MouseButton.WheelDown && mb.Pressed)
            {
                _viewModel.ZoomBy(1.0 / 1.15, mb.Position.X, mb.Position.Y);
                AcceptEvent();
                return;
            }

            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    _isDragging = true;
                    _lastDragPos = mb.Position;
                    _pressStartPos = mb.Position;
                }
                else
                {
                    _isDragging = false;
                    float dist = _pressStartPos.DistanceTo(mb.Position);
                    if (dist < ClickDragThreshold)
                    {
                        string? hitCountryId = _viewModel.HandleClick(mb.Position.X, mb.Position.Y);
                        if (!string.IsNullOrEmpty(hitCountryId))
                        {
                            EmitSignal(SignalName.CountrySelected, hitCountryId);
                        }
                    }
                }
                AcceptEvent();
                return;
            }

            if (mb.ButtonIndex == MouseButton.Right || mb.ButtonIndex == MouseButton.Middle)
            {
                _isDragging = mb.Pressed;
                _lastDragPos = mb.Position;
                AcceptEvent();
                return;
            }
        }
        else if (@event is InputEventMouseMotion mm)
        {
            if (_isDragging)
            {
                Vector2 delta = mm.Position - _lastDragPos;
                _lastDragPos = mm.Position;
                _viewModel.Pan(delta.X, delta.Y);
                AcceptEvent();
                return;
            }

            _viewModel.HandleHover(mm.Position.X, mm.Position.Y);
            string? hovered = _viewModel.HoveredCountryId;
            string tooltip = _viewModel.GetHoverTooltipText() ?? string.Empty;
            EmitSignal(SignalName.CountryHovered, hovered ?? string.Empty, tooltip);
        }
    }

    private static Godot.Color ToGodotColor(ColorRgba c) =>
        new Godot.Color(c.R, c.G, c.B, c.A);
}
