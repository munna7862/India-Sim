using System.Text.Json;
using WorldState.Domain.Commands;
using WorldState.Domain.Events;
using WorldState.Domain.Models;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class DomesticEventsAndStabilityTests
{
    [Fact]
    public void DomesticEventRegistry_LoadsEventsFromJson_Accurately()
    {
        // Arrange
        string eventsPath = Path.Combine(AppContext.BaseDirectory, "../../../../../content/events/domestic_events.json");
        Assert.True(File.Exists(eventsPath), $"domestic_events.json not found at {eventsPath}");
        string json = File.ReadAllText(eventsPath);

        var registry = new DomesticEventRegistry();

        // Act
        registry.LoadFromJson(json);

        // Assert
        Assert.Equal(5, registry.Events.Count);
        Assert.True(registry.TryGet("EVT_FLOOD_RELIEF", out var floodEvt));
        Assert.NotNull(floodEvt);
        Assert.Equal("Emergency", floodEvt.Category);
        Assert.Equal(3, floodEvt.Choices.Count);
        Assert.Equal(500_000_000m, floodEvt.Choices[0].TreasuryCost);
    }

    [Fact]
    public void SimulationEngine_UpdateNationalIndicators_ComputesQualityOfLivingFromFacilitiesAndFunding()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        double startingQol = country.QualityOfLiving;

        // Upgrade hospitals and healthcare funding
        country.Facilities["Hospitals"] = 30;
        country.Ministries[MinistryType.Health] = new MinistryState(
            MinistryType.Health,
            new Minister("M_EXP", "Top Doctor", 0.95, 300_000m),
            FundingLevel.Maximum
        );

        // Act: Run indicator update
        engine.UpdateNationalIndicators(country);

        // Assert: Quality of living should improve
        Assert.True(country.QualityOfLiving >= startingQol, "Quality of living should increase with upgraded healthcare.");
    }

    [Fact]
    public void SimulationEngine_UpdateNationalIndicators_PenalizesZeroFundedMinistries()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        // Cut health and education funding to zero
        country.Ministries[MinistryType.Health] = new MinistryState(MinistryType.Health, null, FundingLevel.Zero);
        country.Ministries[MinistryType.Education] = new MinistryState(MinistryType.Education, null, FundingLevel.Zero);

        double startingApproval = country.ApprovalRating;

        // Act
        engine.UpdateNationalIndicators(country);

        // Assert: Approval should decline from strikes
        Assert.True(country.ApprovalRating < startingApproval);
    }

    [Fact]
    public void ResolveEventChoice_ValidChoice_MutatesTreasuryAndIndicators_AndClearsPrompt()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        decimal startingTreasury = country.Treasury;
        double startingApproval = country.ApprovalRating;
        double startingStability = country.Stability;

        var catalog = DomesticEventRegistry.CreateDefaultCatalog();
        catalog.TryGet("EVT_FLOOD_RELIEF", out var floodEvt);
        Assert.NotNull(floodEvt);

        session.PendingEvent = new ActiveEventPrompt
        {
            EventId = floodEvt.Id,
            CountryId = country.Id,
            Date = session.Clock.CurrentDate,
            Title = floodEvt.Title,
            Description = floodEvt.Description,
            Choices = floodEvt.Choices.ToList()
        };

        var command = new ResolveEventChoiceCommand("IND", "EVT_FLOOD_RELIEF", "CH_FLOOD_RELIEF_FULL");

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(session.PendingEvent);
        Assert.Equal(startingTreasury - 500_000_000m, country.Treasury);
        Assert.Equal(startingApproval + 6.0, country.ApprovalRating);
        Assert.Equal(startingStability + 3.0, country.Stability);
        Assert.Contains("EVT_FLOOD_RELIEF", session.EventCooldowns.Keys);
        Assert.Contains(session.Events, e => e.Category == EventCategory.Emergency && e.Title.Contains("Resolved"));
    }

    [Fact]
    public void ResolveEventChoice_WithInsufficientFunds_FailsValidationWithoutMutatingState()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        country.Treasury = 50_000_000m; // Only $50M

        var catalog = DomesticEventRegistry.CreateDefaultCatalog();
        catalog.TryGet("EVT_FLOOD_RELIEF", out var floodEvt);

        session.PendingEvent = new ActiveEventPrompt
        {
            EventId = floodEvt!.Id,
            CountryId = country.Id,
            Date = session.Clock.CurrentDate,
            Title = floodEvt.Title,
            Description = floodEvt.Description,
            Choices = floodEvt.Choices.ToList()
        };

        var command = new ResolveEventChoiceCommand("IND", "EVT_FLOOD_RELIEF", "CH_FLOOD_RELIEF_FULL"); // Cost: $500M

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("InsufficientFunds", result.ErrorCode);
        Assert.NotNull(session.PendingEvent); // Event remains pending
        Assert.Equal(50_000_000m, country.Treasury);
    }

    [Fact]
    public void ResolveEventChoice_WhenNoActivePendingEvent_FailsSafely()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        session.PendingEvent = null;

        var command = new ResolveEventChoiceCommand("IND", "EVT_FLOOD_RELIEF", "CH_FLOOD_RELIEF_FULL");

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("NoPendingEvent", result.ErrorCode);
    }

    [Fact]
    public void LaunchAntiCorruptionCampaign_ReducesCorruption_AndEnforcesCooldown()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        country.CorruptionIndex = 40.0;
        decimal startingTreasury = country.Treasury;

        var command = new LaunchAntiCorruptionCampaignCommand("IND", 500_000_000m);

        // Act 1: Launch campaign
        var firstResult = engine.Dispatch(command);

        // Assert 1
        Assert.True(firstResult.IsSuccess);
        Assert.Equal(34.0, country.CorruptionIndex);
        Assert.Equal(startingTreasury - 500_000_000m, country.Treasury);
        Assert.Contains(session.Events, e => e.Category == EventCategory.Government && e.Title == "Anti-Corruption Campaign Launched");

        // Act 2: Immediate second attempt while on cooldown
        var secondResult = engine.Dispatch(command);

        // Assert 2
        Assert.False(secondResult.IsSuccess);
        Assert.Equal("CooldownActive", secondResult.ErrorCode);
        Assert.Equal(34.0, country.CorruptionIndex);
    }

    [Fact]
    public void DomesticEventsViewModel_MapsPendingEventAndChoicesAccurately()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var catalog = DomesticEventRegistry.CreateDefaultCatalog();
        catalog.TryGet("EVT_DOCTORS_STRIKE", out var strikeEvt);

        session.PendingEvent = new ActiveEventPrompt
        {
            EventId = strikeEvt!.Id,
            CountryId = "IND",
            Date = new DateOnly(2026, 4, 1),
            Title = strikeEvt.Title,
            Description = strikeEvt.Description,
            Choices = strikeEvt.Choices.ToList()
        };

        // Act
        var vm = DomesticEventsViewModel.FromSession(session);

        // Assert
        Assert.True(vm.HasPendingEvent);
        Assert.NotNull(vm.PendingEvent);
        Assert.Equal(strikeEvt.Title, vm.PendingEvent.Title);
        Assert.Equal(2, vm.PendingEvent.Choices.Count);
        Assert.True(vm.PendingEvent.Choices[0].CanAfford);
        Assert.Contains("+5.0% Approval", vm.PendingEvent.Choices[0].ConsequencesSummary);
    }

    [Fact]
    public void SaveLoadRoundTrip_PreservesPendingEventAndEventCooldowns()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        session.PendingEvent = new ActiveEventPrompt
        {
            EventId = "EVT_TEST",
            CountryId = "IND",
            Date = new DateOnly(2026, 5, 1),
            Title = "Test Emergency",
            Description = "Test description",
            Choices = new()
            {
                new("CH_1", "Choice 1", "Desc 1", 100_000_000m, 2.0, 1.0, 0.0)
            }
        };
        session.EventCooldowns["EVT_PREV"] = 8;

        // Act: Serialize and deserialize
        string json = JsonSerializer.Serialize(session);
        var restored = JsonSerializer.Deserialize<GameSession>(json);

        // Assert
        Assert.NotNull(restored);
        Assert.NotNull(restored.PendingEvent);
        Assert.Equal("EVT_TEST", restored.PendingEvent.EventId);
        Assert.Single(restored.PendingEvent.Choices);
        Assert.Equal(8, restored.EventCooldowns["EVT_PREV"]);
    }
}
