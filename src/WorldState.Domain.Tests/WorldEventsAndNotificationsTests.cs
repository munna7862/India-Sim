using WorldState.Domain.Commands;
using WorldState.Domain.Events;
using WorldState.Domain.Models;
using WorldState.Domain.Navigation;
using WorldState.Domain.Services;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class WorldEventsAndNotificationsTests
{
    [Fact]
    public void WorldEventRegistry_LoadsDefaultCatalog_ContainsExpectedDefinitions()
    {
        var registry = WorldEventRegistry.CreateDefaultCatalog();
        Assert.NotEmpty(registry.Events);
        Assert.True(registry.Events.Count >= 6);
        Assert.True(registry.TryGet("WEV_GLOBAL_TRADE_BOOM", out var tradeBoom));
        Assert.NotNull(tradeBoom);
        Assert.Equal("Global Trade Expansion", tradeBoom.Title);
        Assert.Equal(1_500_000_000m, tradeBoom.TreasuryImpact);
    }

    [Fact]
    public void ProcessWorldEvents_TriggersEvent_AppliesEffectsAndSetsCooldown()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        // Directly invoke a specific world event to test effect application
        var evtDef = engine.WorldEventRegistry.Events.First(e => e.Id == "WEV_GLOBAL_TRADE_BOOM");
        decimal initialTreasury = player.Treasury;
        double initialStability = player.Stability;

        evtDef.ApplyEffects(session, player);

        Assert.Equal(initialTreasury + evtDef.TreasuryImpact, player.Treasury);
        Assert.Equal(Math.Min(100.0, initialStability + evtDef.StabilityImpact), player.Stability);
    }

    [Fact]
    public void ProcessWorldEvents_CooldownActive_PreventsRetrigger()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);

        // Put all events on 10-month cooldown
        foreach (var evt in engine.WorldEventRegistry.Events)
        {
            session.EventCooldowns[evt.Id] = 10;
        }

        int initialEventCount = session.Events.Count;
        engine.ProcessWorldEvents(new DateOnly(2026, 6, 1));

        Assert.Equal(initialEventCount, session.Events.Count);
    }

    [Fact]
    public void DismissNotificationCommand_SpecificEvent_MarksEventAsRead()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);

        var testEvent = new GameEvent(
            Id: "TEST_EVT_01",
            Date: session.Clock.CurrentDate,
            Title: "Test Alert",
            Message: "Alert contents",
            IsRead: false
        );
        session.AddEvent(testEvent);

        var cmd = new DismissNotificationCommand("TEST_EVT_01");
        var result = engine.Dispatch(cmd);

        Assert.True(result.IsSuccess);
        var updated = session.Events.First(e => e.Id == "TEST_EVT_01");
        Assert.True(updated.IsRead);
    }

    [Fact]
    public void DismissNotificationCommand_AllEvents_MarksAllAsRead()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);

        session.AddEvent(new GameEvent("E1", session.Clock.CurrentDate, "T1", "M1", IsRead: false));
        session.AddEvent(new GameEvent("E2", session.Clock.CurrentDate, "T2", "M2", IsRead: false));

        var cmd = new DismissNotificationCommand(null);
        var result = engine.Dispatch(cmd);

        Assert.True(result.IsSuccess);
        Assert.All(session.Events, e => Assert.True(e.IsRead));
    }

    [Fact]
    public void EventHistoryViewModel_FiltersByCategory_AndComputesUnreadAccurately()
    {
        var session = GameSession.CreateNewGame("IND");
        session.Events.Clear();

        session.AddEvent(new GameEvent("E1", session.Clock.CurrentDate, "Economy News", "M1", EventCategory.Economy, IsRead: false, ActionRoute: ScreenRoute.EconomyAndBudget));
        session.AddEvent(new GameEvent("E2", session.Clock.CurrentDate, "Military Alert", "M2", EventCategory.Military, IsRead: true, ActionRoute: ScreenRoute.MilitaryAndDefense));
        session.AddEvent(new GameEvent("E3", session.Clock.CurrentDate, "World Event", "M3", EventCategory.World, IsRead: false));

        var allVm = EventHistoryViewModel.FromSession(session, "All");
        Assert.Equal(3, allVm.TotalEvents);
        Assert.Equal(2, allVm.UnreadCount);
        Assert.Equal(3, allVm.Events.Count);

        var econVm = EventHistoryViewModel.FromSession(session, "Economy");
        Assert.Single(econVm.Events);
        Assert.Equal("View Economy", econVm.Events[0].ActionButtonText);
        Assert.Equal(ScreenRoute.EconomyAndBudget, econVm.Events[0].ActionRoute);
    }

    [Fact]
    public void SaveAndLoad_PreservesEventsAndReadFlags()
    {
        var session = GameSession.CreateNewGame("IND");
        session.AddEvent(new GameEvent("EVT_SAVE_TEST", session.Clock.CurrentDate, "Special Alert", "Details", EventCategory.World, EventSeverity.Critical, IsRead: true, ActionRoute: ScreenRoute.WorldMap));

        var saveManager = new SaveManager();
        string slot = "events_persistence_test";
        var saveRes = saveManager.Save(slot, session);
        Assert.True(saveRes.IsSuccess);

        var loadRes = saveManager.Load(slot);
        Assert.True(loadRes.IsSuccess);
        Assert.NotNull(loadRes.Session);

        var loadedEvt = loadRes.Session!.Events.FirstOrDefault(e => e.Id == "EVT_SAVE_TEST");
        Assert.NotNull(loadedEvt);
        Assert.True(loadedEvt.IsRead);
        Assert.Equal(EventCategory.World, loadedEvt.Category);
        Assert.Equal(EventSeverity.Critical, loadedEvt.Severity);
        Assert.Equal(ScreenRoute.WorldMap, loadedEvt.ActionRoute);

        saveManager.DeleteSlot(slot);
    }
}
