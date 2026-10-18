using WorldState.Domain.Navigation;
using WorldState.Domain.Settings;
using Xunit;

namespace WorldState.Domain.Tests;

public class NavigationAndSettingsTests
{
    [Fact]
    public void NavigationService_NavigatesAndPushesHistory()
    {
        // Arrange
        var nav = new NavigationService();

        // Act & Assert Initial
        Assert.Equal(ScreenRoute.MainMenu, nav.CurrentRoute);
        Assert.False(nav.CanNavigateBack);

        // Navigate forward
        nav.NavigateTo(ScreenRoute.CountrySelect);
        Assert.Equal(ScreenRoute.CountrySelect, nav.CurrentRoute);
        Assert.True(nav.CanNavigateBack);

        nav.NavigateTo(ScreenRoute.WorldMap);
        Assert.Equal(ScreenRoute.WorldMap, nav.CurrentRoute);

        nav.NavigateTo(ScreenRoute.GovernmentCabinet);
        Assert.Equal(ScreenRoute.GovernmentCabinet, nav.CurrentRoute);

        // Step back (Escape key behavior)
        Assert.True(nav.NavigateBack());
        Assert.Equal(ScreenRoute.WorldMap, nav.CurrentRoute);

        Assert.True(nav.NavigateBack());
        Assert.Equal(ScreenRoute.CountrySelect, nav.CurrentRoute);

        Assert.True(nav.NavigateBack());
        Assert.Equal(ScreenRoute.MainMenu, nav.CurrentRoute);

        Assert.False(nav.NavigateBack());
    }

    [Fact]
    public void NavigationService_ResetTo_ClearsHistoryStack()
    {
        // Arrange
        var nav = new NavigationService();
        nav.NavigateTo(ScreenRoute.CountrySelect);
        nav.NavigateTo(ScreenRoute.WorldMap);

        // Act
        nav.ResetTo(ScreenRoute.MainMenu);

        // Assert
        Assert.Equal(ScreenRoute.MainMenu, nav.CurrentRoute);
        Assert.False(nav.CanNavigateBack);
    }

    [Fact]
    public void SettingsService_SerializesAndDeserializesRoundTrip()
    {
        // Arrange
        var service = new SettingsService();
        var custom = new UserSettings(
            MasterVolume: 0.5,
            MusicVolume: 0.4,
            SfxVolume: 0.8,
            DisplayMode: WindowMode.BorderlessFullscreen,
            ResolutionWidth: 2560,
            ResolutionHeight: 1440,
            VSync: false,
            Language: "hi",
            AutosaveEnabled: true,
            AutosaveIntervalMinutes: 10
        );

        // Act
        string json = service.SerializeSettings(custom);
        var restored = service.DeserializeSettings(json);

        // Assert
        Assert.Equal(custom.MasterVolume, restored.MasterVolume);
        Assert.Equal(custom.MusicVolume, restored.MusicVolume);
        Assert.Equal(custom.SfxVolume, restored.SfxVolume);
        Assert.Equal(custom.DisplayMode, restored.DisplayMode);
        Assert.Equal(custom.ResolutionWidth, restored.ResolutionWidth);
        Assert.Equal(custom.ResolutionHeight, restored.ResolutionHeight);
        Assert.Equal(custom.VSync, restored.VSync);
        Assert.Equal(custom.Language, restored.Language);
        Assert.Equal(custom.AutosaveIntervalMinutes, restored.AutosaveIntervalMinutes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{ invalid json }")]
    public void SettingsService_WithCorruptedData_GracefullyFallsBackToDefaults(string invalidJson)
    {
        // Arrange
        var service = new SettingsService();

        // Act
        var settings = service.DeserializeSettings(invalidJson);

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(0.8, settings.MasterVolume);
        Assert.Equal(1920, settings.ResolutionWidth);
        Assert.Equal(1080, settings.ResolutionHeight);
    }
}
