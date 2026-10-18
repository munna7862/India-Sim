using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using WorldState.Domain.Data;
using WorldState.Domain.Models;
using WorldState.Domain.Settings;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class DesktopLayoutAndSettingsTests
{
    [Theory]
    [InlineData(1280, 720, DesktopDisplayTier.Compact)]
    [InlineData(1366, 768, DesktopDisplayTier.Compact)]
    [InlineData(1920, 1080, DesktopDisplayTier.Standard)]
    [InlineData(2560, 1440, DesktopDisplayTier.Large)]
    [InlineData(3840, 2160, DesktopDisplayTier.UltraWide)]
    public void CalculateLayout_AssignsCorrectDisplayTiers(int width, int height, DesktopDisplayTier expectedTier)
    {
        var metrics = DesktopLayoutHelper.CalculateLayout(width, height);

        Assert.Equal(expectedTier, metrics.Tier);
        Assert.Equal(width, metrics.WindowWidth);
        Assert.Equal(height, metrics.WindowHeight);
        Assert.True(metrics.TopBarHeight > 0);
        Assert.True(metrics.DrawerWidth > 300);
        Assert.True(metrics.BaseFontSize >= 12);
        Assert.True(metrics.HeaderFontSize > metrics.BaseFontSize);
    }

    [Fact]
    public void CalculateLayout_ScalesDimensionsWithUiScale()
    {
        var standard1x = DesktopLayoutHelper.CalculateLayout(1920, 1080, 1.0);
        var standard15x = DesktopLayoutHelper.CalculateLayout(1920, 1080, 1.5);

        Assert.Equal(1.5, standard15x.EffectiveScale);
        Assert.True(standard15x.TopBarHeight > standard1x.TopBarHeight);
        Assert.True(standard15x.DrawerWidth > standard1x.DrawerWidth);
        Assert.True(standard15x.BaseFontSize > standard1x.BaseFontSize);
        Assert.True(standard15x.HeaderFontSize > standard1x.HeaderFontSize);
    }

    [Fact]
    public void FilterTasks_AppliesStatusAndTextFiltering()
    {
        var tasks = new List<GameTask>
        {
            new() { Id = "T1", Title = "Expand National Power Grid", Description = "Build 5 Power Plants", Category = TaskCategory.Economic, Status = GameTaskStatus.Active },
            new() { Id = "T2", Title = "Healthcare Modernization", Description = "Construct 10 District Clinics", Category = TaskCategory.Welfare, Status = GameTaskStatus.Completed },
            new() { Id = "T3", Title = "Border Security Initiative", Description = "Recruit 20 Infantry Divisions", Category = TaskCategory.Defense, Status = GameTaskStatus.Failed }
        };

        // Filter by Status
        var active = DesktopLayoutHelper.FilterTasks(tasks, null, "Active").ToList();
        Assert.Single(active);
        Assert.Equal("T1", active[0].Id);

        var completed = DesktopLayoutHelper.FilterTasks(tasks, null, "Completed").ToList();
        Assert.Single(completed);
        Assert.Equal("T2", completed[0].Id);

        var failed = DesktopLayoutHelper.FilterTasks(tasks, null, "Failed").ToList();
        Assert.Single(failed);
        Assert.Equal("T3", failed[0].Id);

        var all = DesktopLayoutHelper.FilterTasks(tasks, null, "All").ToList();
        Assert.Equal(3, all.Count);

        // Filter by Search Query
        var queried = DesktopLayoutHelper.FilterTasks(tasks, "power", "All").ToList();
        Assert.Single(queried);
        Assert.Equal("T1", queried[0].Id);

        var categoryQueried = DesktopLayoutHelper.FilterTasks(tasks, "Welfare", "All").ToList();
        Assert.Single(categoryQueried);
        Assert.Equal("T2", categoryQueried[0].Id);
    }

    [Fact]
    public void FilterNations_ExcludesPlayer_AndFiltersAndSorts()
    {
        var player = new CountryState { Id = "IND", Name = "Republic of India", Treasury = 50_000_000_000m, Population = 1_400_000_000L };
        var nations = new List<CountryState>
        {
            player,
            new() { Id = "USA", Name = "United States of America", Treasury = 150_000_000_000m, Population = 330_000_000L },
            new() { Id = "PAK", Name = "Islamic Republic of Pakistan", Treasury = 10_000_000_000m, Population = 240_000_000L },
            new() { Id = "RUS", Name = "Russian Federation", Treasury = 40_000_000_000m, Population = 144_000_000L }
        };

        // Relations setup
        player.GetRelationsWith("USA").Relations = 35.0;
        player.GetRelationsWith("PAK").Relations = -60.0;
        player.GetRelationsWith("RUS").Relations = 50.0;

        // Base filter: should exclude IND
        var nonPlayer = DesktopLayoutHelper.FilterNations(nations, "IND", player, null, null).ToList();
        Assert.Equal(3, nonPlayer.Count);
        Assert.DoesNotContain(nonPlayer, n => n.Id == "IND");

        // Query filter
        var searchResult = DesktopLayoutHelper.FilterNations(nations, "IND", player, "pakistan", null).ToList();
        Assert.Single(searchResult);
        Assert.Equal("PAK", searchResult[0].Id);

        // Sort by Relations (Descending)
        var sortedByRel = DesktopLayoutHelper.FilterNations(nations, "IND", player, null, "relations").ToList();
        Assert.Equal("RUS", sortedByRel[0].Id); // 50.0
        Assert.Equal("USA", sortedByRel[1].Id); // 35.0
        Assert.Equal("PAK", sortedByRel[2].Id); // -60.0

        // Sort by Treasury (Descending)
        var sortedByTreasury = DesktopLayoutHelper.FilterNations(nations, "IND", player, null, "treasury").ToList();
        Assert.Equal("USA", sortedByTreasury[0].Id);
        Assert.Equal("RUS", sortedByTreasury[1].Id);
        Assert.Equal("PAK", sortedByTreasury[2].Id);

        // Sort by Population (Descending)
        var sortedByPop = DesktopLayoutHelper.FilterNations(nations, "IND", player, null, "population").ToList();
        Assert.Equal("USA", sortedByPop[0].Id);
        Assert.Equal("PAK", sortedByPop[1].Id);
        Assert.Equal("RUS", sortedByPop[2].Id);
    }

    [Fact]
    public void UserSettings_IncludesMapPaletteAndUiScale_AndPersistsCleanly()
    {
        var settings = new UserSettings(
            MasterVolume: 0.8,
            MusicVolume: 0.6,
            SfxVolume: 0.9,
            DisplayMode: WindowMode.BorderlessFullscreen,
            ResolutionWidth: 2560,
            ResolutionHeight: 1440,
            VSync: true,
            Language: "en",
            AutosaveEnabled: true,
            AutosaveIntervalMinutes: 5,
            MapPalette: "StrategicNavy",
            UiScale: 1.25
        );

        Assert.Equal("StrategicNavy", settings.MapPalette);
        Assert.Equal(1.25, settings.UiScale);
        Assert.Contains(UserSettings.ResolutionPresets, p => p.Width == 2560 && p.Height == 1440);
        Assert.Contains(1.25, UserSettings.UiScalePresets);

        var service = new SettingsService();
        string json = service.SerializeSettings(settings);
        var restored = service.DeserializeSettings(json);

        Assert.NotNull(restored);
        Assert.Equal(settings.MasterVolume, restored.MasterVolume);
        Assert.Equal(WindowMode.BorderlessFullscreen, restored.DisplayMode);
        Assert.Equal(2560, restored.ResolutionWidth);
        Assert.Equal(1440, restored.ResolutionHeight);
        Assert.Equal("StrategicNavy", restored.MapPalette);
        Assert.Equal(1.25, restored.UiScale);
        Assert.True(restored.AutosaveEnabled);
    }
}
