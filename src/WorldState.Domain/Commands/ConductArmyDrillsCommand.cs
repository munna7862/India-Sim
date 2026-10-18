using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class ConductArmyDrillsCommand : ICommand
{
    public const decimal DrillCost = 100_000_000m; // $100M drill cost
    public const int CooldownDays = 180; // 6 months cooldown
    public const int DurationDays = 30; // 30 days exercise duration

    public string CountryId { get; }

    public ConductArmyDrillsCommand(string countryId)
    {
        CountryId = countryId;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found.");
        }

        if (session.Cooldowns.IsOnCooldown(CountryId, "ArmyDrills", session.Clock.CurrentDate))
        {
            var expiry = session.Cooldowns.GetExpiry(CountryId, "ArmyDrills");
            return CommandResult.Failure(
                "ActionOnCooldown",
                $"Army drills are on cooldown until {expiry:dd MMM yyyy}."
            );
        }

        if (country.Treasury < DrillCost)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Insufficient funds to conduct army drills. Required: ${DrillCost / 1_000_000m:F0}M, Available: ${country.Treasury / 1_000_000m:F1}M."
            );
        }

        // Deduct cost and set cooldown
        country.Treasury -= DrillCost;
        session.Cooldowns.SetCooldown(CountryId, "ArmyDrills", session.Clock.CurrentDate.AddDays(CooldownDays));

        // Enqueue timed operation
        var op = new TimedOperation
        {
            CountryId = CountryId,
            Title = "Army Drills Exercise",
            OperationType = "ArmyDrills",
            StartDate = session.Clock.CurrentDate,
            CompletionDate = session.Clock.CurrentDate.AddDays(DurationDays),
            TotalDays = DurationDays
        };
        session.TimedOperations.Add(op);

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Army Drills Commenced",
            Message: $"{country.Name} launched nationwide military drills (Cost: ${DrillCost / 1_000_000m:F0}M, Duration: {DurationDays} days).",
            Category: EventCategory.Military,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
