using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class RepayLoanCommand : ICommand
{
    public string CountryId { get; }
    public decimal Amount { get; }

    public RepayLoanCommand(string countryId, decimal amount)
    {
        CountryId = countryId;
        Amount = amount;
    }

    public CommandResult Execute(GameSession session)
    {
        if (Amount <= 0m)
        {
            return CommandResult.Failure("InvalidAmount", $"Repayment amount must be positive. Received: {Amount:C0}.");
        }

        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found.");
        }

        if (country.Debt <= 0m)
        {
            return CommandResult.Failure("NoOutstandingDebt", $"{country.Name} has no outstanding debt to repay.");
        }

        decimal actualRepayment = Math.Min(Amount, country.Debt);

        if (country.Treasury < actualRepayment)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Insufficient treasury funds (${country.Treasury / 1_000_000m:F1}M) to repay ${actualRepayment / 1_000_000m:F1}M of debt."
            );
        }

        country.Treasury -= actualRepayment;
        country.Debt -= actualRepayment;

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Debt Service Repayment",
            Message: $"{country.Name} repaid ${actualRepayment / 1_000_000m:F1}M of national debt (Remaining: ${country.Debt / 1_000_000m:F1}M).",
            Category: EventCategory.Economy,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
