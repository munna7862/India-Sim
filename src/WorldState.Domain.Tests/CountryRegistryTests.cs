using WorldState.Domain.Data;
using WorldState.Domain.Models;
using Xunit;

namespace WorldState.Domain.Tests;

public class CountryRegistryTests
{
    private static CountryDefinition CreateValidDefinition(string id = "IND", string name = "Republic of India") =>
        new(
            Id: id,
            Name: name,
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
            Facilities: new Dictionary<string, int> { ["Farms"] = 50 }
        );

    [Fact]
    public void Register_ValidDefinition_Succeeds()
    {
        var registry = new CountryRegistry();
        var def = CreateValidDefinition();

        registry.Register(def);

        Assert.Equal(1, registry.Count);
        Assert.True(registry.TryGet("IND", out var found));
        Assert.NotNull(found);
        Assert.Equal("Republic of India", found.Name);
    }

    [Fact]
    public void Register_DuplicateId_ThrowsInvalidOperationException()
    {
        var registry = new CountryRegistry();
        registry.Register(CreateValidDefinition("IND", "First"));

        var duplicate = CreateValidDefinition("IND", "Second");

        Assert.Throws<InvalidOperationException>(() => registry.Register(duplicate));
    }

    [Theory]
    [InlineData("in")]       // 2 letters
    [InlineData("india")]    // 5 letters
    [InlineData("123")]      // numbers
    [InlineData("IN1")]      // alphanumeric
    [InlineData("ind")]      // lowercase
    public void Register_InvalidIsoAlpha3Id_ThrowsArgumentException(string invalidId)
    {
        var registry = new CountryRegistry();
        var def = CreateValidDefinition() with { Id = invalidId };

        Assert.Throws<ArgumentException>(() => registry.Register(def));
    }

    [Fact]
    public void Register_NegativeTreasury_ThrowsArgumentException()
    {
        var registry = new CountryRegistry();
        var def = CreateValidDefinition() with { StartingTreasury = -100m };

        Assert.Throws<ArgumentException>(() => registry.Register(def));
    }

    [Fact]
    public void Register_ZeroOrNegativePopulation_ThrowsArgumentException()
    {
        var registry = new CountryRegistry();
        var def = CreateValidDefinition() with { StartingPopulation = 0 };

        Assert.Throws<ArgumentException>(() => registry.Register(def));
    }

    [Fact]
    public void ToCountryState_InitializesCountryWithAll13Ministries()
    {
        var def = CreateValidDefinition();
        var state = def.ToCountryState();

        Assert.Equal("IND", state.Id);
        Assert.Equal("Republic of India", state.Name);
        Assert.Equal(50_000_000_000m, state.Treasury);
        Assert.Equal(13, state.Ministries.Count);
        Assert.All(Enum.GetValues<MinistryType>(), type =>
        {
            Assert.True(state.Ministries.ContainsKey(type));
            Assert.Null(state.Ministries[type].ActiveMinister);
            Assert.Equal(FundingLevel.Medium, state.Ministries[type].Funding);
        });
    }

    [Fact]
    public void RegisterFromJson_ParsesAndRegistersCorrectly()
    {
        var registry = new CountryRegistry();
        string json = """
        {
            "id": "USA",
            "name": "United States of America",
            "capital": "Washington, D.C.",
            "isPlayable": true,
            "startingTreasury": 120000000000.0,
            "startingPopulation": 335000000,
            "startingApproval": 52.0,
            "startingQualityOfLiving": 82.0,
            "startingStability": 75.0,
            "incomeTaxRate": 0.22,
            "corporateTaxRate": 0.21,
            "vatRate": 0.08,
            "facilities": { "Farms": 60 }
        }
        """;

        registry.RegisterFromJson(json);

        Assert.Equal(1, registry.Count);
        var usa = registry.Get("USA");
        Assert.Equal("Washington, D.C.", usa.Capital);
        Assert.True(usa.IsPlayable);
    }
}
