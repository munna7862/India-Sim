import { DateOnly } from './DateOnly';

export interface MonthlyBudgetReport {
  date: DateOnly;
  countryId: string;

  // Revenues
  incomeTaxRevenue: number;
  corporateTaxRevenue: number;
  vatRevenue: number;
  commercialFacilityRevenue: number;
  tradeExportRevenue: number;
  grossRevenue: number;
  taxEfficiency: number;

  // Expenses
  ministryOperationalExpenses: number;
  ministerSalaries: number;
  civilAdministrationExpenses: number;
  defenseUpkeepExpenses: number;
  militaryUnitUpkeepExpenses: number;
  facilityUpkeepExpenses: number;
  debtInterestExpenses: number;
  activePolicyUpkeepExpenses: number;
  tradeImportExpenses: number;
  grossExpenses: number;

  // Net Delta & Macro
  netBalance: number;
  startingTreasury: number;
  endingTreasury: number;
  startingDebt: number;
  endingDebt: number;
  monthlyGdp: number;
  annualGdp: number;
  debtToGdpRatio: number;
  creditRating: string;
  inflationRate: number;
}
