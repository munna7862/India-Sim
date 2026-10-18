using WorldState.Domain.Commands;
using WorldState.Domain.Economy;
using WorldState.Domain.Models;
using WorldState.Domain.Navigation;
using WorldState.Domain.Services;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class CrossSystemGameplayParityTests
{
    [Fact]
    public void CrossSystemParity_EndToEndGameplayProgression_UnlocksTasksAndAchievements()
    {
        // 1. Initialize India game session
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        // 2. Adjust Taxes
        var taxCmd = new AdjustTaxRateCommand("IND", TaxType.IncomeTax, 0.18);
        var taxRes = engine.Dispatch(taxCmd);
        Assert.True(taxRes.IsSuccess);

        // 3. Enact Economic Policies
        var pol1 = new EnactEconomicPolicyCommand("IND", "agricultural_subsidies");
        var pol1Res = engine.Dispatch(pol1);
        Assert.True(pol1Res.IsSuccess);

        var pol2 = new EnactEconomicPolicyCommand("IND", "industrial_modernization");
        var pol2Res = engine.Dispatch(pol2);
        Assert.True(pol2Res.IsSuccess);

        // 4. Verify Policy-based Achievement Unlocked via EvaluateAchievements
        engine.EvaluateAchievements();
        Assert.True(session.Achievements["ACH_IND_REFORM"].IsUnlocked);
        decimal treasuryBeforeClaim = player.Treasury;

        var claimAchCmd = new ClaimAchievementRewardCommand("ACH_IND_REFORM", "IND");
        var claimAchRes = engine.Dispatch(claimAchCmd);
        Assert.True(claimAchRes.IsSuccess);
        Assert.Equal(treasuryBeforeClaim + 1_000_000_000m, player.Treasury);

        // 5. Foreign Diplomacy: Sign NAPs with USA and RUS
        var nap1 = new SignNonAggressionPactCommand("IND", "USA");
        Assert.True(engine.Dispatch(nap1).IsSuccess);

        var nap2 = new SignNonAggressionPactCommand("IND", "RUS");
        Assert.True(engine.Dispatch(nap2).IsSuccess);

        // 6. Verify Peace Maker Achievement
        engine.EvaluateAchievements();
        Assert.True(session.Achievements["ACH_PEACE_MAKER"].IsUnlocked);
        var claimPeaceRes = engine.Dispatch(new ClaimAchievementRewardCommand("ACH_PEACE_MAKER", "IND"));
        Assert.True(claimPeaceRes.IsSuccess);

        // 7. Complete Task ST_POWER and ST_FARMS via facility expansion
        player.Facilities["PowerPlants"] = 35;
        player.Facilities["Farms"] = 60;
        player.Facilities["Factories"] = 50;

        engine.EvaluateTasks();

        var agriTask = session.Tasks.Values.FirstOrDefault(t => t.Id == "TASK_MOD_01");
        if (agriTask != null)
        {
            Assert.True(agriTask.Status == GameTaskStatus.Completed || agriTask.CheckCompletion());
            var claimTaskCmd = new ClaimTaskRewardCommand(agriTask.Id, "IND");
            Assert.True(engine.Dispatch(claimTaskCmd).IsSuccess);
            Assert.Contains(agriTask.Id, session.ClaimedRewardIds);
        }

        // 8. Event Feed & Notifications check
        var eventVm = EventHistoryViewModel.FromSession(session, "All");
        Assert.True(eventVm.TotalEvents > 0);
        Assert.True(eventVm.UnreadCount > 0);

        // Dismiss all notifications
        var dismissRes = engine.Dispatch(new DismissNotificationCommand(null));
        Assert.True(dismissRes.IsSuccess);

        var eventVmAfter = EventHistoryViewModel.FromSession(session, "All");
        Assert.Equal(0, eventVmAfter.UnreadCount);

        // 9. Achievements summary check
        var achVm = AchievementsViewModel.FromSession(session);
        Assert.True(achVm.UnlockedCount >= 2);
        Assert.True(achVm.ClaimedCount >= 2);
        Assert.True(achVm.TotalPrestigeScore >= 300);
    }

    [Fact]
    public void CrossSystemParity_MidOperationSaveAndLoad_PreservesAuthoritativeStateAndTimers()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        // 1. Enqueue Timed Operation (Army Drills)
        var drillCmd = new ConductArmyDrillsCommand("IND");
        Assert.True(engine.Dispatch(drillCmd).IsSuccess);
        Assert.Single(session.TimedOperations);

        // 2. Set an active pending emergency
        session.PendingEvent = new ActiveEventPrompt
        {
            EventId = "EVT_TEST_MID_SAVE",
            CountryId = "IND",
            Date = session.Clock.CurrentDate,
            Title = "Mid-Simulation Emergency",
            Description = "Crisis requires presidential decision.",
            Choices = new()
            {
                new("CH_01", "Choice A", "Option A", 100_000_000m, 2.0, 1.0, 0.0),
                new("CH_02", "Choice B", "Option B", 0m, -2.0, -1.0, 0.0)
            }
        };

        // 3. Sign a trade contract with USA
        var tradeCmd = new SignTradeContractCommand("IND", "USA", "ConsumerGoods", 5.0, 300_000_000m);
        Assert.True(engine.Dispatch(tradeCmd).IsSuccess);

        // 4. Save state
        var saveManager = new SaveManager();
        string slotId = "parity_cross_system_slot";
        var saveResult = saveManager.Save(slotId, session);
        Assert.True(saveResult.IsSuccess);

        // 5. Restore state
        var loadResult = saveManager.Load(slotId);
        Assert.True(loadResult.IsSuccess);
        Assert.NotNull(loadResult.Session);

        var restoredSession = loadResult.Session!;
        var restoredPlayer = restoredSession.GetPlayerCountry()!;

        // Validate restored timers & operations
        Assert.Single(restoredSession.TimedOperations);
        Assert.Equal("ArmyDrills", restoredSession.TimedOperations[0].OperationType);
        Assert.True(restoredSession.Cooldowns.IsOnCooldown("IND", "ArmyDrills", restoredSession.Clock.CurrentDate));

        // Validate restored emergency prompt
        Assert.NotNull(restoredSession.PendingEvent);
        Assert.Equal("Mid-Simulation Emergency", restoredSession.PendingEvent!.Title);
        Assert.Equal(2, restoredSession.PendingEvent.Choices.Count);

        // Validate restored trade contract
        Assert.True(restoredPlayer.DiplomaticRelations.ContainsKey("USA"));
        Assert.Single(restoredPlayer.DiplomaticRelations["USA"].ActiveTradeContracts);

        // 6. Resume simulation on restored engine
        var restoredEngine = new SimulationEngine(restoredSession);

        // Resolve emergency choice
        var choiceCmd = new ResolveEventChoiceCommand("IND", "EVT_TEST_MID_SAVE", "CH_01");
        var choiceRes = restoredEngine.Dispatch(choiceCmd);
        Assert.True(choiceRes.IsSuccess);
        Assert.Null(restoredSession.PendingEvent);

        // Advance 31 days to complete drills
        for (int i = 0; i < 31; i++)
        {
            restoredEngine.Tick(1.0); // 1 tick advances time
        }

        // Clean up save slot
        saveManager.DeleteSlot(slotId);
    }
}
