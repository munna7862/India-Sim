using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WorldState.Domain.Data;
using WorldState.Domain.Map;
using WorldState.Domain.Models;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class MapPerformanceTests
{
    private static string FindMapFile()
    {
        string current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            string candidate = Path.Combine(current, "content", "map", "world_countries.geojson");
            if (File.Exists(candidate)) return candidate;
            string? parent = Directory.GetParent(current)?.FullName;
            if (parent == current || parent == null) break;
            current = parent;
        }
        throw new FileNotFoundException("world_countries.geojson could not be located.");
    }

    [Fact]
    public void RealWorldMap_LoadsAndProjectsAllCountries()
    {
        string mapPath = FindMapFile();
        string json = File.ReadAllText(mapPath);
        var mapData = GeoJsonMapLoader.LoadFromJson(json);

        Assert.NotEmpty(mapData.Countries);
        Assert.True(mapData.TryGet("IND", out var indGeom));
        Assert.NotNull(indGeom);

        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(mapData, session);
        vm.SetViewportSize(1920, 1080);

        var visualStates = vm.GetCountryVisualStates();

        Assert.Equal(mapData.Countries.Count, visualStates.Count);
        foreach (var state in visualStates)
        {
            Assert.NotEmpty(state.ProjectedPolygons);
            Assert.True(state.ProjectedCentroid.X != 0 || state.ProjectedCentroid.Y != 0);
        }
    }

    [Fact]
    public void ProjectionPerformance_MeetsHighFrameRateBudget()
    {
        string mapPath = FindMapFile();
        string json = File.ReadAllText(mapPath);
        var mapData = GeoJsonMapLoader.LoadFromJson(json);
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(mapData, session);
        vm.SetViewportSize(1920, 1080);

        // Warm up cache
        _ = vm.GetCountryVisualStates();

        // 1. Benchmark cached state retrieval (simulating frame draw without camera move)
        var swCache = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            _ = vm.GetCountryVisualStates();
        }
        swCache.Stop();
        // 1000 cached passes should be well under 100ms (< 0.1ms per frame)
        Assert.True(swCache.ElapsedMilliseconds < 100, $"1000 cached passes took {swCache.ElapsedMilliseconds}ms, expected < 100ms");

        // 2. Benchmark dynamic camera recalculation (simulating smooth pan / zoom updates)
        var swDynamic = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            vm.Pan(2.0, 1.0);
            _ = vm.GetCountryVisualStates();
        }
        swDynamic.Stop();
        // 100 full geometry recalculations across all countries should take < 150ms (< 1.5ms per update)
        Assert.True(swDynamic.ElapsedMilliseconds < 150, $"100 dynamic reprojections took {swDynamic.ElapsedMilliseconds}ms, expected < 150ms");
    }

    [Fact]
    public void HitTestPerformance_MeetsResponsiveMouseInputBudget()
    {
        string mapPath = FindMapFile();
        string json = File.ReadAllText(mapPath);
        var mapData = GeoJsonMapLoader.LoadFromJson(json);
        var session = GameSession.CreateNewGame("IND");
        var vm = new WorldMapViewModel(mapData, session);
        vm.SetViewportSize(1920, 1080);

        var swHit = Stopwatch.StartNew();
        int queries = 500;
        for (int i = 0; i < queries; i++)
        {
            double screenX = 100 + (i % 1700);
            double screenY = 100 + ((i * 3) % 800);
            _ = vm.HandleClick(screenX, screenY);
        }
        swHit.Stop();

        // 500 hit test raycasts should be under 50ms (< 0.1ms per mouse movement/click)
        Assert.True(swHit.ElapsedMilliseconds < 50, $"500 hit tests took {swHit.ElapsedMilliseconds}ms, expected < 50ms");
    }

    [Fact]
    public void MapInteractions_PreserveAuthoritativeSimulationInvariants()
    {
        string mapPath = FindMapFile();
        string json = File.ReadAllText(mapPath);
        var mapData = GeoJsonMapLoader.LoadFromJson(json);
        var session = GameSession.CreateNewGame("IND");
        var initialTreasury = session.GetPlayerCountry()!.Treasury;
        var initialDate = session.Clock.CurrentDate;
        var initialEventsCount = session.Events.Count;

        var vm = new WorldMapViewModel(mapData, session);
        vm.SetViewportSize(1920, 1080);

        // Execute myriad UI-only map actions: pan, zoom, click, hover, palette switch
        for (int i = 0; i < 20; i++)
        {
            vm.Pan(15, -10);
            vm.ZoomBy(1.1);
            vm.HandleClick(500, 300);
            vm.HandleHover(600, 400);
            vm.CycleNextPalette();
        }

        // Assert strictly ZERO mutation occurred on domain simulation state
        Assert.Equal(initialTreasury, session.GetPlayerCountry()!.Treasury);
        Assert.Equal(initialDate, session.Clock.CurrentDate);
        Assert.Equal(initialEventsCount, session.Events.Count);
    }
}
