import { GameSession } from '../models/GameSession';
import { CommandResult, ICommand } from './CommandResult';
import { EventCategory, EventSeverity, TaxType } from '../models/types';
import { TimedOperation } from '../models/TimedOperation';
import { EconomicPolicyRegistry } from '../economy/EconomicPolicy';
import { Ideology } from '../models/Ideology';
import { TradeContract } from '../models/Diplomacy';
import { MonthlyBudgetReport } from '../models/MonthlyBudgetReport';
export { TaxType };

export class AdjustTaxRateCommand implements ICommand {
  countryId: string;
  type: TaxType;
  newRate: number;

  constructor(countryId: string, type: TaxType, newRate: number) {
    this.countryId = countryId;
    this.type = type;
    this.newRate = newRate;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    if (this.newRate < 0.0 || this.newRate > 1.0) {
      return CommandResult.failure('InvalidTaxRate', `Tax rate must be between 0.0 (0%) and 1.0 (100%). Received ${this.newRate}.`);
    }

    switch (this.type) {
      case TaxType.IncomeTax:
        country.incomeTaxRate = this.newRate;
        break;
      case TaxType.CorporateTax:
        country.corporateTaxRate = this.newRate;
        break;
      case TaxType.Vat:
        country.vatRate = this.newRate;
        break;
      default:
        return CommandResult.failure('UnknownTaxType', `Tax type ${this.type} is not recognized.`);
    }

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Tax Policy Adjusted',
      message: `${this.type} for ${country.name} adjusted to ${(this.newRate * 100).toFixed(1)}%.`,
      category: EventCategory.Economy,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class TakeLoanCommand implements ICommand {
  countryId: string;
  amount: number;

  constructor(countryId: string, amount: number) {
    this.countryId = countryId;
    this.amount = amount;
  }

  execute(session: GameSession): CommandResult {
    if (this.amount <= 0) {
      return CommandResult.failure('InvalidAmount', `Loan amount must be positive. Received: ${this.amount}.`);
    }

    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    const maxDebt = Math.max(100_000_000_000, country.population * 200);
    if (country.debt + this.amount > maxDebt) {
      return CommandResult.failure(
        'DebtCeilingExceeded',
        `Borrowing $${(this.amount / 1_000_000).toFixed(1)}M exceeds the national debt ceiling of $${(maxDebt / 1_000_000).toFixed(1)}M (Current debt: $${(country.debt / 1_000_000).toFixed(1)}M).`
      );
    }

    country.treasury += this.amount;
    country.debt += this.amount;

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Central Bank Loan Secured',
      message: `${country.name} borrowed $${(this.amount / 1_000_000).toFixed(1)}M (Total Debt: $${(country.debt / 1_000_000).toFixed(1)}M).`,
      category: EventCategory.Economy,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class RepayLoanCommand implements ICommand {
  countryId: string;
  amount: number;

  constructor(countryId: string, amount: number) {
    this.countryId = countryId;
    this.amount = amount;
  }

  execute(session: GameSession): CommandResult {
    if (this.amount <= 0) {
      return CommandResult.failure('InvalidAmount', `Repayment amount must be positive. Received: ${this.amount}.`);
    }

    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    if (country.debt <= 0) {
      return CommandResult.failure('NoOutstandingDebt', `${country.name} has no outstanding debt to repay.`);
    }

    const actualRepayment = Math.min(this.amount, country.debt);
    if (country.treasury < actualRepayment) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Insufficient treasury funds ($${(country.treasury / 1_000_000).toFixed(1)}M) to repay $${(actualRepayment / 1_000_000).toFixed(1)}M of debt.`
      );
    }

    country.treasury -= actualRepayment;
    country.debt -= actualRepayment;

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Debt Service Repayment',
      message: `${country.name} repaid $${(actualRepayment / 1_000_000).toFixed(1)}M of national debt (Remaining: $${(country.debt / 1_000_000).toFixed(1)}M).`,
      category: EventCategory.Economy,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export const FACILITY_COST_TABLE: Record<string, { moneyCost: number; ironCost: number }> = {
  Farms: { moneyCost: 100_000_000, ironCost: 500.0 },
  Mines: { moneyCost: 250_000_000, ironCost: 1_000.0 },
  PowerPlants: { moneyCost: 500_000_000, ironCost: 2_000.0 },
  Factories: { moneyCost: 300_000_000, ironCost: 1_500.0 },
  Hospitals: { moneyCost: 200_000_000, ironCost: 500.0 },
  Schools: { moneyCost: 150_000_000, ironCost: 500.0 },
  Barracks: { moneyCost: 250_000_000, ironCost: 1_000.0 },
  OilWells: { moneyCost: 350_000_000, ironCost: 1_500.0 },
  UraniumMines: { moneyCost: 600_000_000, ironCost: 3_000.0 }
};

export class ConstructFacilityCommand implements ICommand {
  countryId: string;
  facilityType: string;
  quantity: number;

  constructor(countryId: string, facilityType: string, quantity: number = 1) {
    this.countryId = countryId;
    this.facilityType = facilityType;
    this.quantity = quantity;
  }

  execute(session: GameSession): CommandResult {
    if (this.quantity <= 0) {
      return CommandResult.failure('InvalidQuantity', `Facility quantity must be at least 1. Received: ${this.quantity}.`);
    }

    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    const unitCost = FACILITY_COST_TABLE[this.facilityType];
    if (!unitCost) {
      return CommandResult.failure(
        'UnknownFacilityType',
        `Facility type '${this.facilityType}' is not recognized. Supported types: ${Object.keys(FACILITY_COST_TABLE).join(', ')}.`
      );
    }

    const totalMoneyCost = unitCost.moneyCost * this.quantity;
    const totalIronCost = unitCost.ironCost * this.quantity;

    if (country.treasury < totalMoneyCost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Insufficient funds to construct ${this.quantity}x ${this.facilityType}. Required: $${(totalMoneyCost / 1_000_000).toFixed(1)}M, Available: $${(country.treasury / 1_000_000).toFixed(1)}M.`
      );
    }

    if (country.stockpiles.iron < totalIronCost) {
      return CommandResult.failure(
        'InsufficientResources',
        `Insufficient iron to construct ${this.quantity}x ${this.facilityType}. Required: ${totalIronCost.toFixed(0)} Iron, Available: ${country.stockpiles.iron.toFixed(0)} Iron.`
      );
    }

    country.treasury -= totalMoneyCost;
    country.stockpiles = {
      ...country.stockpiles,
      iron: country.stockpiles.iron - totalIronCost
    };

    const currentCount = country.facilities[this.facilityType] ?? 0;
    country.facilities[this.facilityType] = currentCount + this.quantity;

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Facility Construction Completed',
      message: `${country.name} constructed ${this.quantity}x ${this.facilityType} for $${(totalMoneyCost / 1_000_000).toFixed(1)}M and ${totalIronCost.toFixed(0)} Iron. Total now: ${country.facilities[this.facilityType]}.`,
      category: EventCategory.Economy,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class StartTimedConstructionCommand implements ICommand {
  countryId: string;
  facilityType: string;
  quantity: number;
  buildDays: number;

  static readonly DefaultBuildDays = 60;

  constructor(countryId: string, facilityType: string, quantity: number = 1, buildDays: number = StartTimedConstructionCommand.DefaultBuildDays) {
    this.countryId = countryId;
    this.facilityType = facilityType;
    this.quantity = quantity;
    this.buildDays = buildDays;
  }

  execute(session: GameSession): CommandResult {
    if (this.quantity <= 0) {
      return CommandResult.failure('InvalidQuantity', `Facility quantity must be at least 1. Received: ${this.quantity}.`);
    }

    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    const unitCost = FACILITY_COST_TABLE[this.facilityType];
    if (!unitCost) {
      return CommandResult.failure('UnknownFacilityType', `Facility type '${this.facilityType}' is not recognized.`);
    }

    const totalMoneyCost = unitCost.moneyCost * this.quantity;
    const totalIronCost = unitCost.ironCost * this.quantity;

    if (country.treasury < totalMoneyCost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Insufficient funds to construct ${this.quantity}x ${this.facilityType}. Required: $${(totalMoneyCost / 1_000_000).toFixed(1)}M, Available: $${(country.treasury / 1_000_000).toFixed(1)}M.`
      );
    }

    if (country.stockpiles.iron < totalIronCost) {
      return CommandResult.failure(
        'InsufficientResources',
        `Insufficient iron to construct ${this.quantity}x ${this.facilityType}. Required: ${totalIronCost.toFixed(0)} Iron, Available: ${country.stockpiles.iron.toFixed(0)} Iron.`
      );
    }

    country.treasury -= totalMoneyCost;
    country.stockpiles = {
      ...country.stockpiles,
      iron: country.stockpiles.iron - totalIronCost
    };

    const op = new TimedOperation(
      this.countryId,
      `Construct ${this.quantity}x ${this.facilityType}`,
      'Construction',
      session.clock.currentDate,
      session.clock.currentDate.addDays(this.buildDays),
      this.buildDays,
      {
        FacilityType: this.facilityType,
        Quantity: this.quantity.toString()
      }
    );
    session.timedOperations.push(op);

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Construction Commenced',
      message: `${country.name} broke ground on ${this.quantity}x ${this.facilityType} ($${(totalMoneyCost / 1_000_000).toFixed(1)}M, ${totalIronCost.toFixed(0)} Iron). Ready in ${this.buildDays} days.`,
      category: EventCategory.Economy,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class EnactEconomicPolicyCommand implements ICommand {
  countryId: string;
  policyId: string;

  constructor(countryId: string, policyId: string) {
    this.countryId = countryId;
    this.policyId = policyId;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    const registry = session.policyRegistry ?? EconomicPolicyRegistry.createDefaultCatalog();
    const policy = registry.tryGet(this.policyId);
    if (!policy) {
      return CommandResult.failure('PolicyNotFound', `Economic policy '${this.policyId}' not found in registry.`);
    }

    if (country.activePolicies.some((p) => p.toLowerCase() === this.policyId.toLowerCase())) {
      return CommandResult.failure('PolicyAlreadyActive', `Policy '${policy.name}' is already active for ${country.name}.`);
    }

    if (country.treasury < policy.upfrontCost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Insufficient funds to enact '${policy.name}'. Required: $${(policy.upfrontCost / 1_000_000).toFixed(1)}M, Available: $${(country.treasury / 1_000_000).toFixed(1)}M.`
      );
    }

    country.treasury -= policy.upfrontCost;
    country.activePolicies.push(policy.id);

    if (policy.approvalModifier) {
      country.approvalRating = Math.max(0.0, Math.min(100.0, country.approvalRating + policy.approvalModifier));
    }
    if (policy.livingStandardModifier) {
      country.qualityOfLiving = Math.max(0.0, Math.min(100.0, country.qualityOfLiving + policy.livingStandardModifier));
    }
    if (policy.stabilityModifier) {
      country.stability = Math.max(0.0, Math.min(100.0, country.stability + policy.stabilityModifier));
    }

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Economic Policy Enacted',
      message: `${country.name} ratified policy '${policy.name}': ${policy.description}`,
      category: EventCategory.Economy,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class RevokeEconomicPolicyCommand implements ICommand {
  countryId: string;
  policyId: string;

  constructor(countryId: string, policyId: string) {
    this.countryId = countryId;
    this.policyId = policyId;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    const idx = country.activePolicies.findIndex((p) => p.toLowerCase() === this.policyId.toLowerCase());
    if (idx < 0) {
      return CommandResult.failure('PolicyNotActive', `Policy '${this.policyId}' is not active for ${country.name}.`);
    }

    country.activePolicies.splice(idx, 1);

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Economic Policy Revoked',
      message: `${country.name} discontinued economic policy '${this.policyId}'. Monthly upkeep and bonuses terminated.`,
      category: EventCategory.Economy,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class ExecuteMonthlyLedgerCommand implements ICommand {
  countryId: string;

  constructor(countryId: string) {
    this.countryId = countryId;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    // 0. Resolve active economic policy effects
    let gdpBonus = 0.0;
    let farmBonus = 0.0;
    let factoryBonus = 0.0;
    let energyBonus = 0.0;
    let taxBonus = 0.0;
    let policyUpkeep = 0;

    const policyRegistry = session.policyRegistry ?? EconomicPolicyRegistry.createDefaultCatalog();
    for (const policyId of country.activePolicies) {
      const policy = policyRegistry.tryGet(policyId);
      if (policy) {
        gdpBonus += policy.gdpMultiplier ?? 0.0;
        farmBonus += policy.farmEfficiencyMultiplier ?? 0.0;
        factoryBonus += policy.factoryEfficiencyMultiplier ?? 0.0;
        energyBonus += policy.energyEfficiencyMultiplier ?? 0.0;
        taxBonus += policy.taxEfficiencyModifier ?? 0.0;
        policyUpkeep += policy.monthlyUpkeep;
      }
    }

    // 1. Calculate Monthly Revenue:
    const baseGdp = country.calculateMonthlyGdp();
    const monthlyGdp = baseGdp * (1.0 + gdpBonus);
    const annualGdp = monthlyGdp * 12;

    const incomeTaxRevenue = monthlyGdp * country.incomeTaxRate * 0.40;
    const corporateTaxRevenue = monthlyGdp * country.corporateTaxRate * 0.35;
    const vatRevenue = monthlyGdp * country.vatRate * 0.25;

    const factories = country.facilities['Factories'] ?? 10;
    const commercialRevenue = factories * 2_500_000 * (1.0 + factoryBonus);

    const rawRevenue = incomeTaxRevenue + corporateTaxRevenue + vatRevenue + commercialRevenue;
    const ideologyDef = Ideology.get(country.ideology);
    const totalTaxEfficiency = Math.max(0.10, ideologyDef.taxEfficiencyMultiplier + taxBonus - (country.corruptionIndex / 250.0));

    // Trade contracts processing
    let tradeExportRevenue = 0;
    let tradeImportExpenses = 0;
    const tradeResourceNetDelta: Record<string, number> = {};

    for (const [, stance] of Object.entries(country.diplomaticRelations)) {
      if (stance.nonAggressionPactUntil && stance.nonAggressionPactUntil.isSameOrBefore(session.clock.currentDate)) {
        stance.nonAggressionPactUntil = null;
      }

      const activeContracts: TradeContract[] = [];
      for (const contract of stance.activeTradeContracts) {
        if (contract.expiryDate && contract.expiryDate.isSameOrBefore(session.clock.currentDate)) {
          continue;
        }
        activeContracts.push(contract);

        if (contract.exporterCountryId.toLowerCase() === this.countryId.toLowerCase()) {
          tradeExportRevenue += contract.monthlyPayment;
          tradeResourceNetDelta[contract.resourceType] = (tradeResourceNetDelta[contract.resourceType] ?? 0) - contract.monthlyQuantity;
        } else if (contract.importerCountryId.toLowerCase() === this.countryId.toLowerCase()) {
          tradeImportExpenses += contract.monthlyPayment;
          tradeResourceNetDelta[contract.resourceType] = (tradeResourceNetDelta[contract.resourceType] ?? 0) + contract.monthlyQuantity;
        }
      }
      stance.activeTradeContracts = activeContracts;
    }

    const grossRevenue = (rawRevenue * totalTaxEfficiency) + tradeExportRevenue;

    if (country.ideologyCooldownMonths > 0) {
      country.ideologyCooldownMonths--;
    }

    // 2. Calculate Monthly Expenses:
    let ministryExpenses = 0;
    let ministerSalaries = 0;
    for (const ministryState of Object.values(country.ministries)) {
      ministryExpenses += ministryState.funding * 1_000_000;
      ministerSalaries += ministryState.activeMinister?.monthlySalary ?? 0;
    }

    const civilAdminExpenses = 500_000_000;
    const barracks = country.facilities['Barracks'] ?? 10;
    const defenseExpenses = 200_000_000 + (barracks * 5_000_000);

    const militaryUnitUpkeep = country.calculateMonthlyMilitaryUpkeep(session.militaryRegistry);
    let presidentialProtectionCost = 50_000_000;
    switch (country.presidentialProtectionLevel) {
      case 25: // Low
        presidentialProtectionCost = 20_000_000;
        break;
      case 50: // Medium
        presidentialProtectionCost = 50_000_000;
        break;
      case 75: // High
        presidentialProtectionCost = 150_000_000;
        break;
      case 100: // Maximum
        presidentialProtectionCost = 300_000_000;
        break;
    }

    const totalFacilities = Object.values(country.facilities).reduce((a, b) => a + b, 0);
    const facilityUpkeepExpenses = totalFacilities * 500_000;

    const monthlyInterestRate = country.getMonthlyInterestRate();
    const debtInterestExpenses = country.debt * monthlyInterestRate;

    const grossExpenses =
      ministryExpenses +
      ministerSalaries +
      civilAdminExpenses +
      defenseExpenses +
      facilityUpkeepExpenses +
      debtInterestExpenses +
      policyUpkeep +
      tradeImportExpenses +
      militaryUnitUpkeep +
      presidentialProtectionCost;

    // 3. Invariant Net Calculation:
    const startingTreasury = country.treasury;
    const netDelta = grossRevenue - grossExpenses;
    country.treasury += netDelta;

    country.creditRating = country.calculateCreditRating();

    // 4. Resource updates:
    const farms = country.facilities['Farms'] ?? 10;
    const powerPlants = country.facilities['PowerPlants'] ?? 10;
    const mines = country.facilities['Mines'] ?? 10;
    const oilWells = country.facilities['OilWells'] ?? 10;
    const uraniumMines = country.facilities['UraniumMines'] ?? 5;

    const foodProduced = farms * 20_000.0 * (1.0 + farmBonus);
    const foodConsumed = (country.population / 1_000_000.0) * 700.0;

    const energyProduced = powerPlants * 15_000.0 * (1.0 + energyBonus);
    const energyConsumed = (country.population / 1_000_000.0) * 300.0;

    const ironProduced = mines * 5_000.0;
    const ironConsumedByFactories = factories * 200.0;

    const oilProduced = oilWells * 10_000.0;
    const oilConsumed = (country.population / 1_000_000.0) * 120.0;

    const uraniumProduced = uraniumMines * 1_000.0;
    const uraniumConsumed = powerPlants * 50.0;

    const goodsProduced = factories * 5_000.0 * (1.0 + factoryBonus);
    const goodsConsumed = (country.population / 1_000_000.0) * 120.0;

    const current = country.stockpiles;
    const nextFood = Math.max(0.0, current.food + foodProduced - foodConsumed + (tradeResourceNetDelta['Food'] ?? 0.0));
    const nextEnergy = Math.max(0.0, current.energy + energyProduced - energyConsumed + (tradeResourceNetDelta['Energy'] ?? 0.0));
    const nextIron = Math.max(0.0, current.iron + ironProduced - ironConsumedByFactories + (tradeResourceNetDelta['Iron'] ?? 0.0));
    const nextOil = Math.max(0.0, current.oil + oilProduced - oilConsumed + (tradeResourceNetDelta['Oil'] ?? 0.0));
    const nextUranium = Math.max(0.0, current.uranium + uraniumProduced - uraniumConsumed + (tradeResourceNetDelta['Uranium'] ?? 0.0));
    const nextGoods = Math.max(0.0, current.consumerGoods + goodsProduced - goodsConsumed + (tradeResourceNetDelta['ConsumerGoods'] ?? 0.0));

    country.stockpiles = {
      food: nextFood,
      energy: nextEnergy,
      oil: nextOil,
      iron: nextIron,
      uranium: nextUranium,
      consumerGoods: nextGoods
    };

    if (nextFood <= 0.0 && foodConsumed > foodProduced) {
      country.approvalRating = Math.max(0.0, country.approvalRating - 3.0);
      country.stability = Math.max(0.0, country.stability - 2.0);
    }
    if (nextEnergy <= 0.0 && energyConsumed > energyProduced) {
      country.qualityOfLiving = Math.max(0.0, country.qualityOfLiving - 2.0);
      country.approvalRating = Math.max(0.0, country.approvalRating - 2.0);
    }

    // 5. Build and archive Monthly Budget Report
    const report: MonthlyBudgetReport = {
      date: session.clock.currentDate,
      countryId: this.countryId,
      incomeTaxRevenue,
      corporateTaxRevenue,
      vatRevenue,
      commercialFacilityRevenue: commercialRevenue,
      tradeExportRevenue,
      grossRevenue,
      taxEfficiency: totalTaxEfficiency,

      ministryOperationalExpenses: ministryExpenses,
      ministerSalaries,
      civilAdministrationExpenses: civilAdminExpenses,
      defenseUpkeepExpenses: defenseExpenses,
      militaryUnitUpkeepExpenses: militaryUnitUpkeep,
      facilityUpkeepExpenses,
      debtInterestExpenses,
      activePolicyUpkeepExpenses: policyUpkeep,
      tradeImportExpenses,
      grossExpenses,

      netBalance: netDelta,
      startingTreasury,
      endingTreasury: country.treasury,
      startingDebt: country.debt,
      endingDebt: country.debt,
      monthlyGdp,
      annualGdp,
      debtToGdpRatio: country.debt / (annualGdp > 0 ? annualGdp : 1),
      creditRating: country.creditRating,
      inflationRate: country.inflationRate
    };

    country.lastBudgetReport = report;
    country.budgetHistory.push(report);
    if (country.budgetHistory.length > 24) {
      country.budgetHistory.shift();
    }

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Monthly Ledger Settled',
      message: `${country.name} financial ledger: Revenue $${(grossRevenue / 1_000_000).toFixed(1)}M, Expenses $${(grossExpenses / 1_000_000).toFixed(1)}M (Net: ${netDelta >= 0 ? '+' : ''}$${(netDelta / 1_000_000).toFixed(1)}M). Treasury: $${(country.treasury / 1_000_000).toFixed(1)}M.`,
      category: EventCategory.Economy,
      severity: country.treasury < 0 ? EventSeverity.Warning : EventSeverity.Info,
      actorCountryId: this.countryId
    });

    if (country.treasury < -10_000_000_000 || country.creditRating === 'CCC' || country.creditRating === 'D') {
      session.addEvent({
        id: Math.random().toString(36).substring(2, 11),
        date: session.clock.currentDate,
        title: 'Sovereign Debt Distress Alert',
        message: `${country.name} has entered acute sovereign debt distress! Credit rating downgraded to ${country.creditRating}. Urgent budget consolidation required.`,
        category: EventCategory.Economy,
        severity: EventSeverity.Critical,
        actorCountryId: this.countryId
      });
    }

    return CommandResult.success();
  }
}
