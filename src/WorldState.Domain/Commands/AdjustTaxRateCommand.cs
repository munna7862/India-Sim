using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public enum TaxType
{
    IncomeTax,
    CorporateTax,
    Vat
}

public class AdjustTaxRateCommand : ICommand
{
    public string CountryId { get; }
    public TaxType Type { get; }
    public double NewRate { get; }

    public AdjustTaxRateCommand(string countryId, TaxType type, double newRate)
    {
        CountryId = countryId;
        Type = type;
        NewRate = newRate;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found.");
        }

        if (NewRate < 0.0 || NewRate > 1.0)
        {
            return CommandResult.Failure(
                "InvalidTaxRate",
                $"Tax rate must be between 0.0 (0%) and 1.0 (100%). Received {NewRate}."
            );
        }

        switch (Type)
        {
            case TaxType.IncomeTax:
                country.IncomeTaxRate = NewRate;
                break;
            case TaxType.CorporateTax:
                country.CorporateTaxRate = NewRate;
                break;
            case TaxType.Vat:
                country.VatRate = NewRate;
                break;
            default:
                return CommandResult.Failure("UnknownTaxType", $"Tax type {Type} is not recognized.");
        }

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Tax Policy Adjusted",
            Message: $"{Type} for {country.Name} adjusted to {NewRate * 100:F1}%.",
            Category: EventCategory.Economy,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
