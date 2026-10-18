using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class TakeLoanCommand : ICommand
{
    public string CountryId { get; }
    public decimal Amount { get; }

    public TakeLoanCommand(string countryId, decimal amount)
    {
        CountryId = countryId;
        Amount = amount;
    }

    public CommandResult Execute(GameSession session)
    {
        if (Amount <= 0m)
        {
            return CommandResult.Failure("InvalidAmount", $"Loan amount must be positive. Received: {Amount:C0}.");
        }

        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found.");
        }

        decimal maxDebt = Math.Max(100_000_000_000m, (decimal)country.Population * 200m);
        if (country.Debt + Amount > maxDebt)
        {
            return CommandResult.Failure(
                "DebtCeilingExceeded",
                $"Borrowing ${Amount / 1_000_000m:F1}M exceeds the national debt ceiling of ${maxDebt / 1_000_000m:F1}M (Current debt: ${country.Debt / 1_000_000m:F1}M)."
            );
        }

        country.Treasury += Amount;
        country.Debt += Amount;

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Central Bank Loan Secured",
            Message: $"{country.Name} borrowed ${Amount / 1_000_000m:F1}M (Total Debt: ${country.Debt / 1_000_000m:F1}M).",
            Category: EventCategory.Economy,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
