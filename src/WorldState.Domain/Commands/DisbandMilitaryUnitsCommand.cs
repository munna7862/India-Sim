using WorldState.Domain.Military;
using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class DisbandMilitaryUnitsCommand : ICommand
{
    public string CountryId { get; }
    public string UnitTypeId { get; }
    public int Quantity { get; }

    public DisbandMilitaryUnitsCommand(string countryId, string unitTypeId, int quantity)
    {
        CountryId = countryId;
        UnitTypeId = unitTypeId;
        Quantity = quantity;
    }

    public CommandResult Execute(GameSession session)
    {
        if (Quantity <= 0)
        {
            return CommandResult.Failure("InvalidQuantity", "Disband quantity must be greater than zero.");
        }

        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country '{CountryId}' not found.");
        }

        int currentCount = country.MilitaryUnits.GetValueOrDefault(UnitTypeId, 0);
        if (currentCount < Quantity)
        {
            return CommandResult.Failure(
                "InsufficientUnits",
                $"Cannot decommission {Quantity} units. Active inventory of '{UnitTypeId}' is only {currentCount}."
            );
        }

        var registry = session.MilitaryRegistry ?? MilitaryUnitRegistry.CreateDefaultCatalog();
        registry.TryGet(UnitTypeId, out var unit);

        country.MilitaryUnits[UnitTypeId] = currentCount - Quantity;
        if (country.MilitaryUnits[UnitTypeId] <= 0)
        {
            country.MilitaryUnits.Remove(UnitTypeId);
        }

        // Recover 20% scrap materials
        double recoveredIron = (unit?.CostIron ?? 0.0) * Quantity * 0.20;
        double recoveredOil = (unit?.CostOil ?? 0.0) * Quantity * 0.20;

        country.Stockpiles = new ResourceStockpile(
            Food: country.Stockpiles.Food,
            Energy: country.Stockpiles.Energy,
            Oil: country.Stockpiles.Oil + recoveredOil,
            Iron: country.Stockpiles.Iron + recoveredIron,
            Uranium: country.Stockpiles.Uranium,
            ConsumerGoods: country.Stockpiles.ConsumerGoods
        );

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Units Decommissioned",
            Message: $"{country.Name} decommissioned {Quantity:N0}x {unit?.Name ?? UnitTypeId} (Scrap recovered: {recoveredIron:F0} Iron, {recoveredOil:F0} Oil).",
            Category: EventCategory.Military,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
