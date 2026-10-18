using WorldState.Domain.Achievements;
using WorldState.Domain.Models;

namespace WorldState.Domain.ViewModels;

public record AchievementItemViewModel(
    string Id,
    string Title,
    string Description,
    string Category,
    double ProgressPercent,
    string ProgressText,
    bool IsUnlocked,
    string? UnlockedDate,
    bool IsRewardClaimed,
    string RewardText,
    bool CanClaimReward,
    string IconKey
);

public class AchievementsViewModel
{
    public IReadOnlyList<AchievementItemViewModel> Achievements { get; init; } = Array.Empty<AchievementItemViewModel>();
    public int TotalAchievements { get; init; }
    public int UnlockedCount { get; init; }
    public int ClaimedCount { get; init; }
    public int TotalPrestigeScore { get; init; }
    public string SummaryText => $"Achievements: {UnlockedCount} of {TotalAchievements} Unlocked | Prestige Score: {TotalPrestigeScore} pts";

    public static AchievementsViewModel FromSession(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var registry = session.AchievementRegistry ?? AchievementRegistry.CreateDefaultCatalog();
        var items = new List<AchievementItemViewModel>();
        int unlocked = 0;
        int claimed = 0;
        int prestige = 0;

        foreach (var def in registry.Achievements)
        {
            session.Achievements.TryGetValue(def.Id, out var progress);

            bool isUnlocked = progress?.IsUnlocked ?? false;
            bool isClaimed = progress?.IsRewardClaimed ?? false;
            double pct = Math.Clamp(progress?.ProgressPercent ?? 0.0, 0.0, 100.0);

            if (isUnlocked)
            {
                unlocked++;
                prestige += def.RewardPrestigeScore;
            }

            if (isClaimed)
            {
                claimed++;
            }

            string? unlockedDateStr = progress?.UnlockedDate.HasValue == true
                ? progress.UnlockedDate.Value.ToString("yyyy-MM-dd")
                : null;

            string rewardStr = $"+${def.RewardTreasury / 1_000_000_000m:0.##}B Treasury, +{def.RewardPrestigeScore} Prestige";
            string progText = isUnlocked ? "COMPLETED (100%)" : $"{pct:0.#}%";

            items.Add(new AchievementItemViewModel(
                Id: def.Id,
                Title: def.Title,
                Description: def.Description,
                Category: def.Category,
                ProgressPercent: isUnlocked ? 100.0 : pct,
                ProgressText: progText,
                IsUnlocked: isUnlocked,
                UnlockedDate: unlockedDateStr,
                IsRewardClaimed: isClaimed,
                RewardText: rewardStr,
                CanClaimReward: isUnlocked && !isClaimed,
                IconKey: def.IconKey
            ));
        }

        return new AchievementsViewModel
        {
            Achievements = items,
            TotalAchievements = registry.Achievements.Count,
            UnlockedCount = unlocked,
            ClaimedCount = claimed,
            TotalPrestigeScore = prestige
        };
    }
}
