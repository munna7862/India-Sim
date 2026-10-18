using WorldState.Domain.Models;

namespace WorldState.Domain.ViewModels;

public record SubTaskItemViewModel(
    string Id,
    string Description,
    int CurrentCount,
    int TargetCount,
    bool IsCompleted,
    string ProgressText
);

public record TaskItemViewModel(
    string Id,
    string Title,
    string Description,
    TaskCategory Category,
    bool IsPrincipal,
    GameTaskStatus Status,
    string StatusText,
    double ProgressPercentage,
    string ProgressText,
    IReadOnlyList<SubTaskItemViewModel> SubTasks,
    string RewardSummary,
    bool CanClaimReward,
    bool IsRewardClaimed
);

public record TasksViewModel(
    IReadOnlyList<TaskItemViewModel> PrincipalTasks,
    IReadOnlyList<TaskItemViewModel> SecondaryTasks,
    int CompletedCount,
    int TotalCount,
    double OverallProgressPercent,
    string OverallProgressText
)
{
    public static TasksViewModel FromSession(GameSession session)
    {
        var principalList = new List<TaskItemViewModel>();
        var secondaryList = new List<TaskItemViewModel>();
        int completed = 0;

        foreach (var task in session.Tasks.Values)
        {
            if (task.Status == GameTaskStatus.Completed)
            {
                completed++;
            }

            var subTaskVms = task.SubTasks.Select(st => new SubTaskItemViewModel(
                Id: st.Id,
                Description: st.Description,
                CurrentCount: st.CurrentCount,
                TargetCount: st.TargetCount,
                IsCompleted: st.IsCompleted,
                ProgressText: $"{Math.Min(st.CurrentCount, st.TargetCount)}/{st.TargetCount}"
            )).ToList();

            string statusText = task.Status switch
            {
                GameTaskStatus.Completed => task.IsRewardClaimed ? "Reward Claimed" : "Ready to Claim",
                GameTaskStatus.Active => "In Progress",
                GameTaskStatus.Inactive => "Locked (Prerequisites Pending)",
                GameTaskStatus.Failed => "Failed",
                _ => "Unknown"
            };

            var rewardParts = new List<string>();
            if (task.Reward.TreasuryBonus > 0m)
            {
                rewardParts.Add($"+{HudViewModel.FormatCurrency(task.Reward.TreasuryBonus)} Treasury");
            }
            if (task.Reward.ApprovalBonus > 0.0)
            {
                rewardParts.Add($"+{task.Reward.ApprovalBonus:F1}% Approval");
            }
            if (task.Reward.StabilityBonus > 0.0)
            {
                rewardParts.Add($"+{task.Reward.StabilityBonus:F1}% Stability");
            }
            if (!string.IsNullOrEmpty(task.Reward.AchievementId))
            {
                rewardParts.Add($"Badge: {task.Reward.AchievementId}");
            }

            string rewardSummary = rewardParts.Count > 0 ? string.Join(", ", rewardParts) : "No reward";
            bool canClaim = task.Status == GameTaskStatus.Completed && !task.IsRewardClaimed;

            var vm = new TaskItemViewModel(
                Id: task.Id,
                Title: task.Title,
                Description: task.Description,
                Category: task.Category,
                IsPrincipal: task.IsPrincipal,
                Status: task.Status,
                StatusText: statusText,
                ProgressPercentage: task.ProgressPercentage,
                ProgressText: $"{task.ProgressPercentage:F0}%",
                SubTasks: subTaskVms,
                RewardSummary: rewardSummary,
                CanClaimReward: canClaim,
                IsRewardClaimed: task.IsRewardClaimed
            );

            if (task.IsPrincipal)
            {
                principalList.Add(vm);
            }
            else
            {
                secondaryList.Add(vm);
            }
        }

        int total = session.Tasks.Count;
        double overallPct = total > 0 ? (double)completed / total * 100.0 : 0.0;

        return new TasksViewModel(
            PrincipalTasks: principalList,
            SecondaryTasks: secondaryList,
            CompletedCount: completed,
            TotalCount: total,
            OverallProgressPercent: overallPct,
            OverallProgressText: $"{completed}/{total} Completed ({overallPct:F0}%)"
        );
    }
}
