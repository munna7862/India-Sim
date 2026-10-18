using WorldState.Domain.Military;
using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class RecruitMilitaryUnitsCommand : ICommand
{
    public string CountryId { get; }
    public string UnitTypeId { get; }
    public int Quantity { get; }

    public RecruitMilitaryUnitsCommand(string countryId, string unitTypeId, int quantity)
    {
        CountryId = countryId;
        UnitTypeId = unitTypeId;
        Quantity = quantity;
    }

    public CommandResult Execute(GameSession session)
    {
        if (Quantity <= 0)
        {
            return CommandResult.Failure("InvalidQuantity", "Recruitment quantity must be greater than zero.");
        }

        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country '{CountryId}' not found.");
        }

        var registry = session.MilitaryRegistry ?? MilitaryUnitRegistry.CreateDefaultCatalog();
        if (!registry.TryGet(UnitTypeId, out var unit) || unit == null)
        {
            return CommandResult.Failure("UnitNotFound", $"Military unit specification '{UnitTypeId}' not recognized.");
        }

        int barracks = country.Facilities.GetValueOrDefault("Barracks", 0);
        if (barracks < unit.RequiredBarracks)
        {
            return CommandResult.Failure(
                "InsufficientBarracks",
                $"Commissioning {unit.Name} requires at least {unit.RequiredBarracks} operational Barracks. Current: {barracks}."
            );
        }

        decimal totalTreasuryCost = unit.CostTreasury * Quantity;
        double totalIronCost = unit.CostIron * Quantity;
        double totalOilCost = unit.CostOil * Quantity;
        double totalUraniumCost = unit.CostUranium * Quantity;

        if (country.Treasury < totalTreasuryCost)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Insufficient treasury to recruit {Quantity}x {unit.Name}. Required: ${totalTreasuryCost / 1_000_000m:F0}M, Available: ${country.Treasury / 1_000_000m:F1}M."
            );
        }

        if (country.Stockpiles.Iron < totalIronCost)
        {
            return CommandResult.Failure(
                "InsufficientIron",
                $"Insufficient Iron stockpile to construct {Quantity}x {unit.Name}. Required: {totalIronCost:N0}, Available: {country.Stockpiles.Iron:N0}."
            );
        }

        if (country.Stockpiles.Oil < totalOilCost)
        {
            return CommandResult.Failure(
                "InsufficientOil",
                $"Insufficient Oil stockpile to fuel {Quantity}x {unit.Name}. Required: {totalOilCost:N0}, Available: {country.Stockpiles.Oil:N0}."
            );
        }

        if (country.Stockpiles.Uranium < totalUraniumCost)
        {
            return CommandResult.Failure(
                "InsufficientUranium",
                $"Insufficient Uranium stockpile for {Quantity}x {unit.Name}. Required: {totalUraniumCost:N0}, Available: {country.Stockpiles.Uranium:N0}."
            );
        }

        // Deduct treasury and strategic resources
        country.Treasury -= totalTreasuryCost;
        country.Stockpiles = new ResourceStockpile(
            Food: country.Stockpiles.Food,
            Energy: country.Stockpiles.Energy,
            Oil: country.Stockpiles.Oil - totalOilCost,
            Iron: country.Stockpiles.Iron - totalIronCost,
            Uranium: country.Stockpiles.Uranium - totalUraniumCost,
            ConsumerGoods: country.Stockpiles.ConsumerGoods
        );

        // Add to inventory
        country.MilitaryUnits[UnitTypeId] = country.MilitaryUnits.GetValueOrDefault(UnitTypeId, 0) + Quantity;

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Military Force Commissioned",
            Message: $"{country.Name} inducted {Quantity:N0}x {unit.Name} into the armed forces (Cost: ${totalTreasuryCost / 1_000_000m:F0}M). Total units of type: {country.MilitaryUnits[UnitTypeId]}.",
            Category: EventCategory.Military,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
