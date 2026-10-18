using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class ConstructFacilityCommand : ICommand
{
    public string CountryId { get; }
    public string FacilityType { get; }
    public int Quantity { get; }

    public static readonly Dictionary<string, (decimal MoneyCost, double IronCost)> CostTable =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Farms"] = (100_000_000m, 500.0),
            ["Mines"] = (250_000_000m, 1_000.0),
            ["PowerPlants"] = (500_000_000m, 2_000.0),
            ["Factories"] = (300_000_000m, 1_500.0),
            ["Hospitals"] = (200_000_000m, 500.0),
            ["Schools"] = (150_000_000m, 500.0),
            ["Barracks"] = (250_000_000m, 1_000.0)
        };

    public ConstructFacilityCommand(string countryId, string facilityType, int quantity = 1)
    {
        CountryId = countryId;
        FacilityType = facilityType;
        Quantity = quantity;
    }

    public CommandResult Execute(GameSession session)
    {
        if (Quantity <= 0)
        {
            return CommandResult.Failure("InvalidQuantity", $"Facility quantity must be at least 1. Received: {Quantity}.");
        }

        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found.");
        }

        if (!CostTable.TryGetValue(FacilityType, out var unitCost))
        {
            return CommandResult.Failure(
                "UnknownFacilityType",
                $"Facility type '{FacilityType}' is not recognized. Supported types: {string.Join(", ", CostTable.Keys)}."
            );
        }

        decimal totalMoneyCost = unitCost.MoneyCost * Quantity;
        double totalIronCost = unitCost.IronCost * Quantity;

        if (country.Treasury < totalMoneyCost)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Insufficient funds to construct {Quantity}x {FacilityType}. Required: ${totalMoneyCost / 1_000_000m:F1}M, Available: ${country.Treasury / 1_000_000m:F1}M."
            );
        }

        if (country.Stockpiles.Iron < totalIronCost)
        {
            return CommandResult.Failure(
                "InsufficientResources",
                $"Insufficient iron to construct {Quantity}x {FacilityType}. Required: {totalIronCost:F0} Iron, Available: {country.Stockpiles.Iron:F0} Iron."
            );
        }

        // Apply mutation
        country.Treasury -= totalMoneyCost;
        country.Stockpiles = country.Stockpiles with
        {
            Iron = country.Stockpiles.Iron - totalIronCost
        };

        int currentCount = country.Facilities.GetValueOrDefault(FacilityType, 0);
        country.Facilities[FacilityType] = currentCount + Quantity;

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Facilities Constructed",
            Message: $"{country.Name} constructed {Quantity}x {FacilityType} for ${totalMoneyCost / 1_000_000m:F1}M and {totalIronCost:F0} Iron (Total: {country.Facilities[FacilityType]}).",
            Category: EventCategory.Economy,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
