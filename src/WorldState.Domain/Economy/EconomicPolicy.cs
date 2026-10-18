using System.Text.Json;
using WorldState.Domain.Models;

namespace WorldState.Domain.Economy;

public class EconomicPolicyDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public decimal UpfrontCost { get; set; }
    public decimal MonthlyUpkeep { get; set; }
    public double GdpMultiplier { get; set; }
    public double FarmEfficiencyMultiplier { get; set; }
    public double FactoryEfficiencyMultiplier { get; set; }
    public double EnergyEfficiencyMultiplier { get; set; }
    public double TaxEfficiencyModifier { get; set; }
    public double ApprovalModifier { get; set; }
    public double LivingStandardModifier { get; set; }
    public double StabilityModifier { get; set; }
    public decimal RequiredTreasury { get; set; }
}

public class EconomicPolicyRegistry
{
    private readonly Dictionary<string, EconomicPolicyDefinition> _policies = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<EconomicPolicyDefinition> Policies => _policies.Values;

    public void Register(EconomicPolicyDefinition policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (string.IsNullOrWhiteSpace(policy.Id))
        {
            throw new ArgumentException("Policy ID cannot be empty.", nameof(policy));
        }
        _policies[policy.Id] = policy;
    }

    public bool TryGet(string id, out EconomicPolicyDefinition? policy) =>
        _policies.TryGetValue(id, out policy);

    public EconomicPolicyDefinition Get(string id)
    {
        if (!TryGet(id, out var policy) || policy == null)
        {
            throw new KeyNotFoundException($"Economic policy '{id}' was not found in registry.");
        }
        return policy;
    }

    public void LoadFromJson(string json)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var list = JsonSerializer.Deserialize<List<EconomicPolicyDefinition>>(json, options);
        if (list == null) return;

        foreach (var item in list)
        {
            Register(item);
        }
    }

    public static EconomicPolicyRegistry CreateDefaultCatalog()
    {
        var registry = new EconomicPolicyRegistry();

        registry.Register(new EconomicPolicyDefinition
        {
            Id = "agricultural_subsidies",
            Name = "Agricultural Subsidies & Modernization",
            Description = "Subsidize agricultural machinery, high-yield seeds, and rural irrigation to secure food independence.",
            Category = "Agriculture",
            UpfrontCost = 500_000_000m,
            MonthlyUpkeep = 200_000_000m,
            FarmEfficiencyMultiplier = 0.25,
            ApprovalModifier = 3.0,
            LivingStandardModifier = 2.0
        });

        registry.Register(new EconomicPolicyDefinition
        {
            Id = "industrial_modernization",
            Name = "Industrial Automation & Modernization",
            Description = "Equip domestic factories with advanced robotics and automation, boosting high-value goods manufacturing.",
            Category = "Industry",
            UpfrontCost = 1_000_000_000m,
            MonthlyUpkeep = 400_000_000m,
            FactoryEfficiencyMultiplier = 0.30,
            GdpMultiplier = 0.05,
            TaxEfficiencyModifier = 0.05,
            LivingStandardModifier = 2.0
        });

        registry.Register(new EconomicPolicyDefinition
        {
            Id = "green_energy_transition",
            Name = "Renewable Clean Energy Transition",
            Description = "Deploy grid-scale solar and wind arrays to lower power generation costs and improve urban air quality.",
            Category = "Energy",
            UpfrontCost = 1_500_000_000m,
            MonthlyUpkeep = 350_000_000m,
            EnergyEfficiencyMultiplier = 0.20,
            LivingStandardModifier = 4.0,
            ApprovalModifier = 2.0
        });

        registry.Register(new EconomicPolicyDefinition
        {
            Id = "fdi_promotion_zones",
            Name = "Special Economic Zones & FDI Incentives",
            Description = "Create tariff-free manufacturing export hubs that attract global corporations and direct investments.",
            Category = "Trade",
            UpfrontCost = 800_000_000m,
            MonthlyUpkeep = 250_000_000m,
            GdpMultiplier = 0.08,
            TaxEfficiencyModifier = 0.08,
            StabilityModifier = 1.0
        });

        registry.Register(new EconomicPolicyDefinition
        {
            Id = "national_infrastructure_initiative",
            Name = "National Freight & Logistics Corridors",
            Description = "Construct dedicated high-capacity freight rail and port expressways, accelerating commodity transit.",
            Category = "Infrastructure",
            UpfrontCost = 2_000_000_000m,
            MonthlyUpkeep = 500_000_000m,
            GdpMultiplier = 0.06,
            LivingStandardModifier = 5.0,
            StabilityModifier = 3.0
        });

        registry.Register(new EconomicPolicyDefinition
        {
            Id = "fiscal_austerity_program",
            Name = "Fiscal Austerity & Civil Expenditure Cap",
            Description = "Cap administrative operational spending and freeze discretionary allowances to accelerate debt reduction.",
            Category = "Fiscal",
            UpfrontCost = 100_000_000m,
            MonthlyUpkeep = -300_000_000m, // Net expenditure saving!
            ApprovalModifier = -5.0,
            LivingStandardModifier = -2.0,
            StabilityModifier = -2.0
        });

        return registry;
    }
}
