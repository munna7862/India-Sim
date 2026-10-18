using WorldState.Domain.Military;
using WorldState.Domain.Models;

namespace WorldState.Domain.ViewModels;

public record MilitaryUnitCardViewModel(
    string UnitId,
    string UnitName,
    MilitaryBranch Branch,
    int ActiveCount,
    int CombatPower,
    int DefensePower,
    decimal CostTreasury,
    double CostIron,
    double CostOil,
    double CostUranium,
    decimal MonthlyUpkeepTreasury,
    int RequiredBarracks,
    bool MeetsBarracksRequirement,
    bool CanAfford1,
    bool CanAfford5,
    bool CanAfford10
);

public record MilitaryViewModel(
    string PlayerCountryId,
    string PlayerCountryName,
    int TotalUnits,
    int TotalCombatPower,
    int TotalDefensePower,
    decimal TotalMonthlyUpkeep,
    bool HasActiveDrillReadiness,
    string DrillReadinessExpiryText,
    int LandUnitsCount,
    int AirUnitsCount,
    int NavalUnitsCount,
    IReadOnlyList<MilitaryUnitCardViewModel> Units
)
{
    public static MilitaryViewModel FromSession(GameSession session, string? playerCountryId = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        string playerId = playerCountryId ?? session.PlayerCountryId;
        var playerCountry = session.Countries.TryGetValue(playerId, out var pc) ? pc : null;
        string playerName = playerCountry?.Name ?? playerId;

        var registry = session.MilitaryRegistry ?? MilitaryUnitRegistry.CreateDefaultCatalog();

        int totalUnits = playerCountry?.GetTotalMilitaryUnits() ?? 0;
        int totalCombat = playerCountry?.CalculateTotalCombatPower(registry) ?? 0;
        int totalDefense = playerCountry?.CalculateTotalDefensePower(registry) ?? 0;
        decimal totalUpkeep = playerCountry?.CalculateMonthlyMilitaryUpkeep(registry) ?? 0m;

        bool hasDrill = session.Cooldowns.IsOnCooldown(playerId, "ArmyDrills", session.Clock.CurrentDate);
        string drillText = "No active drill readiness buff";
        if (hasDrill)
        {
            var expiry = session.Cooldowns.GetExpiry(playerId, "ArmyDrills");
            drillText = $"Active Drill Readiness (expires {expiry:dd MMM yyyy})";
        }

        int landCount = 0;
        int airCount = 0;
        int navalCount = 0;

        var cards = new List<MilitaryUnitCardViewModel>();

        if (playerCountry != null)
        {
            int barracks = playerCountry.Facilities.GetValueOrDefault("Barracks", 0);

            foreach (var (_, unit) in registry.Units)
            {
                int count = playerCountry.MilitaryUnits.GetValueOrDefault(unit.Id, 0);

                switch (unit.Branch)
                {
                    case MilitaryBranch.Land: landCount += count; break;
                    case MilitaryBranch.Air: airCount += count; break;
                    case MilitaryBranch.Naval: navalCount += count; break;
                }

                bool meetsBarracks = barracks >= unit.RequiredBarracks;

                bool canAfford1 = meetsBarracks &&
                                  playerCountry.Treasury >= unit.CostTreasury &&
                                  playerCountry.Stockpiles.Iron >= unit.CostIron &&
                                  playerCountry.Stockpiles.Oil >= unit.CostOil &&
                                  playerCountry.Stockpiles.Uranium >= unit.CostUranium;

                bool canAfford5 = meetsBarracks &&
                                  playerCountry.Treasury >= unit.CostTreasury * 5 &&
                                  playerCountry.Stockpiles.Iron >= unit.CostIron * 5 &&
                                  playerCountry.Stockpiles.Oil >= unit.CostOil * 5 &&
                                  playerCountry.Stockpiles.Uranium >= unit.CostUranium * 5;

                bool canAfford10 = meetsBarracks &&
                                   playerCountry.Treasury >= unit.CostTreasury * 10 &&
                                   playerCountry.Stockpiles.Iron >= unit.CostIron * 10 &&
                                   playerCountry.Stockpiles.Oil >= unit.CostOil * 10 &&
                                   playerCountry.Stockpiles.Uranium >= unit.CostUranium * 10;

                cards.Add(new MilitaryUnitCardViewModel(
                    UnitId: unit.Id,
                    UnitName: unit.Name,
                    Branch: unit.Branch,
                    ActiveCount: count,
                    CombatPower: unit.CombatPower,
                    DefensePower: unit.DefensePower,
                    CostTreasury: unit.CostTreasury,
                    CostIron: unit.CostIron,
                    CostOil: unit.CostOil,
                    CostUranium: unit.CostUranium,
                    MonthlyUpkeepTreasury: unit.MonthlyUpkeepTreasury,
                    RequiredBarracks: unit.RequiredBarracks,
                    MeetsBarracksRequirement: meetsBarracks,
                    CanAfford1: canAfford1,
                    CanAfford5: canAfford5,
                    CanAfford10: canAfford10
                ));
            }
        }

        return new MilitaryViewModel(
            PlayerCountryId: playerId,
            PlayerCountryName: playerName,
            TotalUnits: totalUnits,
            TotalCombatPower: totalCombat,
            TotalDefensePower: totalDefense,
            TotalMonthlyUpkeep: totalUpkeep,
            HasActiveDrillReadiness: hasDrill,
            DrillReadinessExpiryText: drillText,
            LandUnitsCount: landCount,
            AirUnitsCount: airCount,
            NavalUnitsCount: navalCount,
            Units: cards
        );
    }
}
