using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class StartTimedConstructionCommand : ICommand
{
    public const int DefaultBuildDays = 60; // 60 in-game days to construct

    public string CountryId { get; }
    public string FacilityType { get; }
    public int Quantity { get; }
    public int BuildDays { get; }

    public StartTimedConstructionCommand(
        string countryId,
        string facilityType,
        int quantity = 1,
        int buildDays = DefaultBuildDays)
    {
        CountryId = countryId;
        FacilityType = facilityType;
        Quantity = quantity;
        BuildDays = buildDays;
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

        if (!ConstructFacilityCommand.CostTable.TryGetValue(FacilityType, out var unitCost))
        {
            return CommandResult.Failure(
                "UnknownFacilityType",
                $"Facility type '{FacilityType}' is not recognized."
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

        // Deduct upfront resource investments
        country.Treasury -= totalMoneyCost;
        country.Stockpiles = country.Stockpiles with
        {
            Iron = country.Stockpiles.Iron - totalIronCost
        };

        // Enqueue timed operation
        var op = new TimedOperation
        {
            CountryId = CountryId,
            Title = $"Construct {Quantity}x {FacilityType}",
            OperationType = "Construction",
            StartDate = session.Clock.CurrentDate,
            CompletionDate = session.Clock.CurrentDate.AddDays(BuildDays),
            TotalDays = BuildDays,
            Parameters = new Dictionary<string, string>
            {
                ["FacilityType"] = FacilityType,
                ["Quantity"] = Quantity.ToString()
            }
        };
        session.TimedOperations.Add(op);

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Construction Commenced",
            Message: $"{country.Name} broke ground on {Quantity}x {FacilityType} (${totalMoneyCost / 1_000_000m:F1}M, {totalIronCost:F0} Iron). Ready in {BuildDays} days.",
            Category: EventCategory.Economy,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
