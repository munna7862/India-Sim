using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorldState.Domain.Data;
using WorldState.Domain.Map;
using WorldState.Domain.Models;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class WorldMapViewModelTests
{
    private readonly WorldMapData _mapData;
    private readonly CountryRegistry _registry;

    public WorldMapViewModelTests()
    {
        _mapData = new WorldMapData();
        _mapData.AddCountry(new CountryGeometry(
            "IND",
            "India",
            new[] { new MapPolygon(new List<MapPoint> { new(70, 10), new(85, 10), new(85, 30), new(70, 30), new(70, 10) }) },
            new MapPoint(77.5, 20.0)
        ));
        _mapData.AddCountry(new CountryGeometry(
            "USA",
            "United States",
            new[] { new MapPolygon(new List<MapPoint> { new(-125, 25), new(-65, 25), new(-65, 50), new(-125, 50), new(-125, 25) }) },
            new MapPoint(-95.0, 37.5)
        ));
        _mapData.AddCountry(new CountryGeometry(
            "PAK",
            "Pakistan",
            new[] { new MapPolygon(new List<MapPoint> { new(60, 24), new(75, 24), new(75, 37), new(60, 37), new(60, 24) }) },
            new MapPoint(67.5, 30.0)
        ));

        _registry = new CountryRegistry();
        _registry.Register(new CountryDefinition("IND", "Republic of India", "New Delhi", true, 50_000_000_000m, 1_400_000_000L, 65.0, 55.0, 70.0, 0.2, 0.25, 0.15, new Dictionary<string, int>()));
        _registry.Register(new CountryDefinition("USA", "United States of America", "Washington, D.C.", true, 100_000_000_000m, 335_000_000L, 50.0, 75.0, 65.0, 0.22, 0.21, 0.0, new Dictionary<string, int>()));
        _registry.Register(new CountryDefinition("PAK", "Islamic Republic of Pakistan", "Islamabad", true, 10_000_000_000m, 240_000_000L, 45.0, 40.0, 50.0, 0.15, 0.20, 0.18, new Dictionary<string, int>()));
    }

    [Fact]
    public void Constructor_InitializesWithPlayerSelectionAndDefaultPalette()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);

        Assert.Equal("IND", vm.SelectedCountryId);
        Assert.Null(vm.HoveredCountryId);
        Assert.Equal(MapThemeRegistry.DefaultPaletteName, vm.ActivePalette.Name);
        Assert.Equal(1.0, vm.Zoom);
        Assert.Equal(0.0, vm.PanX);
        Assert.Equal(0.0, vm.PanY);
    }

    [Fact]
    public void SetViewportSize_UpdatesMetrics_AndFiresStateChanged()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);
        bool fired = false;
        vm.OnStateChanged += () => fired = true;

        vm.SetViewportSize(1920, 1080);

        Assert.True(fired);
        Assert.Equal(1920.0, vm.ViewportWidth);
        Assert.Equal(1080.0, vm.ViewportHeight);
    }

    [Fact]
    public void ZoomBy_ClampsToMinAndMaxProjectionZoom()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);

        // Zoom way out
        vm.ZoomBy(0.01);
        Assert.Equal(MapProjection.MinZoom, vm.Zoom);

        // Zoom way in
        vm.ZoomBy(100.0);
        Assert.Equal(MapProjection.MaxZoom, vm.Zoom);
    }

    [Fact]
    public void ZoomBy_WithCursor_AdjustsPanFocalPoint()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);
        vm.SetViewportSize(1000, 1000);

        // Zoom in towards cursor at (800, 800)
        vm.ZoomBy(2.0, cursorX: 800, cursorY: 800);

        Assert.Equal(2.0, vm.Zoom);
        Assert.True(vm.PanX < 0.0, "PanX should shift left when zooming into right-side point");
        Assert.True(vm.PanY < 0.0, "PanY should shift up when zooming into bottom-side point");
    }

    [Fact]
    public void Pan_AppliesClampedDelta()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);
        vm.SetViewportSize(1000, 1000);

        vm.Pan(100, 50);

        Assert.Equal(100.0, vm.PanX);
        Assert.Equal(50.0, vm.PanY);

        // Huge pan should clamp within soft bounds
        vm.Pan(50_000, 50_000);
        double maxExpected = (1000.0 * 1.0) / 1.5;
        Assert.Equal(maxExpected, vm.PanX);
        Assert.Equal(maxExpected, vm.PanY);
    }

    [Fact]
    public void ResetCamera_RestoresDefaultZoomAndPan()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);

        vm.ZoomBy(3.0);
        vm.Pan(200, -150);
        vm.ResetCamera();

        Assert.Equal(1.0, vm.Zoom);
        Assert.Equal(0.0, vm.PanX);
        Assert.Equal(0.0, vm.PanY);
    }

    [Fact]
    public void FocusCountry_CentersCentroidAndSelectsCountry()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);
        vm.SetViewportSize(1000, 500);

        bool focused = vm.FocusCountry("USA");

        Assert.True(focused);
        Assert.Equal("USA", vm.SelectedCountryId);

        // Non-existent country returns false
        Assert.False(vm.FocusCountry("NON_EXISTENT"));
    }

    [Fact]
    public void HandleClick_AndHover_UpdatesSelectionAndHover()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);
        vm.SetViewportSize(1000, 500);

        // Project center of India
        var (screenX, screenY) = MapProjection.Project(new MapPoint(77.5, 20.0), 1000, 500, 1.0, 0.0, 0.0);

        string? clicked = vm.HandleClick(screenX, screenY);
        Assert.Equal("IND", clicked);
        Assert.Equal("IND", vm.SelectedCountryId);

        // Hover over USA
        var (usaX, usaY) = MapProjection.Project(new MapPoint(-95.0, 37.5), 1000, 500, 1.0, 0.0, 0.0);
        vm.HandleHover(usaX, usaY);
        Assert.Equal("USA", vm.HoveredCountryId);

        // Hover over ocean
        vm.HandleHover(10, 10);
        Assert.Null(vm.HoveredCountryId);
    }

    [Fact]
    public void MapPalettes_CanSwitchAndCycleCleanly()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);

        Assert.Equal("TacticalDark", vm.ActivePalette.Name);

        vm.SetPalette("StrategicNavy");
        Assert.Equal("StrategicNavy", vm.ActivePalette.Name);

        vm.CycleNextPalette();
        Assert.Equal("AmberCartographic", vm.ActivePalette.Name);

        vm.CycleNextPalette();
        Assert.Equal("TacticalDark", vm.ActivePalette.Name);

        // Unknown palette falls back to TacticalDark
        vm.SetPalette("InvalidPaletteName");
        Assert.Equal("TacticalDark", vm.ActivePalette.Name);
    }

    [Fact]
    public void MapThemeRegistry_GetCountryColors_ResolvesCorrectStances()
    {
        var palette = MapThemeRegistry.TacticalDark;

        // Player country
        var (playerFill, playerBorder, playerWidth) = MapThemeRegistry.GetCountryColors(
            palette, isPlayer: true, isSelected: false, isHovered: false);
        Assert.Equal(palette.PlayerFill, playerFill);
        Assert.Equal(palette.PlayerBorder, playerBorder);
        Assert.Equal(1.4f, playerWidth);

        // Player selected
        var (_, selBorder, selWidth) = MapThemeRegistry.GetCountryColors(
            palette, isPlayer: true, isSelected: true, isHovered: false);
        Assert.Equal(palette.SelectionBorder, selBorder);
        Assert.Equal(3.0f, selWidth);

        // At War
        var (warFill, warBorder, _) = MapThemeRegistry.GetCountryColors(
            palette, isPlayer: false, isSelected: false, isHovered: false, isAtWar: true);
        Assert.Equal(palette.HostileFill, warFill);
        Assert.Equal(palette.HostileBorder, warBorder);

        // Hostile relations (<= -50)
        var (hostileFill, _, _) = MapThemeRegistry.GetCountryColors(
            palette, isPlayer: false, isSelected: false, isHovered: false, relationScore: -60);
        Assert.Equal(palette.HostileFill, hostileFill);

        // Non-aggression pact
        var (napFill, napBorder, _) = MapThemeRegistry.GetCountryColors(
            palette, isPlayer: false, isSelected: false, isHovered: false, hasNonAggressionPact: true);
        Assert.Equal(palette.AlliedFill, napFill);
        Assert.Equal(palette.AlliedBorder, napBorder);

        // Friendly relations (>= +50)
        var (friendlyFill, friendlyBorder, _) = MapThemeRegistry.GetCountryColors(
            palette, isPlayer: false, isSelected: false, isHovered: false, relationScore: 65);
        Assert.Equal(palette.FriendlyFill, friendlyFill);
        Assert.Equal(palette.FriendlyBorder, friendlyBorder);

        // Neutral relations (-49 to +49)
        var (neutralFill, neutralBorder, _) = MapThemeRegistry.GetCountryColors(
            palette, isPlayer: false, isSelected: false, isHovered: false, relationScore: 10);
        Assert.Equal(palette.NeutralFill, neutralFill);
        Assert.Equal(palette.NeutralBorder, neutralBorder);

        // Hovered border width
        var (_, hovBorder, hovWidth) = MapThemeRegistry.GetCountryColors(
            palette, isPlayer: false, isSelected: false, isHovered: true);
        Assert.Equal(palette.HoverBorder, hovBorder);
        Assert.Equal(2.4f, hovWidth);
    }

    [Fact]
    public void GetCountryVisualStates_GeneratesCompleteVisualList()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);
        vm.SetViewportSize(1280, 720);

        var states = vm.GetCountryVisualStates();

        Assert.Equal(3, states.Count);

        var indState = states.First(s => s.CountryId == "IND");
        Assert.True(indState.IsPlayer);
        Assert.True(indState.IsSelected);
        Assert.Equal("Republic of India", indState.Name);
        Assert.Equal("Sovereign Homeland", indState.StanceText);
        Assert.NotEmpty(indState.ProjectedPolygons);

        var pakState = states.First(s => s.CountryId == "PAK");
        Assert.False(pakState.IsPlayer);
        Assert.False(pakState.IsSelected);
        Assert.Equal("Islamic Republic of Pakistan", pakState.Name);
        Assert.Contains("Hostile", pakState.StanceText);
    }

    [Fact]
    public void GetHoverTooltipText_ReturnsRichGeopoliticalDetails()
    {
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(_mapData, session, _registry);

        // Nothing hovered
        Assert.Null(vm.GetHoverTooltipText());

        // Player hovered
        vm.SetHoveredCountry("IND");
        string? indTooltip = vm.GetHoverTooltipText();
        Assert.NotNull(indTooltip);
        Assert.Contains("Sovereign Homeland", indTooltip);
        Assert.Contains("New Delhi", indTooltip);
        Assert.Contains("Approval:", indTooltip);

        // Foreign hovered
        vm.SetHoveredCountry("USA");
        string? usaTooltip = vm.GetHoverTooltipText();
        Assert.NotNull(usaTooltip);
        Assert.Contains("Washington, D.C.", usaTooltip);
        Assert.Contains("Click to open Foreign Affairs", usaTooltip);
    }

    [Fact]
    public void ColorRgba_ParsesHexAndFormatsCorrectly()
    {
        var c1 = ColorRgba.FromHex("#FF9933");
        Assert.InRange(c1.R, 0.99f, 1.01f);
        Assert.InRange(c1.G, 0.59f, 0.61f);
        Assert.InRange(c1.B, 0.19f, 0.21f);
        Assert.Equal(1.0f, c1.A);
        Assert.Equal("#FF9933", c1.ToHex());

        var c2 = ColorRgba.FromHex("0B0F1780"); // 8-char hex with alpha
        Assert.Equal(8, c2.ToHex(true).Length - 1);

        // Empty/invalid string defaults to White
        var fallback = ColorRgba.FromHex("invalid");
        Assert.Equal(ColorRgba.White, fallback);
    }
}
