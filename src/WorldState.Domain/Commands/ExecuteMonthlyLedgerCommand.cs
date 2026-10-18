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

        // 1. Calculate Monthly Revenue:
        // Base economic output per capita: $100/month
        decimal grossGdpPerMonth = (decimal)country.Population * 100m;
        decimal incomeTaxRevenue = grossGdpPerMonth * (decimal)country.IncomeTaxRate * 0.40m;
        decimal corporateTaxRevenue = grossGdpPerMonth * (decimal)country.CorporateTaxRate * 0.35m;
        decimal vatRevenue = grossGdpPerMonth * (decimal)country.VatRate * 0.25m;
        decimal rawRevenue = incomeTaxRevenue + corporateTaxRevenue + vatRevenue;
        var ideologyDef = IdeologyDefinition.Get(country.Ideology);
        decimal totalRevenue = rawRevenue * (decimal)ideologyDef.TaxEfficiencyMultiplier;

        if (country.IdeologyCooldownMonths > 0)
        {
            country.IdeologyCooldownMonths--;
        }

        // 2. Calculate Monthly Expenses:
        decimal totalExpenses = 0m;
        foreach (var (_, ministryState) in country.Ministries)
        {
            // Base funding level operational cost ($50M per tier)
            decimal operationalCost = (int)ministryState.Funding * 1_000_000m;
            decimal salary = ministryState.ActiveMinister?.MonthlySalary ?? 0m;
            totalExpenses += operationalCost + salary;
        }

        // Baseline defense and civil administration ($500M)
        totalExpenses += 500_000_000m;

        // Debt service interest (0.5% monthly = 6% per annum)
        decimal debtInterest = country.Debt * 0.005m;
        totalExpenses += debtInterest;

        // 3. Invariant net calculation:
        decimal netDelta = totalRevenue - totalExpenses;
        country.Treasury += netDelta;

        // 4. Resource updates (Facilities production minus civilian consumption):
        int farms = country.Facilities.GetValueOrDefault("Farms", 10);
        int powerPlants = country.Facilities.GetValueOrDefault("PowerPlants", 10);
        int mines = country.Facilities.GetValueOrDefault("Mines", 10);

        double foodProduced = farms * 20_000.0;
        double foodConsumed = (country.Population / 1_000_000.0) * 10_000.0;

        double energyProduced = powerPlants * 15_000.0;
        double energyConsumed = (country.Population / 1_000_000.0) * 8_000.0;

        double ironProduced = mines * 5_000.0;

        var current = country.Stockpiles;
        country.Stockpiles = new ResourceStockpile(
            Food: Math.Max(0.0, current.Food + foodProduced - foodConsumed),
            Energy: Math.Max(0.0, current.Energy + energyProduced - energyConsumed),
            Oil: current.Oil,
            Iron: current.Iron + ironProduced,
            Uranium: current.Uranium,
            ConsumerGoods: current.ConsumerGoods
        );

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Monthly Ledger Settled",
            Message: $"{country.Name} financial ledger: Revenue ${totalRevenue / 1_000_000m:F1}M, Expenses ${totalExpenses / 1_000_000m:F1}M (Net: {(netDelta >= 0 ? "+" : "")}${netDelta / 1_000_000m:F1}M). Treasury: ${country.Treasury / 1_000_000m:F1}M.",
            Category: EventCategory.Economy,
            Severity: country.Treasury < 0m ? EventSeverity.Warning : EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
