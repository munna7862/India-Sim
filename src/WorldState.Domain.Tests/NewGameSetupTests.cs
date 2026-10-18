using System.Text.Json;
using WorldState.Domain.Data;
using WorldState.Domain.Models;
using WorldState.Domain.Scenarios;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class NewGameSetupTests
{
    private readonly CountryRegistry _registry;
    private readonly ScenarioDefinition _scenario;

    public NewGameSetupTests()
    {
        _registry = new CountryRegistry();
        string[] countryIds = { "IND", "USA", "CHN", "RUS", "GBR", "PAK" };
        foreach (var id in countryIds)
        {
            _registry.Register(new CountryDefinition(
                Id: id,
                Name: $"Nation {id}",
                Capital: $"Capital {id}",
                IsPlayable: true,
                StartingTreasury: 50_000_000_000m,
                StartingPopulation: 100_000_000L,
                StartingApproval: 60.0,
                StartingQualityOfLiving: 50.0,
                StartingStability: 70.0,
                IncomeTaxRate: 0.20,
                CorporateTaxRate: 0.25,
                VatRate: 0.15,
                Facilities: new Dictionary<string, int>
                {
                    ["Farms"] = 50,
                    ["Mines"] = 25,
                    ["PowerPlants"] = 30,
                    ["Factories"] = 40
                }
            ));
        }

        _scenario = ScenarioDefinition.CreateDefault();
    }

    [Fact]
    public void InitializeNewGame_WithDefaultConfig_ReturnsValidGameSession()
    {
        var config = new NewGameConfig();
        var session = GameInitializer.InitializeNewGame(config, _registry, _scenario);

        Assert.NotNull(session);
        Assert.Equal("IND", session.PlayerCountryId);
        Assert.Equal("modern_2026", session.ScenarioId);
        Assert.Equal(42, session.Seed);
        Assert.Equal(new DateOnly(2026, 1, 1), session.Clock.CurrentDate);
        Assert.Equal(ClockSpeed.Paused, session.Clock.Speed);

        var playerCountry = session.GetPlayerCountry();
        Assert.NotNull(playerCountry);
        Assert.Equal("IND", playerCountry!.Id);
        Assert.Equal(13, playerCountry.Ministries.Count);
        Assert.Equal(6, session.Countries.Count);
    }

    [Theory]
    [InlineData("IND")]
    [InlineData("USA")]
    [InlineData("CHN")]
    [InlineData("RUS")]
    [InlineData("GBR")]
    [InlineData("PAK")]
    public void InitializeNewGame_SupportsAllPlayableCountries(string countryId)
    {
        var config = new NewGameConfig(PlayerCountryId: countryId);
        var session = GameInitializer.InitializeNewGame(config, _registry, _scenario);

        Assert.Equal(countryId, session.PlayerCountryId);
        var player = session.GetPlayerCountry();
        Assert.NotNull(player);
        Assert.Equal(countryId, player!.Id);
        Assert.Equal(13, player.Ministries.Count);
    }

    [Fact]
    public void InitializeNewGame_DeterministicSeed_ProducesIdenticalSessionId()
    {
        var config1 = new NewGameConfig(Seed: 12345);
        var config2 = new NewGameConfig(Seed: 12345);

        var session1 = GameInitializer.InitializeNewGame(config1, _registry, _scenario);
        var session2 = GameInitializer.InitializeNewGame(config2, _registry, _scenario);

        Assert.Equal(session1.SessionId, session2.SessionId);
    }

    [Fact]
    public void InitializeNewGame_WithDifferentSeed_ProducesDifferentSessionId()
    {
        var config1 = new NewGameConfig(Seed: 100);
        var config2 = new NewGameConfig(Seed: 200);

        var session1 = GameInitializer.InitializeNewGame(config1, _registry, _scenario);
        var session2 = GameInitializer.InitializeNewGame(config2, _registry, _scenario);

        Assert.NotEqual(session1.SessionId, session2.SessionId);
    }

    [Fact]
    public void InitializeNewGame_WithNonPlayableCountry_ThrowsInvalidOperationException()
    {
        _registry.Register(new CountryDefinition(
            Id: "LKD",
            Name: "Locked Land",
            Capital: "Locktown",
            IsPlayable: false,
            StartingTreasury: 1_000m,
            StartingPopulation: 1_000L,
            StartingApproval: 50.0,
            StartingQualityOfLiving: 50.0,
            StartingStability: 50.0,
            IncomeTaxRate: 0.1,
            CorporateTaxRate: 0.1,
            VatRate: 0.1,
            Facilities: new Dictionary<string, int>()
        ));

        var config = new NewGameConfig(PlayerCountryId: "LKD");

        Assert.Throws<InvalidOperationException>(() =>
            GameInitializer.InitializeNewGame(config, _registry, _scenario));
    }

    [Fact]
    public void InitializeNewGame_WithUnknownCountry_ThrowsInvalidOperationException()
    {
        var config = new NewGameConfig(PlayerCountryId: "XYZ");

        Assert.Throws<InvalidOperationException>(() =>
            GameInitializer.InitializeNewGame(config, _registry, _scenario));
    }

    [Fact]
    public void NewGameSetupViewModel_StartGame_ProducesExpectedSession()
    {
        var vm = new NewGameSetupViewModel(_registry, null, _scenario)
        {
            Seed = 999
        };

        vm.CountrySelection.SelectCountry("USA");
        var session = vm.StartGame();

        Assert.NotNull(session);
        Assert.Equal("USA", session.PlayerCountryId);
        Assert.Equal(999, session.Seed);
    }

    [Fact]
    public void SaveLoadRoundTrip_WithInitializedWorldState_PreservesAllCountriesAndState()
    {
        var config = new NewGameConfig(PlayerCountryId: "IND", Seed: 777);
        var originalSession = GameInitializer.InitializeNewGame(config, _registry, _scenario);

        string json = JsonSerializer.Serialize(originalSession);
        var restoredSession = JsonSerializer.Deserialize<GameSession>(json);

        Assert.NotNull(restoredSession);
        Assert.Equal(originalSession.SessionId, restoredSession!.SessionId);
        Assert.Equal(originalSession.PlayerCountryId, restoredSession.PlayerCountryId);
        Assert.Equal(originalSession.Seed, restoredSession.Seed);
        Assert.Equal(originalSession.ScenarioId, restoredSession.ScenarioId);
        Assert.Equal(originalSession.Clock.CurrentDate, restoredSession.Clock.CurrentDate);
        Assert.Equal(originalSession.Countries.Count, restoredSession.Countries.Count);

        foreach (var kvp in originalSession.Countries)
        {
            Assert.True(restoredSession.Countries.ContainsKey(kvp.Key));
            var orig = kvp.Value;
            var rest = restoredSession.Countries[kvp.Key];
            Assert.Equal(orig.Treasury, rest.Treasury);
            Assert.Equal(orig.Population, rest.Population);
            Assert.Equal(orig.ApprovalRating, rest.ApprovalRating);
            Assert.Equal(orig.Ministries.Count, rest.Ministries.Count);
        }
    }
}
