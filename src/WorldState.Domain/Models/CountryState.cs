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
        ["Barracks"] = 15
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
            Stockpiles = ResourceStockpile.DefaultStartingStockpile()
        };

        foreach (MinistryType ministry in Enum.GetValues<MinistryType>())
        {
            india.Ministries[ministry] = new MinistryState(ministry, null, FundingLevel.Medium);
        }

        return india;
    }
}
