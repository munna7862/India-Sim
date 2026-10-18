using WorldState.Domain.Data;
using WorldState.Domain.Map;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class CountrySelectionViewModelTests
{
    private readonly CountryRegistry _registry;
    private readonly WorldMapData _mapData;

    public CountrySelectionViewModelTests()
    {
        _registry = new CountryRegistry();
        _registry.Register(new CountryDefinition(
            Id: "IND",
            Name: "Republic of India",
            Capital: "New Delhi",
            IsPlayable: true,
            StartingTreasury: 50_000_000_000m,
            StartingPopulation: 1_400_000_000L,
            StartingApproval: 65.0,
            StartingQualityOfLiving: 55.0,
            StartingStability: 70.0,
            IncomeTaxRate: 0.20,
            CorporateTaxRate: 0.25,
            VatRate: 0.15,
            Facilities: new Dictionary<string, int>()
        ));
        _registry.Register(new CountryDefinition(
            Id: "USA",
            Name: "United States of America",
            Capital: "Washington, D.C.",
            IsPlayable: true,
            StartingTreasury: 100_000_000_000m,
            StartingPopulation: 335_000_000L,
            StartingApproval: 50.0,
            StartingQualityOfLiving: 75.0,
            StartingStability: 65.0,
            IncomeTaxRate: 0.22,
            CorporateTaxRate: 0.21,
            VatRate: 0.0,
            Facilities: new Dictionary<string, int>()
        ));
        _registry.Register(new CountryDefinition(
            Id: "VAT",
            Name: "Vatican City",
            Capital: "Vatican City",
            IsPlayable: false, // Non-playable country
            StartingTreasury: 1_000_000m,
            StartingPopulation: 800L,
            StartingApproval: 90.0,
            StartingQualityOfLiving: 90.0,
            StartingStability: 99.0,
            IncomeTaxRate: 0.0,
            CorporateTaxRate: 0.0,
            VatRate: 0.0,
            Facilities: new Dictionary<string, int>()
        ));

        _mapData = new WorldMapData();
        _mapData.AddCountry(new CountryGeometry(
            "IND",
            "India",
            new[] { new MapPolygon(new List<MapPoint> { new(70, 10), new(85, 10), new(85, 30), new(70, 30), new(70, 10) }) },
            new MapPoint(77.5, 20.0)
        ));
    }

    [Fact]
    public void Constructor_DefaultsToIndia_WhenPresent()
    {
        var vm = new CountrySelectionViewModel(_registry, _mapData);

        Assert.NotNull(vm.SelectedCountry);
        Assert.Equal("IND", vm.SelectedCountry!.Id);
        Assert.True(vm.CanStartGame);
    }

    [Fact]
    public void SetSearchQuery_FiltersCountriesByNameAndCode()
    {
        var vm = new CountrySelectionViewModel(_registry, _mapData);

        vm.SetSearchQuery("united");
        Assert.Single(vm.FilteredCountries);
        Assert.Equal("USA", vm.FilteredCountries[0].Id);

        vm.SetSearchQuery("IND");
        Assert.Single(vm.FilteredCountries);
        Assert.Equal("IND", vm.FilteredCountries[0].Id);

        vm.SetSearchQuery("");
        Assert.Equal(3, vm.FilteredCountries.Count);
    }

    [Fact]
    public void SelectCountry_WithValidId_UpdatesSelectionAndStatus()
    {
        var vm = new CountrySelectionViewModel(_registry, _mapData);

        bool selected = vm.SelectCountry("USA");
        Assert.True(selected);
        Assert.Equal("USA", vm.SelectedCountry?.Id);
        Assert.True(vm.CanStartGame);
    }

    [Fact]
    public void SelectCountry_WithNonPlayableCountry_MarksCanStartGameFalse()
    {
        var vm = new CountrySelectionViewModel(_registry, _mapData);

        bool selected = vm.SelectCountry("VAT");
        Assert.True(selected);
        Assert.Equal("VAT", vm.SelectedCountry?.Id);
        Assert.False(vm.CanStartGame);
    }

    [Fact]
    public void SelectCountry_WithInvalidId_ReturnsFalseWithoutChangingSelection()
    {
        var vm = new CountrySelectionViewModel(_registry, _mapData);

        string originalId = vm.SelectedCountry!.Id;
        bool selected = vm.SelectCountry("NONEXISTENT");

        Assert.False(selected);
        Assert.Equal(originalId, vm.SelectedCountry!.Id);
    }

    [Fact]
    public void HandleMapClick_WhenClickingWithinCountry_SelectsCountry()
    {
        var vm = new CountrySelectionViewModel(_registry, _mapData);
        vm.SelectCountry("USA"); // Switch away from IND first

        // Project IND center (77.5, 20.0) into viewport
        var (screenX, screenY) = MapProjection.Project(new MapPoint(77.5, 20.0), 1000, 500, vm.Zoom, vm.PanX, vm.PanY);

        bool clicked = vm.HandleMapClick(screenX, screenY, 1000, 500);

        Assert.True(clicked);
        Assert.Equal("IND", vm.SelectedCountry?.Id);
    }

    [Fact]
    public void HandleMapHover_WhenHoveringOverCountry_UpdatesHoverState()
    {
        var vm = new CountrySelectionViewModel(_registry, _mapData);

        var (screenX, screenY) = MapProjection.Project(new MapPoint(77.5, 20.0), 1000, 500, vm.Zoom, vm.PanX, vm.PanY);
        vm.HandleMapHover(screenX, screenY, 1000, 500);

        Assert.Equal("IND", vm.HoveredCountryId);

        // Hover over ocean
        var (oceanX, oceanY) = MapProjection.Project(new MapPoint(0.0, 0.0), 1000, 500, vm.Zoom, vm.PanX, vm.PanY);
        vm.HandleMapHover(oceanX, oceanY, 1000, 500);

        Assert.Null(vm.HoveredCountryId);
    }

    [Fact]
    public void PanAndZoom_ManipulatesViewportCorrectly()
    {
        var vm = new CountrySelectionViewModel(_registry, _mapData);

        vm.Pan(50, -25);
        Assert.Equal(50.0, vm.PanX);
        Assert.Equal(-25.0, vm.PanY);

        vm.ZoomBy(2.0);
        Assert.Equal(2.0, vm.Zoom);

        vm.ResetViewport();
        Assert.Equal(0.0, vm.PanX);
        Assert.Equal(0.0, vm.PanY);
        Assert.Equal(1.0, vm.Zoom);
    }
}
