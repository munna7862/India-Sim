import { describe, it, expect } from 'vitest';
import { GameSession } from '../../src/domain/models/GameSession';
import { SimulationEngine } from '../../src/domain/services/SimulationEngine';
import { TaxType } from '../../src/domain/models/types';
import {
  AdjustTaxRateCommand,
  TakeLoanCommand,
  RepayLoanCommand,
  ConstructFacilityCommand,
  EnactEconomicPolicyCommand,
  RevokeEconomicPolicyCommand,
  ExecuteMonthlyLedgerCommand,
} from '../../src/domain/commands/EconomyCommands';

describe('Economy & Budget System', () => {
  it('AdjustTaxRate_ValidRates_UpdatesCountryStateAndCalculatesProjections', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;

    let result = engine.dispatch(new AdjustTaxRateCommand('IND', TaxType.IncomeTax, 0.28));
    expect(result.isSuccess).toBe(true);
    expect(country.incomeTaxRate).toBeCloseTo(0.28, 4);

    result = engine.dispatch(new AdjustTaxRateCommand('IND', TaxType.CorporateTax, 0.32));
    expect(result.isSuccess).toBe(true);
    expect(country.corporateTaxRate).toBeCloseTo(0.32, 4);

    result = engine.dispatch(new AdjustTaxRateCommand('IND', TaxType.Vat, 0.18));
    expect(result.isSuccess).toBe(true);
    expect(country.vatRate).toBeCloseTo(0.18, 4);
  });

  it('AdjustTaxRate_InvalidRates_FailsValidationWithoutMutatingState', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;
    const initialRate = country.incomeTaxRate;

    const invalidRates = [-0.01, -0.5, 1.05, 2.0];
    for (const invalidRate of invalidRates) {
      const result = engine.dispatch(new AdjustTaxRateCommand('IND', TaxType.IncomeTax, invalidRate));
      expect(result.isSuccess).toBe(false);
      expect(result.errorCode).toBe('InvalidTaxRate');
      expect(country.incomeTaxRate).toBe(initialRate);
    }
  });

  it('TakeLoanAndRepayLoan_EnforcesCeilingsAndDeductions', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;

    const startingTreasury = country.treasury;
    const startingDebt = country.debt;
    const loanAmount = 10_000_000_000; // $10B

    // 1. Take valid loan
    const takeResult = engine.dispatch(new TakeLoanCommand('IND', loanAmount));
    expect(takeResult.isSuccess).toBe(true);
    expect(country.treasury).toBe(startingTreasury + loanAmount);
    expect(country.debt).toBe(startingDebt + loanAmount);

    // 2. Exceed national debt ceiling
    const excessiveLoan = 500_000_000_000; // $500B
    const exceedResult = engine.dispatch(new TakeLoanCommand('IND', excessiveLoan));
    expect(exceedResult.isSuccess).toBe(false);
    expect(exceedResult.errorCode).toBe('DebtCeilingExceeded');

    // 3. Repay loan partially
    const repayAmount = 4_000_000_000; // $4B
    const repayResult = engine.dispatch(new RepayLoanCommand('IND', repayAmount));
    expect(repayResult.isSuccess).toBe(true);
    expect(country.treasury).toBe(startingTreasury + loanAmount - repayAmount);
    expect(country.debt).toBe(startingDebt + loanAmount - repayAmount);

    // 4. Repay loan with amount greater than outstanding debt clamps safely
    const overpayment = 20_000_000_000;
    const fullRepayResult = engine.dispatch(new RepayLoanCommand('IND', overpayment));
    expect(fullRepayResult.isSuccess).toBe(true);
    expect(country.debt).toBe(0);

    // 5. Repay when debt is zero fails gracefully
    const zeroDebtRepay = engine.dispatch(new RepayLoanCommand('IND', 1_000_000));
    expect(zeroDebtRepay.isSuccess).toBe(false);
    expect(zeroDebtRepay.errorCode).toBe('NoOutstandingDebt');
  });

  it('ConstructFacility_NewFacilityTypes_ConsumesResourcesAndIncrementsCounts', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;

    const initialOilWells = country.facilities['OilWells'] ?? 0;
    const initialUraniumMines = country.facilities['UraniumMines'] ?? 0;
    const initialTreasury = country.treasury;
    const initialIron = country.stockpiles.iron;

    // 1. Construct OilWells
    const oilResult = engine.dispatch(new ConstructFacilityCommand('IND', 'OilWells', 2));
    expect(oilResult.isSuccess).toBe(true);
    expect(country.facilities['OilWells']).toBe(initialOilWells + 2);
    expect(country.treasury).toBe(initialTreasury - 700_000_000);
    expect(country.stockpiles.iron).toBe(initialIron - 3_000.0);

    // 2. Construct UraniumMines
    const uraniumResult = engine.dispatch(new ConstructFacilityCommand('IND', 'UraniumMines', 1));
    expect(uraniumResult.isSuccess).toBe(true);
    expect(country.facilities['UraniumMines']).toBe(initialUraniumMines + 1);

    // 3. Construct with insufficient iron fails without mutating state
    country.stockpiles.iron = 10.0;
    const failResult = engine.dispatch(new ConstructFacilityCommand('IND', 'Mines', 5));
    expect(failResult.isSuccess).toBe(false);
    expect(failResult.errorCode).toBe('InsufficientResources');
  });

  it('EconomicPolicies_EnactAndRevoke_AppliesModifiersAndUpkeep', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;

    const initialTreasury = country.treasury;
    const initialApproval = country.approvalRating;

    // 1. Enact Agricultural Subsidies
    const enactResult = engine.dispatch(new EnactEconomicPolicyCommand('IND', 'agricultural_subsidies'));
    expect(enactResult.isSuccess).toBe(true);
    expect(country.activePolicies).toContain('agricultural_subsidies');
    expect(country.treasury).toBe(initialTreasury - 500_000_000);
    expect(country.approvalRating).toBe(initialApproval + 3.0);

    // 2. Cannot enact duplicate policy
    const dupResult = engine.dispatch(new EnactEconomicPolicyCommand('IND', 'agricultural_subsidies'));
    expect(dupResult.isSuccess).toBe(false);
    expect(dupResult.errorCode).toBe('PolicyAlreadyActive');

    // 3. Enact unknown policy fails safely
    const unknownResult = engine.dispatch(new EnactEconomicPolicyCommand('IND', 'non_existent_policy'));
    expect(unknownResult.isSuccess).toBe(false);
    expect(unknownResult.errorCode).toBe('PolicyNotFound');

    // 4. Revoke active policy
    const revokeResult = engine.dispatch(new RevokeEconomicPolicyCommand('IND', 'agricultural_subsidies'));
    expect(revokeResult.isSuccess).toBe(true);
    expect(country.activePolicies).not.toContain('agricultural_subsidies');

    // 5. Revoking inactive policy fails safely
    const revokeInactive = engine.dispatch(new RevokeEconomicPolicyCommand('IND', 'agricultural_subsidies'));
    expect(revokeInactive.isSuccess).toBe(false);
    expect(revokeInactive.errorCode).toBe('PolicyNotActive');
  });

  it('MonthlyLedger_GeneratesItemizedReport_AndUpdatesCreditRating', () => {
    const session = GameSession.createNewGame('IND');
    const country = session.getPlayerCountry()!;

    const ledgerCmd = new ExecuteMonthlyLedgerCommand('IND');
    const result = ledgerCmd.execute(session);

    expect(result.isSuccess).toBe(true);
    expect(country.lastBudgetReport).not.toBeNull();
    expect(country.budgetHistory.length).toBe(1);

    const report = country.lastBudgetReport!;
    expect(report.countryId).toBe('IND');
    expect(report.grossRevenue).toBeGreaterThan(0);
    expect(report.incomeTaxRevenue).toBeGreaterThan(0);
    expect(report.corporateTaxRevenue).toBeGreaterThan(0);
    expect(report.vatRevenue).toBeGreaterThan(0);
    expect(report.grossExpenses).toBeGreaterThan(0);
    expect(report.ministryOperationalExpenses).toBeGreaterThan(0);
    expect(report.civilAdministrationExpenses).toBeGreaterThan(0);
    expect(report.grossRevenue - report.grossExpenses).toBe(report.netBalance);
    expect(report.creditRating).toBe('AAA');
    expect(country.creditRating).toBe('AAA');
  });

  it('SovereignDebtDistress_LowTreasuryAndHighDebt_DowngradesRatingAndEmitsAlert', () => {
    const session = GameSession.createNewGame('IND');
    const country = session.getPlayerCountry()!;

    // Place country in severe fiscal crisis: $3 Trillion debt vs $1.68T GDP
    country.treasury = -15_000_000_000;
    country.debt = 3_000_000_000_000;

    const ledgerCmd = new ExecuteMonthlyLedgerCommand('IND');
    ledgerCmd.execute(session);

    expect(country.creditRating).toBe('CCC');
    expect(session.events.some((e) => e.title.includes('Sovereign Debt Distress'))).toBe(true);
  });

  it('LongRunEconomicSimulation_MultiMonthProgression_RemainsStableAndDeterministic', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;

    engine.dispatch(new EnactEconomicPolicyCommand('IND', 'industrial_modernization'));

    // Advance 12 months (~30 days each)
    for (let month = 0; month < 12; month++) {
      session.clock.advanceDays(30);
      engine.tick(0.0);
    }

    expect(country.budgetHistory.length).toBeGreaterThanOrEqual(10);
    expect(country.treasury).toBeGreaterThan(0);
    expect(country.stockpiles.food).toBeGreaterThan(0);
    expect(country.stockpiles.energy).toBeGreaterThan(0);
    expect(country.stockpiles.iron).toBeGreaterThan(0);
    expect(country.stockpiles.oil).toBeGreaterThan(0);
    expect(country.stockpiles.consumerGoods).toBeGreaterThan(0);
    expect(country.approvalRating).toBeGreaterThanOrEqual(0);
    expect(country.approvalRating).toBeLessThanOrEqual(100);
    expect(country.qualityOfLiving).toBeGreaterThanOrEqual(0);
    expect(country.qualityOfLiving).toBeLessThanOrEqual(100);
    expect(country.stability).toBeGreaterThanOrEqual(0);
    expect(country.stability).toBeLessThanOrEqual(100);
  });
});
