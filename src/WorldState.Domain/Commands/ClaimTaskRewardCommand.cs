using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class ClaimTaskRewardCommand : ICommand
{
    public string CountryId { get; }
    public string TaskId { get; }

    public ClaimTaskRewardCommand(string countryId, string taskId)
    {
        CountryId = countryId;
        TaskId = taskId;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found.");
        }

        if (!session.Tasks.TryGetValue(TaskId, out var task))
        {
            return CommandResult.Failure("TaskNotFound", $"Task '{TaskId}' does not exist.");
        }

        if (!task.CheckCompletion())
        {
            return CommandResult.Failure("TaskNotCompleted", $"Task '{task.Title}' is not yet completed.");
        }

        if (task.IsRewardClaimed || session.ClaimedRewardIds.Contains(TaskId))
        {
            return CommandResult.Failure("RewardAlreadyClaimed", $"Reward for task '{task.Title}' has already been claimed.");
        }

        // Apply reward
        country.Treasury += task.Reward.TreasuryBonus;
        country.ApprovalRating = Math.Clamp(country.ApprovalRating + task.Reward.ApprovalBonus, 0.0, 100.0);
        country.Stability = Math.Clamp(country.Stability + task.Reward.StabilityBonus, 0.0, 100.0);

        task.IsRewardClaimed = true;
        if (!session.ClaimedRewardIds.Contains(TaskId))
        {
            session.ClaimedRewardIds.Add(TaskId);
        }

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Presidential Task Reward Claimed",
            Message: $"{country.Name} completed '{task.Title}' and claimed reward: +${task.Reward.TreasuryBonus / 1_000_000m:F0}M Treasury, +{task.Reward.ApprovalBonus:F1}% Approval, +{task.Reward.StabilityBonus:F1}% Stability.",
            Category: EventCategory.Tasks,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
