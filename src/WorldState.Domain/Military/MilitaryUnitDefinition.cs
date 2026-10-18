using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorldState.Domain.Military;

public enum MilitaryBranch
{
    Land,
    Air,
    Naval
}

public record MilitaryUnitDefinition(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("branch"), JsonConverter(typeof(JsonStringEnumConverter))] MilitaryBranch Branch,
    [property: JsonPropertyName("costTreasury")] decimal CostTreasury,
    [property: JsonPropertyName("costIron")] double CostIron,
    [property: JsonPropertyName("costOil")] double CostOil,
    [property: JsonPropertyName("costUranium")] double CostUranium,
    [property: JsonPropertyName("monthlyUpkeepTreasury")] decimal MonthlyUpkeepTreasury,
    [property: JsonPropertyName("combatPower")] int CombatPower,
    [property: JsonPropertyName("defensePower")] int DefensePower,
    [property: JsonPropertyName("requiredBarracks")] int RequiredBarracks
);

public class MilitaryUnitRegistry
{
    private readonly Dictionary<string, MilitaryUnitDefinition> _units = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, MilitaryUnitDefinition> Units => _units;

    public void Register(MilitaryUnitDefinition def)
    {
        ArgumentNullException.ThrowIfNull(def);
        _units[def.Id] = def;
    }

    public bool TryGet(string id, out MilitaryUnitDefinition? unit) =>
        _units.TryGetValue(id, out unit);

    public void LoadFromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var list = JsonSerializer.Deserialize<List<MilitaryUnitDefinition>>(json, options);
        if (list == null) return;

        foreach (var def in list)
        {
            Register(def);
        }
    }

    public static MilitaryUnitRegistry CreateDefaultCatalog()
    {
        var registry = new MilitaryUnitRegistry();

        // Land Branch
        registry.Register(new MilitaryUnitDefinition(
            Id: "infantry_division",
            Name: "Infantry Division",
            Branch: MilitaryBranch.Land,
            CostTreasury: 50_000_000m, // $50M
            CostIron: 200.0,
            CostOil: 50.0,
            CostUranium: 0.0,
            MonthlyUpkeepTreasury: 2_000_000m,
            CombatPower: 15,
            DefensePower: 20,
            RequiredBarracks: 5
        ));

        registry.Register(new MilitaryUnitDefinition(
            Id: "main_battle_tank",
            Name: "Main Battle Tank Brigade",
            Branch: MilitaryBranch.Land,
            CostTreasury: 200_000_000m, // $200M
            CostIron: 800.0,
            CostOil: 300.0,
            CostUranium: 0.0,
            MonthlyUpkeepTreasury: 8_000_000m,
            CombatPower: 45,
            DefensePower: 40,
            RequiredBarracks: 10
        ));

        registry.Register(new MilitaryUnitDefinition(
            Id: "artillery_corps",
            Name: "Artillery Corps",
            Branch: MilitaryBranch.Land,
            CostTreasury: 120_000_000m, // $120M
            CostIron: 500.0,
            CostOil: 100.0,
            CostUranium: 0.0,
            MonthlyUpkeepTreasury: 5_000_000m,
            CombatPower: 35,
            DefensePower: 25,
            RequiredBarracks: 8
        ));

        registry.Register(new MilitaryUnitDefinition(
            Id: "mechanized_infantry",
            Name: "Mechanized Armor Division",
            Branch: MilitaryBranch.Land,
            CostTreasury: 150_000_000m, // $150M
            CostIron: 600.0,
            CostOil: 250.0,
            CostUranium: 0.0,
            MonthlyUpkeepTreasury: 6_000_000m,
            CombatPower: 32,
            DefensePower: 32,
            RequiredBarracks: 8
        ));

        // Air Branch
        registry.Register(new MilitaryUnitDefinition(
            Id: "multirole_fighter",
            Name: "Multi-Role Fighter Squadron",
            Branch: MilitaryBranch.Air,
            CostTreasury: 350_000_000m, // $350M
            CostIron: 400.0,
            CostOil: 600.0,
            CostUranium: 0.0,
            MonthlyUpkeepTreasury: 15_000_000m,
            CombatPower: 65,
            DefensePower: 55,
            RequiredBarracks: 12
        ));

        registry.Register(new MilitaryUnitDefinition(
            Id: "attack_helicopter",
            Name: "Attack Helicopter Wing",
            Branch: MilitaryBranch.Air,
            CostTreasury: 180_000_000m, // $180M
            CostIron: 300.0,
            CostOil: 400.0,
            CostUranium: 0.0,
            MonthlyUpkeepTreasury: 8_000_000m,
            CombatPower: 40,
            DefensePower: 30,
            RequiredBarracks: 10
        ));

        registry.Register(new MilitaryUnitDefinition(
            Id: "strategic_bomber",
            Name: "Stealth Strategic Bomber",
            Branch: MilitaryBranch.Air,
            CostTreasury: 600_000_000m, // $600M
            CostIron: 500.0,
            CostOil: 800.0,
            CostUranium: 0.0,
            MonthlyUpkeepTreasury: 25_000_000m,
            CombatPower: 95,
            DefensePower: 35,
            RequiredBarracks: 15
        ));

        registry.Register(new MilitaryUnitDefinition(
            Id: "air_defense_system",
            Name: "Surface-to-Air Missile Battery",
            Branch: MilitaryBranch.Air,
            CostTreasury: 250_000_000m, // $250M
            CostIron: 500.0,
            CostOil: 100.0,
            CostUranium: 0.0,
            MonthlyUpkeepTreasury: 10_000_000m,
            CombatPower: 25,
            DefensePower: 75,
            RequiredBarracks: 10
        ));

        registry.Register(new MilitaryUnitDefinition(
            Id: "recon_drone_fleet",
            Name: "Armed Recon Drone Fleet",
            Branch: MilitaryBranch.Air,
            CostTreasury: 100_000_000m, // $100M
            CostIron: 150.0,
            CostOil: 150.0,
            CostUranium: 0.0,
            MonthlyUpkeepTreasury: 4_000_000m,
            CombatPower: 30,
            DefensePower: 15,
            RequiredBarracks: 5
        ));

        // Naval Branch
        registry.Register(new MilitaryUnitDefinition(
            Id: "guided_missile_destroyer",
            Name: "Guided Missile Destroyer",
            Branch: MilitaryBranch.Naval,
            CostTreasury: 500_000_000m, // $500M
            CostIron: 1500.0,
            CostOil: 800.0,
            CostUranium: 0.0,
            MonthlyUpkeepTreasury: 20_000_000m,
            CombatPower: 80,
            DefensePower: 75,
            RequiredBarracks: 12
        ));

        registry.Register(new MilitaryUnitDefinition(
            Id: "attack_submarine",
            Name: "Nuclear Attack Submarine",
            Branch: MilitaryBranch.Naval,
            CostTreasury: 800_000_000m, // $800M
            CostIron: 2000.0,
            CostOil: 200.0,
            CostUranium: 100.0,
            MonthlyUpkeepTreasury: 30_000_000m,
            CombatPower: 100,
            DefensePower: 85,
            RequiredBarracks: 15
        ));

        registry.Register(new MilitaryUnitDefinition(
            Id: "aircraft_carrier",
            Name: "Fleet Aircraft Carrier",
            Branch: MilitaryBranch.Naval,
            CostTreasury: 2_500_000_000m, // $2.5B
            CostIron: 5000.0,
            CostOil: 2000.0,
            CostUranium: 200.0,
            MonthlyUpkeepTreasury: 80_000_000m,
            CombatPower: 220,
            DefensePower: 180,
            RequiredBarracks: 20
        ));

        return registry;
    }
}
