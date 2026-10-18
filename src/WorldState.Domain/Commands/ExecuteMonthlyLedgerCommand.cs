using WorldState.Domain.Economy;
using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class ExecuteMonthlyLedgerCommand : ICommand
{
    public string CountryId { get; }

    public ExecuteMonthlyLedgerCommand(string countryId)
    {
        CountryId = countryId;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found.");
        }

        // 0. Resolve active economic policy effects
        double gdpBonus = 0.0;
        double farmBonus = 0.0;
        double factoryBonus = 0.0;
        double energyBonus = 0.0;
        double taxBonus = 0.0;
        decimal policyUpkeep = 0m;

        var policyRegistry = session.PolicyRegistry ?? EconomicPolicyRegistry.CreateDefaultCatalog();
        foreach (var policyId in country.ActivePolicies)
        {
            if (policyRegistry.TryGet(policyId, out var policy) && policy != null)
            {
                gdpBonus += policy.GdpMultiplier;
                farmBonus += policy.FarmEfficiencyMultiplier;
                factoryBonus += policy.FactoryEfficiencyMultiplier;
                energyBonus += policy.EnergyEfficiencyMultiplier;
                taxBonus += policy.TaxEfficiencyModifier;
                policyUpkeep += policy.MonthlyUpkeep;
            }
        }

        // 1. Calculate Monthly Revenue:
        decimal baseGdp = country.CalculateMonthlyGdp();
        decimal monthlyGdp = baseGdp * (1.0m + (decimal)gdpBonus);
        decimal annualGdp = monthlyGdp * 12m;

        decimal incomeTaxRevenue = monthlyGdp * (decimal)country.IncomeTaxRate * 0.40m;
        decimal corporateTaxRevenue = monthlyGdp * (decimal)country.CorporateTaxRate * 0.35m;
        decimal vatRevenue = monthlyGdp * (decimal)country.VatRate * 0.25m;

        int factories = country.Facilities.GetValueOrDefault("Factories", 10);
        decimal commercialRevenue = factories * 2_500_000m * (1.0m + (decimal)factoryBonus);

        decimal rawRevenue = incomeTaxRevenue + corporateTaxRevenue + vatRevenue + commercialRevenue;
        var ideologyDef = IdeologyDefinition.Get(country.Ideology);
        double totalTaxEfficiency = Math.Max(0.10, ideologyDef.TaxEfficiencyMultiplier + taxBonus - (country.CorruptionIndex / 250.0));
        decimal grossRevenue = rawRevenue * (decimal)totalTaxEfficiency;

        if (country.IdeologyCooldownMonths > 0)
        {
            country.IdeologyCooldownMonths--;
        }

        // 2. Calculate Monthly Expenses:
        decimal ministryExpenses = 0m;
        decimal ministerSalaries = 0m;
        foreach (var (_, ministryState) in country.Ministries)
        {
            ministryExpenses += (int)ministryState.Funding * 1_000_000m;
            ministerSalaries += ministryState.ActiveMinister?.MonthlySalary ?? 0m;
        }

        decimal civilAdminExpenses = 500_000_000m;
        int barracks = country.Facilities.GetValueOrDefault("Barracks", 10);
        decimal defenseExpenses = 200_000_000m + (barracks * 5_000_000m);

        int totalFacilities = country.Facilities.Values.Sum();
        decimal facilityUpkeepExpenses = totalFacilities * 500_000m;

        decimal monthlyInterestRate = country.GetMonthlyInterestRate();
        decimal debtInterestExpenses = country.Debt * monthlyInterestRate;

        decimal grossExpenses = ministryExpenses + ministerSalaries + civilAdminExpenses + defenseExpenses + facilityUpkeepExpenses + debtInterestExpenses + policyUpkeep;

        // 3. Invariant Net Calculation:
        decimal startingTreasury = country.Treasury;
        decimal netDelta = grossRevenue - grossExpenses;
        country.Treasury += netDelta;

        // Update Credit Rating based on debt-to-GDP and liquid balance
        country.CreditRating = country.CalculateCreditRating();

        // 4. Resource updates (Facilities production minus civilian consumption):
        int farms = country.Facilities.GetValueOrDefault("Farms", 10);
        int powerPlants = country.Facilities.GetValueOrDefault("PowerPlants", 10);
        int mines = country.Facilities.GetValueOrDefault("Mines", 10);
        int oilWells = country.Facilities.GetValueOrDefault("OilWells", 10);
        int uraniumMines = country.Facilities.GetValueOrDefault("UraniumMines", 5);

        double foodProduced = farms * 20_000.0 * (1.0 + farmBonus);
        double foodConsumed = (country.Population / 1_000_000.0) * 700.0;

        double energyProduced = powerPlants * 15_000.0 * (1.0 + energyBonus);
        double energyConsumed = (country.Population / 1_000_000.0) * 300.0;

        double ironProduced = mines * 5_000.0;
        double ironConsumedByFactories = factories * 200.0;

        double oilProduced = oilWells * 10_000.0;
        double oilConsumed = (country.Population / 1_000_000.0) * 120.0;

        double uraniumProduced = uraniumMines * 1_000.0;
        double uraniumConsumed = powerPlants * 50.0;

        double goodsProduced = factories * 5_000.0 * (1.0 + factoryBonus);
        double goodsConsumed = (country.Population / 1_000_000.0) * 120.0;

        var current = country.Stockpiles;
        double nextFood = Math.Max(0.0, current.Food + foodProduced - foodConsumed);
        double nextEnergy = Math.Max(0.0, current.Energy + energyProduced - energyConsumed);
        double nextIron = Math.Max(0.0, current.Iron + ironProduced - ironConsumedByFactories);
        double nextOil = Math.Max(0.0, current.Oil + oilProduced - oilConsumed);
        double nextUranium = Math.Max(0.0, current.Uranium + uraniumProduced - uraniumConsumed);
        double nextGoods = Math.Max(0.0, current.ConsumerGoods + goodsProduced - goodsConsumed);

        country.Stockpiles = new ResourceStockpile(
            Food: nextFood,
            Energy: nextEnergy,
            Oil: nextOil,
            Iron: nextIron,
            Uranium: nextUranium,
            ConsumerGoods: nextGoods
        );

        // Check for severe resource shortages and apply penalties
        if (nextFood <= 0.0 && foodConsumed > foodProduced)
        {
            country.ApprovalRating = Math.Max(0.0, country.ApprovalRating - 3.0);
            country.Stability = Math.Max(0.0, country.Stability - 2.0);
        }
        if (nextEnergy <= 0.0 && energyConsumed > energyProduced)
        {
            country.QualityOfLiving = Math.Max(0.0, country.QualityOfLiving - 2.0);
            country.ApprovalRating = Math.Max(0.0, country.ApprovalRating - 2.0);
        }

        // 5. Build and archive Monthly Budget Report
        var report = new MonthlyBudgetReport
        {
            Date = session.Clock.CurrentDate,
            CountryId = CountryId,
            IncomeTaxRevenue = incomeTaxRevenue,
            CorporateTaxRevenue = corporateTaxRevenue,
            VatRevenue = vatRevenue,
            CommercialFacilityRevenue = commercialRevenue,
            GrossRevenue = grossRevenue,
            TaxEfficiency = totalTaxEfficiency,

            MinistryOperationalExpenses = ministryExpenses,
            MinisterSalaries = ministerSalaries,
            CivilAdministrationExpenses = civilAdminExpenses,
            DefenseUpkeepExpenses = defenseExpenses,
            FacilityUpkeepExpenses = facilityUpkeepExpenses,
            DebtInterestExpenses = debtInterestExpenses,
            ActivePolicyUpkeepExpenses = policyUpkeep,
            GrossExpenses = grossExpenses,

            NetBalance = netDelta,
            StartingTreasury = startingTreasury,
            EndingTreasury = country.Treasury,
            StartingDebt = country.Debt,
            EndingDebt = country.Debt,
            MonthlyGdp = monthlyGdp,
            AnnualGdp = annualGdp,
            DebtToGdpRatio = (double)(country.Debt / (annualGdp > 0 ? annualGdp : 1m)),
            CreditRating = country.CreditRating,
            InflationRate = country.InflationRate
        };

        country.LastBudgetReport = report;
        country.BudgetHistory.Add(report);
        if (country.BudgetHistory.Count > 24)
        {
            country.BudgetHistory.RemoveAt(0);
        }

        // 6. Emit Monthly Ledger Settled Event
        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Monthly Ledger Settled",
            Message: $"{country.Name} financial ledger: Revenue ${grossRevenue / 1_000_000m:F1}M, Expenses ${grossExpenses / 1_000_000m:F1}M (Net: {(netDelta >= 0 ? "+" : "")}${netDelta / 1_000_000m:F1}M). Treasury: ${country.Treasury / 1_000_000m:F1}M.",
            Category: EventCategory.Economy,
            Severity: country.Treasury < 0m ? EventSeverity.Warning : EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        // 7. Check for Sovereign Debt Distress alert
        if (country.Treasury < -10_000_000_000m || country.CreditRating is "CCC" or "D")
        {
            session.AddEvent(new GameEvent(
                Id: Guid.NewGuid().ToString("N"),
                Date: session.Clock.CurrentDate,
                Title: "Sovereign Debt Distress Alert",
                Message: $"{country.Name} has entered acute sovereign debt distress! Credit rating downgraded to {country.CreditRating}. Urgent budget consolidation required.",
                Category: EventCategory.Economy,
                Severity: EventSeverity.Critical,
                ActorCountryId: CountryId
            ));
        }

        return CommandResult.Success();
    }
}
