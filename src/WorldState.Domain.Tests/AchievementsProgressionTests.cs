using WorldState.Domain.Achievements;
using WorldState.Domain.Commands;
using WorldState.Domain.Models;
using WorldState.Domain.Services;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class AchievementsProgressionTests
{
    [Fact]
    public void AchievementRegistry_LoadsDefaultCatalog_ContainsExpectedDefinitions()
    {
        var registry = AchievementRegistry.CreateDefaultCatalog();
        Assert.NotEmpty(registry.Achievements);
        Assert.True(registry.Achievements.Count >= 8);
        Assert.True(registry.TryGet("ACH_IND_REFORM", out var reformAch));
        Assert.NotNull(reformAch);
        Assert.Equal("Economic Awakening", reformAch.Title);
        Assert.Equal(1_000_000_000m, reformAch.RewardTreasury);
    }

    [Fact]
    public void EvaluateAchievements_PolicyCount_UnlocksWhenThresholdMet()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        // Before policies
        engine.EvaluateAchievements();
        var achProgress = session.Achievements["ACH_IND_REFORM"];
        Assert.False(achProgress.IsUnlocked);
        Assert.Equal(0.0, achProgress.ProgressPercent);

        // Enact 2 policies
        player.ActivePolicies.Add("agri_subsidies");
        player.ActivePolicies.Add("industrial_automation");

        engine.EvaluateAchievements();
        Assert.True(achProgress.IsUnlocked);
        Assert.Equal(100.0, achProgress.ProgressPercent);
        Assert.NotNull(achProgress.UnlockedDate);

        // Verify notification event was added
        Assert.Contains(session.Events, e => e.Category == EventCategory.Achievements && e.Title.Contains("Economic Awakening"));
    }

    [Fact]
    public void EvaluateAchievements_DebtAndTreasury_RequiresZeroDebtAndTreasuryThreshold()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        player.Debt = 10_000_000_000m;
        player.Treasury = 60_000_000_000m;

        engine.EvaluateAchievements();
        var progress = session.Achievements["ACH_SOVEREIGN_SOLVENCY"];
        Assert.False(progress.IsUnlocked);

        // Repay all debt
        player.Debt = 0m;
        engine.EvaluateAchievements();
        Assert.True(progress.IsUnlocked);
        Assert.Equal(100.0, progress.ProgressPercent);
    }

    [Fact]
    public void ClaimAchievementReward_ValidClaim_CreditsTreasuryAndMarksClaimed()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        player.ActivePolicies.Add("agri_subsidies");
        player.ActivePolicies.Add("industrial_automation");
        engine.EvaluateAchievements();

        decimal initialTreasury = player.Treasury;

        var cmd = new ClaimAchievementRewardCommand("ACH_IND_REFORM", player.Id);
        var result = engine.Dispatch(cmd);

        Assert.True(result.IsSuccess);
        Assert.Equal(initialTreasury + 1_000_000_000m, player.Treasury);

        var progress = session.Achievements["ACH_IND_REFORM"];
        Assert.True(progress.IsRewardClaimed);
        Assert.NotNull(progress.ClaimedDate);
        Assert.Contains("ACH_IND_REFORM", session.ClaimedAchievementRewardIds);
    }

    [Fact]
    public void ClaimAchievementReward_DuplicateClaim_FailsValidation()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        player.ActivePolicies.Add("agri_subsidies");
        player.ActivePolicies.Add("industrial_automation");
        engine.EvaluateAchievements();

        var cmd1 = new ClaimAchievementRewardCommand("ACH_IND_REFORM", player.Id);
        var res1 = engine.Dispatch(cmd1);
        Assert.True(res1.IsSuccess);

        // Attempt second claim
        var cmd2 = new ClaimAchievementRewardCommand("ACH_IND_REFORM", player.Id);
        var res2 = engine.Dispatch(cmd2);
        Assert.False(res2.IsSuccess);
        Assert.Equal("AlreadyClaimed", res2.ErrorCode);
    }

    [Fact]
    public void ClaimAchievementReward_LockedAchievement_FailsValidation()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        var cmd = new ClaimAchievementRewardCommand("ACH_TITAN_INDUSTRY", player.Id);
        var res = engine.Dispatch(cmd);
        Assert.False(res.IsSuccess);
        Assert.Equal("NotUnlocked", res.ErrorCode);
    }

    [Fact]
    public void AchievementsViewModel_MapsAllMetricsAndPrestigeAccurately()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        player.ActivePolicies.Add("agri_subsidies");
        player.ActivePolicies.Add("industrial_automation");
        engine.EvaluateAchievements();

        var vm = AchievementsViewModel.FromSession(session);
        Assert.Equal(8, vm.TotalAchievements);
        Assert.Equal(3, vm.UnlockedCount);
        Assert.Equal(0, vm.ClaimedCount);
        Assert.Equal(650, vm.TotalPrestigeScore);

        var reformItem = vm.Achievements.First(a => a.Id == "ACH_IND_REFORM");
        Assert.True(reformItem.IsUnlocked);
        Assert.False(reformItem.IsRewardClaimed);
        Assert.True(reformItem.CanClaimReward);
        Assert.Equal("COMPLETED (100%)", reformItem.ProgressText);
    }

    [Fact]
    public void SaveAndLoad_PreservesAchievementProgressAndClaimedStatus()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        player.ActivePolicies.Add("agri_subsidies");
        player.ActivePolicies.Add("industrial_automation");
        engine.EvaluateAchievements();

        engine.Dispatch(new ClaimAchievementRewardCommand("ACH_IND_REFORM", player.Id));

        var saveManager = new SaveManager();
        string testSlot = "achievements_roundtrip_test";
        var saveResult = saveManager.Save(testSlot, session);
        Assert.True(saveResult.IsSuccess);

        var loadResult = saveManager.Load(testSlot);
        Assert.True(loadResult.IsSuccess);
        Assert.NotNull(loadResult.Session);

        var restored = loadResult.Session!;
        Assert.True(restored.Achievements.ContainsKey("ACH_IND_REFORM"));
        Assert.True(restored.Achievements["ACH_IND_REFORM"].IsUnlocked);
        Assert.True(restored.Achievements["ACH_IND_REFORM"].IsRewardClaimed);
        Assert.Contains("ACH_IND_REFORM", restored.ClaimedAchievementRewardIds);

        saveManager.DeleteSlot(testSlot);
    }
}
