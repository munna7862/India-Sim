using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class LaunchCounterTerrorismCommand : ICommand
{
    public const decimal OperationCost = 200_000_000m; // $200M
    public const int CooldownDays = 60; // 2 months cooldown

    public string CountryId { get; }

    public LaunchCounterTerrorismCommand(string countryId)
    {
        CountryId = countryId;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country '{CountryId}' not found.");
        }

        string cooldownKey = "CounterTerrorism";
        if (session.Cooldowns.IsOnCooldown(CountryId, cooldownKey, session.Clock.CurrentDate))
        {
            var expiry = session.Cooldowns.GetExpiry(CountryId, cooldownKey);
            return CommandResult.Failure(
                "ActionOnCooldown",
                $"Counter-terrorism task force is redeploying. Available on {expiry:dd MMM yyyy}."
            );
        }

        if (country.Treasury < OperationCost)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Insufficient treasury to mobilize counter-terrorism commandos. Required: ${OperationCost / 1_000_000m:F0}M, Available: ${country.Treasury / 1_000_000m:F1}M."
            );
        }

        country.Treasury -= OperationCost;
        country.Stability = Math.Min(100.0, country.Stability + 4.0);
        country.ApprovalRating = Math.Min(100.0, country.ApprovalRating + 3.0);
        country.CorruptionIndex = Math.Max(0.0, country.CorruptionIndex - 3.0);

        session.Cooldowns.SetCooldown(CountryId, cooldownKey, session.Clock.CurrentDate.AddDays(CooldownDays));

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Counter-Terrorism Strike Executed",
            Message: $"{country.Name} internal security forces neutralized active terror syndicates. National stability raised to {country.Stability:F0}%.",
            Category: EventCategory.Government,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
