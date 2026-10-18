namespace WorldState.Domain.Models;

public record MonthlyBudgetReport
{
    public DateOnly Date { get; init; }
    public string CountryId { get; init; } = string.Empty;

    // Revenues
    public decimal IncomeTaxRevenue { get; init; }
    public decimal CorporateTaxRevenue { get; init; }
    public decimal VatRevenue { get; init; }
    public decimal CommercialFacilityRevenue { get; init; }
    public decimal TradeExportRevenue { get; init; }
    public decimal GrossRevenue { get; init; }
    public double TaxEfficiency { get; init; } = 1.0;

    // Expenses
    public decimal MinistryOperationalExpenses { get; init; }
    public decimal MinisterSalaries { get; init; }
    public decimal CivilAdministrationExpenses { get; init; }
    public decimal DefenseUpkeepExpenses { get; init; }
    public decimal MilitaryUnitUpkeepExpenses { get; init; }
    public decimal FacilityUpkeepExpenses { get; init; }
    public decimal DebtInterestExpenses { get; init; }
    public decimal ActivePolicyUpkeepExpenses { get; init; }
    public decimal TradeImportExpenses { get; init; }
    public decimal GrossExpenses { get; init; }

    // Net Delta & Macro
    public decimal NetBalance { get; init; }
    public decimal StartingTreasury { get; init; }
    public decimal EndingTreasury { get; init; }
    public decimal StartingDebt { get; init; }
    public decimal EndingDebt { get; init; }
    public decimal MonthlyGdp { get; init; }
    public decimal AnnualGdp { get; init; }
    public double DebtToGdpRatio { get; init; }
    public string CreditRating { get; init; } = "AAA";
    public double InflationRate { get; init; }
}
