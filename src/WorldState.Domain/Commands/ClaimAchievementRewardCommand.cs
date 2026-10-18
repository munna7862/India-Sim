using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class ClaimAchievementRewardCommand : ICommand
{
    public string AchievementId { get; }
    public string CountryId { get; }

    public ClaimAchievementRewardCommand(string achievementId, string countryId)
    {
        if (string.IsNullOrWhiteSpace(achievementId))
        {
            throw new ArgumentException("AchievementId cannot be null or whitespace.", nameof(achievementId));
        }

        if (string.IsNullOrWhiteSpace(countryId))
        {
            throw new ArgumentException("CountryId cannot be null or whitespace.", nameof(countryId));
        }

        AchievementId = achievementId;
        CountryId = countryId;
    }

    public CommandResult Validate(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!session.Countries.TryGetValue(CountryId, out _))
        {
            return CommandResult.Failure("CountryNotFound", $"Country '{CountryId}' not found in active session.");
        }

        if (!session.Achievements.TryGetValue(AchievementId, out var progress))
        {
            return CommandResult.Failure("NoProgress", $"Achievement '{AchievementId}' has no recorded progress.");
        }

        if (!progress.IsUnlocked)
        {
            return CommandResult.Failure("NotUnlocked", $"Achievement '{AchievementId}' has not been unlocked yet.");
        }

        if (progress.IsRewardClaimed || session.ClaimedAchievementRewardIds.Contains(AchievementId, StringComparer.OrdinalIgnoreCase))
        {
            return CommandResult.Failure("AlreadyClaimed", $"Reward for achievement '{AchievementId}' has already been claimed.");
        }

        var registry = session.AchievementRegistry ?? Achievements.AchievementRegistry.CreateDefaultCatalog();
        if (!registry.TryGet(AchievementId, out _))
        {
            return CommandResult.Failure("DefinitionNotFound", $"Achievement definition for '{AchievementId}' not found in registry.");
        }

        return CommandResult.Success();
    }

    public CommandResult Execute(GameSession session)
    {
        var validation = Validate(session);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var country = session.Countries[CountryId];
        var progress = session.Achievements[AchievementId];
        var registry = session.AchievementRegistry ?? Achievements.AchievementRegistry.CreateDefaultCatalog();
        registry.TryGet(AchievementId, out var definition);

        progress.IsRewardClaimed = true;
        progress.ClaimedDate = session.Clock.CurrentDate;

        if (!session.ClaimedAchievementRewardIds.Contains(AchievementId, StringComparer.OrdinalIgnoreCase))
        {
            session.ClaimedAchievementRewardIds.Add(AchievementId);
        }

        decimal rewardAmount = definition?.RewardTreasury ?? 0m;
        int prestige = definition?.RewardPrestigeScore ?? 0;

        country.Treasury += rewardAmount;

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Achievement Reward Claimed",
            Message: $"Collected national reward for '{definition?.Title ?? AchievementId}': +${rewardAmount / 1_000_000_000m:0.##}B Treasury and +{prestige} Prestige.",
            Category: EventCategory.Economy,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
