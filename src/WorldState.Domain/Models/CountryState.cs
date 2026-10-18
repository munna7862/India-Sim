namespace WorldState.Domain.Models;

public class CountryState
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Treasury { get; set; }
    public long Population { get; set; }
    public double ApprovalRating { get; set; } // 0.0 to 100.0
    public double QualityOfLiving { get; set; } // 0.0 to 100.0
    public double Stability { get; set; } // 0.0 to 100.0

    // Tax Sliders (0.0 to 1.0)
    public double IncomeTaxRate { get; set; } = 0.20;
    public double CorporateTaxRate { get; set; } = 0.25;
    public double VatRate { get; set; } = 0.15;

    // Government & Ideology
    public IdeologyType Ideology { get; set; } = IdeologyType.ConstitutionalDemocracy;
    public int IdeologyCooldownMonths { get; set; } = 0;

    // Cabinet ministries
    public Dictionary<MinistryType, MinistryState> Ministries { get; set; } = new();

    // Resources
    public ResourceStockpile Stockpiles { get; set; } = ResourceStockpile.DefaultStartingStockpile();

    // National Indicators & Macroeconomics
    public decimal Debt { get; set; } = 0m;
    public string CreditRating { get; set; } = "AAA";
    public double CorruptionIndex { get; set; } = 25.0; // 0.0 (Clean) to 100.0 (Severe Corruption)
    public double InflationRate { get; set; } = 0.05; // 5.0%

    // Active Economic Policies
    public List<string> ActivePolicies { get; set; } = new();

    // Internal Security & Espionage
    public FundingLevel PresidentialProtectionLevel { get; set; } = FundingLevel.Medium;

    // Military Inventory (Unit ID -> Count)
    public Dictionary<string, int> MilitaryUnits { get; set; } = new();

    // Fiscal Ledger & Trend History
    public List<MonthlyBudgetReport> BudgetHistory { get; set; } = new();
    public MonthlyBudgetReport? LastBudgetReport { get; set; }

    // Foreign Relations
    public Dictionary<string, DiplomaticStance> DiplomaticRelations { get; set; } = new();

    // Facility counts
    public Dictionary<string, int> Facilities { get; set; } = new()
    {
        ["Farms"] = 50,
        ["Mines"] = 25,
        ["PowerPlants"] = 30,
        ["Factories"] = 40,
        ["Hospitals"] = 20,
        ["Schools"] = 30,
        ["Barracks"] = 15,
        ["OilWells"] = 20,
        ["UraniumMines"] = 5
    };


    public decimal CalculateMonthlyGdp()
    {
        // Base monthly output: $100/person, modified by factories and quality of living
        decimal baseGdp = (decimal)Population * 100m;
        int factories = Facilities.GetValueOrDefault("Factories", 10);
        decimal industrialMultiplier = 1.0m + (factories / 200m);
        decimal livingMultiplier = 0.8m + ((decimal)QualityOfLiving / 250m);
        return baseGdp * industrialMultiplier * livingMultiplier;
    }

    public string CalculateCreditRating()
    {
        decimal annualGdp = CalculateMonthlyGdp() * 12m;
        if (annualGdp <= 0m) return "D";

        double debtToGdp = (double)(Debt / annualGdp);

        if (Treasury < -50_000_000_000m) return "D";
        if (Treasury < -10_000_000_000m || debtToGdp >= 1.30) return "CCC";
        if (debtToGdp >= 1.10) return "B";
        if (debtToGdp >= 0.90) return "BB";
        if (debtToGdp >= 0.70) return "BBB";
        if (debtToGdp >= 0.50) return "A";
        if (debtToGdp >= 0.30) return "AA";
        return "AAA";
    }

    public decimal GetMonthlyInterestRate() => CreditRating switch
    {
        "AAA" => 0.0050m, // 6.0% annual
        "AA" => 0.0060m,  // 7.2% annual
        "A" => 0.0075m,   // 9.0% annual
        "BBB" => 0.0090m, // 10.8% annual
        "BB" => 0.0120m,  // 14.4% annual
        "B" => 0.0150m,   // 18.0% annual
        "CCC" => 0.0200m, // 24.0% annual
        _ => 0.0300m      // 36.0% annual for Default / D
    };

    public DiplomaticStance GetRelationsWith(string otherCountryId)
    {
        if (!DiplomaticRelations.TryGetValue(otherCountryId, out var stance))
        {
            stance = new DiplomaticStance();
            DiplomaticRelations[otherCountryId] = stance;
        }
        return stance;
    }

    public int GetTotalMilitaryUnits() => MilitaryUnits.Values.Sum();

    public int CalculateTotalCombatPower(Military.MilitaryUnitRegistry? registry = null)
    {
        registry ??= Military.MilitaryUnitRegistry.CreateDefaultCatalog();
        int total = 0;
        foreach (var (unitId, count) in MilitaryUnits)
        {
            if (registry.TryGet(unitId, out var def) && def != null)
            {
                total += def.CombatPower * count;
            }
        }
        return total;
    }

    public int CalculateTotalDefensePower(Military.MilitaryUnitRegistry? registry = null)
    {
        registry ??= Military.MilitaryUnitRegistry.CreateDefaultCatalog();
        int total = 0;
        foreach (var (unitId, count) in MilitaryUnits)
        {
            if (registry.TryGet(unitId, out var def) && def != null)
            {
                total += def.DefensePower * count;
            }
        }
        return total;
    }

    public decimal CalculateMonthlyMilitaryUpkeep(Military.MilitaryUnitRegistry? registry = null)
    {
        registry ??= Military.MilitaryUnitRegistry.CreateDefaultCatalog();
        decimal total = 0m;
        foreach (var (unitId, count) in MilitaryUnits)
        {
            if (registry.TryGet(unitId, out var def) && def != null)
            {
                total += def.MonthlyUpkeepTreasury * count;
            }
        }
        return total;
    }

    public static CountryState CreateIndiaDefault()
    {
        var india = new CountryState
        {
            Id = "IND",
            Name = "Republic of India",
            Treasury = 50_000_000_000m, // $50B liquid starting funds
            Population = 1_400_000_000L, // 1.4 Billion
            ApprovalRating = 65.0,
            QualityOfLiving = 55.0,
            Stability = 70.0,
            IncomeTaxRate = 0.20,
            CorporateTaxRate = 0.25,
            VatRate = 0.15,
            PresidentialProtectionLevel = FundingLevel.Medium,
            Stockpiles = ResourceStockpile.DefaultStartingStockpile(),
            MilitaryUnits = new Dictionary<string, int>
            {
                ["infantry_division"] = 250,
                ["main_battle_tank"] = 50,
                ["artillery_corps"] = 35,
                ["mechanized_infantry"] = 30,
                ["multirole_fighter"] = 40,
                ["attack_helicopter"] = 20,
                ["strategic_bomber"] = 5,
                ["air_defense_system"] = 20,
                ["recon_drone_fleet"] = 25,
                ["guided_missile_destroyer"] = 10,
                ["attack_submarine"] = 6,
                ["aircraft_carrier"] = 2
            }
        };

        foreach (MinistryType ministry in Enum.GetValues<MinistryType>())
        {
            india.Ministries[ministry] = new MinistryState(ministry, null, FundingLevel.Medium);
        }

        return india;
    }
}

