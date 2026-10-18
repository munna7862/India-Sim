using WorldState.Domain.Commands;
using WorldState.Domain.Economy;
using WorldState.Domain.Models;

namespace WorldState.Domain.ViewModels;

public record FacilityCardViewModel(
    string FacilityType,
    int CurrentCount,
    decimal MoneyCost,
    double IronCost,
    string PrimaryProduction,
    bool CanAfford
);

public record PolicyCardViewModel(
    string Id,
    string Name,
    string Description,
    string Category,
    decimal UpfrontCost,
    decimal MonthlyUpkeep,
    bool IsActive,
    bool CanAfford,
    string EffectSummary
);

public record EconomyViewModel(
    string CountryId,
    string CountryName,
    decimal Treasury,
    decimal Debt,
    decimal DebtCeiling,
    double DebtToGdpRatio,
    string CreditRating,
    decimal MonthlyInterestRate,
    decimal MonthlyGdp,
    decimal AnnualGdp,
    double InflationRate,
    double CorruptionIndex,

    // Tax Sliders & Rates
    double IncomeTaxRate,
    double CorporateTaxRate,
    double VatRate,

    // Revenue Projections
    decimal ProjectedIncomeTaxRevenue,
    decimal ProjectedCorporateTaxRevenue,
    decimal ProjectedVatRevenue,
    decimal ProjectedCommercialRevenue,
    decimal ProjectedGrossRevenue,
    double ProjectedTaxEfficiency,

    // Expense Projections
    decimal ProjectedMinistryExpenses,
    decimal ProjectedCivilAdminExpenses,
    decimal ProjectedDefenseExpenses,
    decimal ProjectedFacilityUpkeep,
    decimal ProjectedDebtInterest,
    decimal ProjectedPolicyUpkeep,
    decimal ProjectedGrossExpenses,
    decimal ProjectedNetBalance,

    // Stockpiles
    double FoodStockpile,
    double EnergyStockpile,
    double OilStockpile,
    double IronStockpile,
    double UraniumStockpile,
    double ConsumerGoodsStockpile,

    // Sub-collections
    IReadOnlyList<FacilityCardViewModel> Facilities,
    IReadOnlyList<PolicyCardViewModel> Policies,
    IReadOnlyList<MonthlyBudgetReport> History,
    MonthlyBudgetReport? LastReport
)
{
    public static EconomyViewModel FromCountry(CountryState country, EconomicPolicyRegistry? registry = null)
    {
        registry ??= EconomicPolicyRegistry.CreateDefaultCatalog();

        // Macro GDP
        decimal monthlyGdp = country.CalculateMonthlyGdp();
        decimal annualGdp = monthlyGdp * 12m;
        decimal debtCeiling = Math.Max(100_000_000_000m, (decimal)country.Population * 200m);
        double debtToGdp = annualGdp > 0 ? (double)(country.Debt / annualGdp) : 0.0;
        string creditRating = country.CalculateCreditRating();
        decimal interestRate = country.GetMonthlyInterestRate();

        // Policy bonuses
        double gdpBonus = 0.0;
        double factoryBonus = 0.0;
        double taxBonus = 0.0;
        decimal policyUpkeep = 0m;

        foreach (var policyId in country.ActivePolicies)
        {
            if (registry.TryGet(policyId, out var p) && p != null)
            {
                gdpBonus += p.GdpMultiplier;
                factoryBonus += p.FactoryEfficiencyMultiplier;
                taxBonus += p.TaxEfficiencyModifier;
                policyUpkeep += p.MonthlyUpkeep;
            }
        }

        decimal effectiveMonthlyGdp = monthlyGdp * (1.0m + (decimal)gdpBonus);
        decimal incomeTaxRev = effectiveMonthlyGdp * (decimal)country.IncomeTaxRate * 0.40m;
        decimal corpTaxRev = effectiveMonthlyGdp * (decimal)country.CorporateTaxRate * 0.35m;
        decimal vatRev = effectiveMonthlyGdp * (decimal)country.VatRate * 0.25m;

        int factories = country.Facilities.GetValueOrDefault("Factories", 10);
        decimal commRev = factories * 2_500_000m * (1.0m + (decimal)factoryBonus);

        var ideologyDef = IdeologyDefinition.Get(country.Ideology);
        double taxEff = Math.Max(0.10, ideologyDef.TaxEfficiencyMultiplier + taxBonus - (country.CorruptionIndex / 250.0));
        decimal grossRev = (incomeTaxRev + corpTaxRev + vatRev + commRev) * (decimal)taxEff;

        // Expenses
        decimal ministryExp = 0m;
        foreach (var (_, m) in country.Ministries)
        {
            ministryExp += (int)m.Funding * 1_000_000m;
            ministryExp += m.ActiveMinister?.MonthlySalary ?? 0m;
        }

        decimal civilExp = 500_000_000m;
        int barracks = country.Facilities.GetValueOrDefault("Barracks", 10);
        decimal defenseExp = 200_000_000m + (barracks * 5_000_000m);
        int totalFacs = country.Facilities.Values.Sum();
        decimal facUpkeep = totalFacs * 500_000m;
        decimal debtInterest = country.Debt * interestRate;

        decimal grossExp = ministryExp + civilExp + defenseExp + facUpkeep + debtInterest + policyUpkeep;
        decimal netBal = grossRev - grossExp;

        // Facility cards
        var facCards = new List<FacilityCardViewModel>();
        foreach (var (facType, (moneyCost, ironCost)) in ConstructFacilityCommand.CostTable)
        {
            int count = country.Facilities.GetValueOrDefault(facType, 0);
            string prod = facType switch
            {
                "Farms" => "+20,000 Food / mo",
                "Mines" => "+5,000 Iron / mo",
                "PowerPlants" => "+15,000 Energy / mo",
                "Factories" => "+5,000 Consumer Goods & $2.5M Commercial Rev",
                "OilWells" => "+10,000 Oil / mo",
                "UraniumMines" => "+1,000 Uranium / mo",
                "Hospitals" => "+Quality of Living & Health Index",
                "Schools" => "+Quality of Living & Education Index",
                "Barracks" => "+Military Readiness & Force Projection",
                _ => "General Output"
            };

            bool canAfford = country.Treasury >= moneyCost && country.Stockpiles.Iron >= ironCost;
            facCards.Add(new FacilityCardViewModel(facType, count, moneyCost, ironCost, prod, canAfford));
        }

        // Policy cards
        var polCards = new List<PolicyCardViewModel>();
        foreach (var policy in registry.Policies)
        {
            bool isActive = country.ActivePolicies.Contains(policy.Id, StringComparer.OrdinalIgnoreCase);
            bool canAfford = !isActive && country.Treasury >= policy.UpfrontCost;

            var effects = new List<string>();
            if (policy.GdpMultiplier != 0) effects.Add($"GDP +{policy.GdpMultiplier * 100:F0}%");
            if (policy.FarmEfficiencyMultiplier != 0) effects.Add($"Farm Yield +{policy.FarmEfficiencyMultiplier * 100:F0}%");
            if (policy.FactoryEfficiencyMultiplier != 0) effects.Add($"Factory Output +{policy.FactoryEfficiencyMultiplier * 100:F0}%");
            if (policy.EnergyEfficiencyMultiplier != 0) effects.Add($"Energy Yield +{policy.EnergyEfficiencyMultiplier * 100:F0}%");
            if (policy.TaxEfficiencyModifier != 0) effects.Add($"Tax Revenue +{policy.TaxEfficiencyModifier * 100:F0}%");
            if (policy.ApprovalModifier != 0) effects.Add($"Approval {(policy.ApprovalModifier > 0 ? "+" : "")}{policy.ApprovalModifier:F0}%");
            if (policy.LivingStandardModifier != 0) effects.Add($"Living Std {(policy.LivingStandardModifier > 0 ? "+" : "")}{policy.LivingStandardModifier:F0}%");
            if (policy.StabilityModifier != 0) effects.Add($"Stability {(policy.StabilityModifier > 0 ? "+" : "")}{policy.StabilityModifier:F0}%");

            string effectSummary = string.Join(", ", effects);
            polCards.Add(new PolicyCardViewModel(
                policy.Id,
                policy.Name,
                policy.Description,
                policy.Category,
                policy.UpfrontCost,
                policy.MonthlyUpkeep,
                isActive,
                canAfford,
                effectSummary
            ));
        }

        return new EconomyViewModel(
            CountryId: country.Id,
            CountryName: country.Name,
            Treasury: country.Treasury,
            Debt: country.Debt,
            DebtCeiling: debtCeiling,
            DebtToGdpRatio: debtToGdp,
            CreditRating: creditRating,
            MonthlyInterestRate: interestRate,
            MonthlyGdp: effectiveMonthlyGdp,
            AnnualGdp: annualGdp,
            InflationRate: country.InflationRate,
            CorruptionIndex: country.CorruptionIndex,

            IncomeTaxRate: country.IncomeTaxRate,
            CorporateTaxRate: country.CorporateTaxRate,
            VatRate: country.VatRate,

            ProjectedIncomeTaxRevenue: incomeTaxRev,
            ProjectedCorporateTaxRevenue: corpTaxRev,
            ProjectedVatRevenue: vatRev,
            ProjectedCommercialRevenue: commRev,
            ProjectedGrossRevenue: grossRev,
            ProjectedTaxEfficiency: taxEff,

            ProjectedMinistryExpenses: ministryExp,
            ProjectedCivilAdminExpenses: civilExp,
            ProjectedDefenseExpenses: defenseExp,
            ProjectedFacilityUpkeep: facUpkeep,
            ProjectedDebtInterest: debtInterest,
            ProjectedPolicyUpkeep: policyUpkeep,
            ProjectedGrossExpenses: grossExp,
            ProjectedNetBalance: netBal,

            FoodStockpile: country.Stockpiles.Food,
            EnergyStockpile: country.Stockpiles.Energy,
            OilStockpile: country.Stockpiles.Oil,
            IronStockpile: country.Stockpiles.Iron,
            UraniumStockpile: country.Stockpiles.Uranium,
            ConsumerGoodsStockpile: country.Stockpiles.ConsumerGoods,

            Facilities: facCards,
            Policies: polCards,
            History: country.BudgetHistory.ToList(),
            LastReport: country.LastBudgetReport
        );
    }
}
