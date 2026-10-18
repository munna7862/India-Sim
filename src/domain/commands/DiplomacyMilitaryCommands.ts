import { GameSession } from '../models/GameSession';
import { CommandResult, ICommand } from './CommandResult';
import { EventCategory, EventSeverity, CovertOperationType } from '../models/types';
export { CovertOperationType };
import { DiplomaticGiftTier, TradeContract } from '../models/Diplomacy';
import { TimedOperation } from '../models/TimedOperation';
import { MilitaryUnitRegistry } from '../military/MilitaryUnitDefinition';
import { DateOnly } from '../models/DateOnly';
import { CountryState } from '../models/CountryState';

export class SendDiplomaticGiftCommand implements ICommand {
  static readonly CooldownDays = 30;

  senderCountryId: string;
  targetCountryId: string;
  tier: DiplomaticGiftTier;

  constructor(senderCountryId: string, targetCountryId: string, tier: DiplomaticGiftTier) {
    this.senderCountryId = senderCountryId;
    this.targetCountryId = targetCountryId;
    this.tier = tier;
  }

  static getTierValues(tier: DiplomaticGiftTier): { cost: number; relationsBonus: number } {
    switch (tier) {
      case DiplomaticGiftTier.Small:
        return { cost: 250_000_000, relationsBonus: 5.0 };
      case DiplomaticGiftTier.Medium:
        return { cost: 1_000_000_000, relationsBonus: 12.0 };
      case DiplomaticGiftTier.Large:
        return { cost: 5_000_000_000, relationsBonus: 25.0 };
      default:
        return { cost: 250_000_000, relationsBonus: 5.0 };
    }
  }

  execute(session: GameSession): CommandResult {
    if (this.senderCountryId.toLowerCase() === this.targetCountryId.toLowerCase()) {
      return CommandResult.failure('InvalidTarget', 'A country cannot send a diplomatic gift to itself.');
    }

    const sender = session.countries[this.senderCountryId];
    if (!sender) {
      return CommandResult.failure('CountryNotFound', `Sender country '${this.senderCountryId}' not found.`);
    }

    const target = session.countries[this.targetCountryId];
    if (!target) {
      return CommandResult.failure('CountryNotFound', `Target country '${this.targetCountryId}' not found.`);
    }

    const senderStance = sender.getRelationsWith(this.targetCountryId);
    if (senderStance.isAtWar) {
      return CommandResult.failure('AtWar', `Cannot send diplomatic aid to '${target.name}' while actively at war.`);
    }

    const cooldownKey = `DiplomaticGift_${this.targetCountryId}`;
    if (session.cooldowns.isOnCooldown(this.senderCountryId, cooldownKey, session.clock.currentDate)) {
      const expiry = session.cooldowns.getExpiry(this.senderCountryId, cooldownKey);
      return CommandResult.failure(
        'ActionOnCooldown',
        `Diplomatic aid to ${target.name} is on cooldown until ${expiry?.toString()}.`
      );
    }

    const { cost, relationsBonus } = SendDiplomaticGiftCommand.getTierValues(this.tier);
    if (sender.treasury < cost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Insufficient treasury ($${(sender.treasury / 1_000_000).toFixed(1)}M) to fund ${this.tier} diplomatic aid package of $${(cost / 1_000_000).toFixed(0)}M.`
      );
    }

    sender.treasury -= cost;
    target.treasury += cost;

    senderStance.modifyRelations(relationsBonus);
    const targetStance = target.getRelationsWith(this.senderCountryId);
    targetStance.modifyRelations(relationsBonus);

    session.cooldowns.setCooldown(
      this.senderCountryId,
      cooldownKey,
      session.clock.currentDate.addDays(SendDiplomaticGiftCommand.CooldownDays)
    );

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Diplomatic Foreign Aid Dispatched',
      message: `${sender.name} delivered a ${this.tier} foreign development package ($${(cost / 1_000_000).toFixed(0)}M) to ${target.name}. Relations improved by +${relationsBonus.toFixed(0)}.`,
      category: EventCategory.Diplomacy,
      severity: EventSeverity.Info,
      actorCountryId: this.senderCountryId,
      targetCountryId: this.targetCountryId
    });

    return CommandResult.success();
  }
}

export class SignNonAggressionPactCommand implements ICommand {
  static readonly TreatyProcessingFee = 50_000_000;
  static readonly MinimumRelationsRequired = -20.0;
  static readonly RelationsBonus = 10.0;

  initiatorCountryId: string;
  targetCountryId: string;
  durationMonths: number;

  constructor(initiatorCountryId: string, targetCountryId: string, durationMonths: number = 24) {
    this.initiatorCountryId = initiatorCountryId;
    this.targetCountryId = targetCountryId;
    this.durationMonths = durationMonths > 0 ? durationMonths : 24;
  }

  execute(session: GameSession): CommandResult {
    if (this.initiatorCountryId.toLowerCase() === this.targetCountryId.toLowerCase()) {
      return CommandResult.failure('InvalidTarget', 'A country cannot sign a Non-Aggression Pact with itself.');
    }

    const initiator = session.countries[this.initiatorCountryId];
    if (!initiator) {
      return CommandResult.failure('CountryNotFound', `Initiator country '${this.initiatorCountryId}' not found.`);
    }

    const target = session.countries[this.targetCountryId];
    if (!target) {
      return CommandResult.failure('CountryNotFound', `Target country '${this.targetCountryId}' not found.`);
    }

    const initiatorStance = initiator.getRelationsWith(this.targetCountryId);
    const targetStance = target.getRelationsWith(this.initiatorCountryId);

    if (initiatorStance.isAtWar) {
      return CommandResult.failure('AtWar', `Cannot sign a Non-Aggression Pact with '${target.name}' while actively at war.`);
    }

    if (initiatorStance.relations < SignNonAggressionPactCommand.MinimumRelationsRequired) {
      return CommandResult.failure(
        'RelationsTooLow',
        `Relations with ${target.name} (${initiatorStance.relations.toFixed(0)}) are too hostile to sign a Non-Aggression Pact. Minimum required: ${SignNonAggressionPactCommand.MinimumRelationsRequired}.`
      );
    }

    if (initiatorStance.hasActiveNonAggressionPact(session.clock.currentDate)) {
      return CommandResult.failure(
        'ActivePactExists',
        `A Non-Aggression Pact with ${target.name} is already active until ${initiatorStance.nonAggressionPactUntil?.toString()}.`
      );
    }

    if (initiator.treasury < SignNonAggressionPactCommand.TreatyProcessingFee) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Insufficient funds to process treaty protocols ($${(SignNonAggressionPactCommand.TreatyProcessingFee / 1_000_000).toFixed(0)}M required).`
      );
    }

    initiator.treasury -= SignNonAggressionPactCommand.TreatyProcessingFee;

    const expiryDate = session.clock.currentDate.addDays(this.durationMonths * 30);
    initiatorStance.nonAggressionPactUntil = expiryDate;
    targetStance.nonAggressionPactUntil = expiryDate;

    initiatorStance.modifyRelations(SignNonAggressionPactCommand.RelationsBonus);
    targetStance.modifyRelations(SignNonAggressionPactCommand.RelationsBonus);

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Non-Aggression Pact Ratified',
      message: `${initiator.name} and ${target.name} concluded a bilateral Non-Aggression Pact valid until ${expiryDate.toString()}.`,
      category: EventCategory.Diplomacy,
      severity: EventSeverity.Info,
      actorCountryId: this.initiatorCountryId,
      targetCountryId: this.targetCountryId
    });

    return CommandResult.success();
  }
}

export class SignTradeContractCommand implements ICommand {
  static readonly ValidResourceTypes = new Set(['Food', 'Energy', 'Oil', 'Iron', 'Uranium', 'ConsumerGoods']);

  exporterCountryId: string;
  importerCountryId: string;
  resourceType: string;
  monthlyQuantity: number;
  monthlyPayment: number;
  durationMonths: number;

  constructor(
    exporterCountryId: string,
    importerCountryId: string,
    resourceType: string,
    monthlyQuantity: number,
    monthlyPayment: number,
    durationMonths: number = 24
  ) {
    this.exporterCountryId = exporterCountryId;
    this.importerCountryId = importerCountryId;
    this.resourceType = resourceType;
    this.monthlyQuantity = monthlyQuantity;
    this.monthlyPayment = monthlyPayment;
    this.durationMonths = durationMonths > 0 ? durationMonths : 24;
  }

  execute(session: GameSession): CommandResult {
    if (this.exporterCountryId.toLowerCase() === this.importerCountryId.toLowerCase()) {
      return CommandResult.failure('InvalidContractParties', 'A nation cannot establish an export-import trade contract with itself.');
    }

    const exporter = session.countries[this.exporterCountryId];
    if (!exporter) {
      return CommandResult.failure('CountryNotFound', `Exporter nation '${this.exporterCountryId}' not found.`);
    }

    const importer = session.countries[this.importerCountryId];
    if (!importer) {
      return CommandResult.failure('CountryNotFound', `Importer nation '${this.importerCountryId}' not found.`);
    }

    if (!SignTradeContractCommand.ValidResourceTypes.has(this.resourceType)) {
      return CommandResult.failure('UnknownResourceType', `Resource '${this.resourceType}' is not a valid trade commodity.`);
    }

    if (this.monthlyQuantity <= 0.0 || this.monthlyPayment <= 0) {
      return CommandResult.failure('InvalidQuantities', 'Monthly trade quantity and payment must both be strictly positive.');
    }

    const exporterStance = exporter.getRelationsWith(this.importerCountryId);
    const importerStance = importer.getRelationsWith(this.exporterCountryId);

    if (exporterStance.isAtWar) {
      return CommandResult.failure('AtWar', `Cannot sign commercial trade agreements with '${importer.name}' during active hostilities.`);
    }

    if (exporterStance.relations < -30.0) {
      return CommandResult.failure(
        'RelationsTooLow',
        `Relations with ${importer.name} (${exporterStance.relations.toFixed(0)}) are too poor to agree on commercial trade terms. Minimum required: -30.`
      );
    }

    const contractExists = exporterStance.activeTradeContracts.some(
      (c) =>
        c.importerCountryId.toLowerCase() === this.importerCountryId.toLowerCase() &&
        c.resourceType.toLowerCase() === this.resourceType.toLowerCase()
    );

    if (contractExists) {
      return CommandResult.failure(
        'DuplicateContract',
        `An active trade contract for ${this.resourceType} already exists between ${exporter.name} and ${importer.name}.`
      );
    }

    const currentStock = this.getStockpileAmount(exporter.stockpiles, this.resourceType);
    if (currentStock < this.monthlyQuantity) {
      return CommandResult.failure(
        'InsufficientStockpile',
        `Exporter ${exporter.name} lacks adequate ${this.resourceType} stockpile (${currentStock.toLocaleString()}) to initiate monthly export of ${this.monthlyQuantity.toLocaleString()}.`
      );
    }

    const contract: TradeContract = {
      id: `TC_${this.exporterCountryId}_${this.importerCountryId}_${this.resourceType}_${Math.random().toString(36).substring(2, 9)}`,
      exporterCountryId: this.exporterCountryId,
      importerCountryId: this.importerCountryId,
      resourceType: this.resourceType,
      monthlyQuantity: this.monthlyQuantity,
      monthlyPayment: this.monthlyPayment,
      startDate: session.clock.currentDate,
      expiryDate: session.clock.currentDate.addDays(this.durationMonths * 30)
    };

    exporterStance.activeTradeContracts.push(contract);
    importerStance.activeTradeContracts.push(contract);

    exporterStance.modifyRelations(5.0);
    importerStance.modifyRelations(5.0);

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Commercial Trade Agreement Ratified',
      message: `${exporter.name} will export ${this.monthlyQuantity.toLocaleString()} ${this.resourceType}/mo to ${importer.name} in exchange for $${(this.monthlyPayment / 1_000_000).toFixed(1)}M/mo until ${contract.expiryDate?.toString()}.`,
      category: EventCategory.Diplomacy,
      severity: EventSeverity.Info,
      actorCountryId: this.exporterCountryId
    });

    return CommandResult.success();
  }

  private getStockpileAmount(stockpile: any, resourceType: string): number {
    switch (resourceType.toLowerCase()) {
      case 'food':
        return stockpile.food;
      case 'energy':
        return stockpile.energy;
      case 'oil':
        return stockpile.oil;
      case 'iron':
        return stockpile.iron;
      case 'uranium':
        return stockpile.uranium;
      case 'consumergoods':
        return stockpile.consumerGoods;
      default:
        return 0;
    }
  }
}

export class CancelTradeContractCommand implements ICommand {
  countryId: string;
  contractId: string;

  constructor(countryId: string, contractId: string) {
    this.countryId = countryId;
    this.contractId = contractId;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country '${this.countryId}' not found.`);
    }

    let partnerCountryId: string | null = null;
    for (const [partnerId, stance] of Object.entries(country.diplomaticRelations)) {
      const match = stance.activeTradeContracts.find((c) => c.id === this.contractId);
      if (match) {
        partnerCountryId = partnerId;
        break;
      }
    }

    if (!partnerCountryId) {
      return CommandResult.failure('ContractNotFound', `Active trade contract '${this.contractId}' not found.`);
    }

    const ownStance = country.getRelationsWith(partnerCountryId);
    ownStance.activeTradeContracts = ownStance.activeTradeContracts.filter((c) => c.id !== this.contractId);

    const partner = session.countries[partnerCountryId];
    if (partner) {
      const partnerStance = partner.getRelationsWith(this.countryId);
      partnerStance.activeTradeContracts = partnerStance.activeTradeContracts.filter((c) => c.id !== this.contractId);

      ownStance.modifyRelations(-5.0);
      partnerStance.modifyRelations(-5.0);
    }

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Trade Contract Terminated',
      message: `${country.name} unilaterally terminated trade contract '${this.contractId}'.`,
      category: EventCategory.Diplomacy,
      severity: EventSeverity.Warning,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class RecruitMilitaryUnitsCommand implements ICommand {
  countryId: string;
  unitTypeId: string;
  quantity: number;

  constructor(countryId: string, unitTypeId: string, quantity: number) {
    this.countryId = countryId;
    this.unitTypeId = unitTypeId;
    this.quantity = quantity;
  }

  execute(session: GameSession): CommandResult {
    if (this.quantity <= 0) {
      return CommandResult.failure('InvalidQuantity', 'Recruitment quantity must be greater than zero.');
    }

    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country '${this.countryId}' not found.`);
    }

    const registry = session.militaryRegistry ?? MilitaryUnitRegistry.createDefaultCatalog();
    const unit = registry.tryGet(this.unitTypeId);
    if (!unit) {
      return CommandResult.failure('UnitNotFound', `Military unit specification '${this.unitTypeId}' not recognized.`);
    }

    const barracks = country.facilities['Barracks'] ?? 0;
    if (barracks < unit.requiredBarracks) {
      return CommandResult.failure(
        'InsufficientBarracks',
        `Commissioning ${unit.name} requires at least ${unit.requiredBarracks} operational Barracks. Current: ${barracks}.`
      );
    }

    const totalTreasuryCost = unit.costTreasury * this.quantity;
    const totalIronCost = unit.costIron * this.quantity;
    const totalOilCost = unit.costOil * this.quantity;
    const totalUraniumCost = unit.costUranium * this.quantity;

    if (country.treasury < totalTreasuryCost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Insufficient treasury to recruit ${this.quantity}x ${unit.name}. Required: $${(totalTreasuryCost / 1_000_000).toFixed(0)}M, Available: $${(country.treasury / 1_000_000).toFixed(1)}M.`
      );
    }

    if (country.stockpiles.iron < totalIronCost) {
      return CommandResult.failure(
        'InsufficientIron',
        `Insufficient iron. Required: ${totalIronCost.toFixed(0)}, Available: ${country.stockpiles.iron.toFixed(0)}.`
      );
    }
    if (country.stockpiles.oil < totalOilCost) {
      return CommandResult.failure(
        'InsufficientOil',
        `Insufficient oil. Required: ${totalOilCost.toFixed(0)}, Available: ${country.stockpiles.oil.toFixed(0)}.`
      );
    }
    if (country.stockpiles.uranium < totalUraniumCost) {
      return CommandResult.failure(
        'InsufficientUranium',
        `Insufficient uranium. Required: ${totalUraniumCost.toFixed(0)}, Available: ${country.stockpiles.uranium.toFixed(0)}.`
      );
    }

    country.treasury -= totalTreasuryCost;
    country.stockpiles = {
      ...country.stockpiles,
      iron: country.stockpiles.iron - totalIronCost,
      oil: country.stockpiles.oil - totalOilCost,
      uranium: country.stockpiles.uranium - totalUraniumCost
    };

    const currentCount = country.militaryUnits[this.unitTypeId] ?? 0;
    country.militaryUnits[this.unitTypeId] = currentCount + this.quantity;

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Military Units Commissioned',
      message: `${country.name} commissioned ${this.quantity}x ${unit.name} (Total: ${country.militaryUnits[this.unitTypeId]}).`,
      category: EventCategory.Military,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class DisbandMilitaryUnitsCommand implements ICommand {
  countryId: string;
  unitTypeId: string;
  quantity: number;

  constructor(countryId: string, unitTypeId: string, quantity: number) {
    this.countryId = countryId;
    this.unitTypeId = unitTypeId;
    this.quantity = quantity;
  }

  execute(session: GameSession): CommandResult {
    if (this.quantity <= 0) {
      return CommandResult.failure('InvalidQuantity', 'Quantity to disband must be greater than zero.');
    }

    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country '${this.countryId}' not found.`);
    }

    const currentCount = country.militaryUnits[this.unitTypeId] ?? 0;
    if (currentCount <= 0) {
      return CommandResult.failure('NoUnitsToDisband', `No active units of type '${this.unitTypeId}' exist in military inventory.`);
    }

    const actualDisband = Math.min(this.quantity, currentCount);
    country.militaryUnits[this.unitTypeId] = currentCount - actualDisband;

    if (country.militaryUnits[this.unitTypeId] <= 0) {
      delete country.militaryUnits[this.unitTypeId];
    }

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Military Units Disbanded',
      message: `${country.name} decommissioned ${actualDisband}x ${this.unitTypeId}. Monthly military upkeep reduced.`,
      category: EventCategory.Military,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class ConductArmyDrillsCommand implements ICommand {
  static readonly DrillCost = 100_000_000;
  static readonly CooldownDays = 180;
  static readonly DurationDays = 30;

  countryId: string;

  constructor(countryId: string) {
    this.countryId = countryId;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    if (session.cooldowns.isOnCooldown(this.countryId, 'ArmyDrills', session.clock.currentDate)) {
      const expiry = session.cooldowns.getExpiry(this.countryId, 'ArmyDrills');
      return CommandResult.failure('ActionOnCooldown', `Army drills are on cooldown until ${expiry?.toString()}.`);
    }

    if (country.treasury < ConductArmyDrillsCommand.DrillCost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Insufficient funds to conduct army drills. Required: $${(ConductArmyDrillsCommand.DrillCost / 1_000_000).toFixed(0)}M, Available: $${(country.treasury / 1_000_000).toFixed(1)}M.`
      );
    }

    country.treasury -= ConductArmyDrillsCommand.DrillCost;
    session.cooldowns.setCooldown(
      this.countryId,
      'ArmyDrills',
      session.clock.currentDate.addDays(ConductArmyDrillsCommand.CooldownDays)
    );

    const op = new TimedOperation(
      this.countryId,
      'Army Drills Exercise',
      'ArmyDrills',
      session.clock.currentDate,
      session.clock.currentDate.addDays(ConductArmyDrillsCommand.DurationDays),
      ConductArmyDrillsCommand.DurationDays
    );
    session.timedOperations.push(op);

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Army Drills Commenced',
      message: `${country.name} initiated nationwide military readiness drills. Concludes in ${ConductArmyDrillsCommand.DurationDays} days.`,
      category: EventCategory.Military,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class DeclareWarCommand implements ICommand {
  aggressorCountryId: string;
  targetCountryId: string;

  constructor(aggressorCountryId: string, targetCountryId: string) {
    this.aggressorCountryId = aggressorCountryId;
    this.targetCountryId = targetCountryId;
  }

  execute(session: GameSession): CommandResult {
    if (this.aggressorCountryId.toLowerCase() === this.targetCountryId.toLowerCase()) {
      return CommandResult.failure('InvalidTarget', 'A country cannot declare war upon itself.');
    }

    const aggressor = session.countries[this.aggressorCountryId];
    if (!aggressor) {
      return CommandResult.failure('CountryNotFound', `Aggressor country '${this.aggressorCountryId}' not found.`);
    }

    const target = session.countries[this.targetCountryId];
    if (!target) {
      return CommandResult.failure('CountryNotFound', `Target country '${this.targetCountryId}' not found.`);
    }

    const aggressorStance = aggressor.getRelationsWith(this.targetCountryId);
    const targetStance = target.getRelationsWith(this.aggressorCountryId);

    if (aggressorStance.isAtWar) {
      return CommandResult.failure('AlreadyAtWar', `State of war is already active with ${target.name}.`);
    }

    if (aggressorStance.hasActiveNonAggressionPact(session.clock.currentDate)) {
      return CommandResult.failure(
        'TreatyViolationBlocked',
        `Cannot declare war! A binding Non-Aggression Pact is in effect until ${aggressorStance.nonAggressionPactUntil?.toString()}.`
      );
    }

    aggressorStance.isAtWar = true;
    targetStance.isAtWar = true;

    aggressorStance.relations = -100.0;
    targetStance.relations = -100.0;

    aggressorStance.activeTradeContracts = [];
    targetStance.activeTradeContracts = [];

    aggressor.approvalRating = Math.max(0.0, aggressor.approvalRating - 5.0);
    aggressor.stability = Math.max(0.0, aggressor.stability - 8.0);

    target.stability = Math.max(0.0, target.stability - 5.0);

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Declaration of War!',
      message: `${aggressor.name} officially declared war on ${target.name}! Commercial trade contracts terminated.`,
      category: EventCategory.Military,
      severity: EventSeverity.Critical,
      actorCountryId: this.aggressorCountryId,
      targetCountryId: this.targetCountryId
    });

    return CommandResult.success();
  }
}

export enum CombatOutcome {
  AttackerVictory = 'AttackerVictory',
  Stalemate = 'Stalemate',
  DefenderVictory = 'DefenderVictory'
}

export interface CombatReport {
  date: DateOnly;
  attackerCountryId: string;
  defenderCountryId: string;
  outcome: CombatOutcome;
  attackerCasualties: number;
  defenderCasualties: number;
  attackerCombatPower: number;
  defenderDefensePower: number;
  narrative: string;
}

export class ResolveCombatCommand implements ICommand {
  static readonly CombatCooldownDays = 15;

  attackerCountryId: string;
  defenderCountryId: string;
  lastReport: CombatReport | null = null;

  constructor(attackerCountryId: string, defenderCountryId: string) {
    this.attackerCountryId = attackerCountryId;
    this.defenderCountryId = defenderCountryId;
  }

  execute(session: GameSession): CommandResult {
    if (this.attackerCountryId.toLowerCase() === this.defenderCountryId.toLowerCase()) {
      return CommandResult.failure('InvalidTarget', 'A country cannot conduct offensive operations against itself.');
    }

    const attacker = session.countries[this.attackerCountryId];
    if (!attacker) {
      return CommandResult.failure('CountryNotFound', `Attacker country '${this.attackerCountryId}' not found.`);
    }

    const defender = session.countries[this.defenderCountryId];
    if (!defender) {
      return CommandResult.failure('CountryNotFound', `Defender country '${this.defenderCountryId}' not found.`);
    }

    const stance = attacker.getRelationsWith(this.defenderCountryId);
    if (!stance.isAtWar) {
      return CommandResult.failure('NotAtWar', `Cannot engage in combat with ${defender.name}; not currently in a state of war.`);
    }

    const cooldownKey = `Combat_${this.defenderCountryId}`;
    if (session.cooldowns.isOnCooldown(this.attackerCountryId, cooldownKey, session.clock.currentDate)) {
      const expiry = session.cooldowns.getExpiry(this.attackerCountryId, cooldownKey);
      return CommandResult.failure(
        'CombatOnCooldown',
        `Offensive command staff is reorganizing. Next operation available on ${expiry?.toString()}.`
      );
    }

    const attackerUnits = attacker.getTotalMilitaryUnits();
    if (attackerUnits <= 0) {
      return CommandResult.failure('NoMilitaryUnits', `${attacker.name} has no operational combat units in inventory.`);
    }

    const registry = session.militaryRegistry ?? MilitaryUnitRegistry.createDefaultCatalog();

    const rawAttackerPower = attacker.calculateTotalCombatPower(registry);
    const hasDrillsBuff = session.cooldowns.isOnCooldown(this.attackerCountryId, 'ArmyDrills', session.clock.currentDate);
    const drillMultiplier = hasDrillsBuff ? 1.25 : 1.0;
    const effectiveAttackerPower = Math.floor(rawAttackerPower * drillMultiplier);

    const rawDefenderPower = defender.calculateTotalDefensePower(registry);
    const effectiveDefenderPower = Math.floor(rawDefenderPower * 1.10);

    const totalPower = Math.max(1, effectiveAttackerPower + effectiveDefenderPower);
    const attackerRatio = effectiveAttackerPower / totalPower;

    let outcome: CombatOutcome;
    let attackerLossRate: number;
    let defenderLossRate: number;
    let outcomeText: string;

    if (attackerRatio >= 0.55) {
      outcome = CombatOutcome.AttackerVictory;
      attackerLossRate = 0.08;
      defenderLossRate = 0.22;
      outcomeText = 'Decisive Offensive Breakthrough';
      attacker.approvalRating = Math.min(100.0, attacker.approvalRating + 2.0);
      defender.stability = Math.max(0.0, defender.stability - 4.0);
    } else if (attackerRatio >= 0.45) {
      outcome = CombatOutcome.Stalemate;
      attackerLossRate = 0.15;
      defenderLossRate = 0.15;
      outcomeText = 'Contested Frontline Stalemate';
    } else {
      outcome = CombatOutcome.DefenderVictory;
      attackerLossRate = 0.22;
      defenderLossRate = 0.08;
      outcomeText = 'Offensive Repelled by Defense Fortifications';
      attacker.stability = Math.max(0.0, attacker.stability - 3.0);
    }

    const attackerLosses = this.applyCasualties(attacker, attackerLossRate);
    const defenderLosses = this.applyCasualties(defender, defenderLossRate);

    session.cooldowns.setCooldown(
      this.attackerCountryId,
      cooldownKey,
      session.clock.currentDate.addDays(ResolveCombatCommand.CombatCooldownDays)
    );

    const narrative = `${outcomeText}! ${attacker.name} power ${effectiveAttackerPower.toLocaleString()} vs ${defender.name} power ${effectiveDefenderPower.toLocaleString()}. Losses: ${attacker.name} lost ${attackerLosses.toLocaleString()} units, ${defender.name} lost ${defenderLosses.toLocaleString()} units.`;

    this.lastReport = {
      date: session.clock.currentDate,
      attackerCountryId: this.attackerCountryId,
      defenderCountryId: this.defenderCountryId,
      outcome,
      attackerCasualties: attackerLosses,
      defenderCasualties: defenderLosses,
      attackerCombatPower: effectiveAttackerPower,
      defenderDefensePower: effectiveDefenderPower,
      narrative
    };

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: `Battle Report: ${outcomeText}`,
      message: narrative,
      category: EventCategory.Military,
      severity: outcome === CombatOutcome.AttackerVictory ? EventSeverity.Info : EventSeverity.Warning,
      actorCountryId: this.attackerCountryId
    });

    return CommandResult.success();
  }

  private applyCasualties(country: CountryState, casualtyRate: number): number {
    let totalCasualties = 0;
    for (const unitKey of Object.keys(country.militaryUnits)) {
      const current = country.militaryUnits[unitKey];
      let lost = Math.ceil(current * casualtyRate);
      if (lost > current) lost = current;
      country.militaryUnits[unitKey] = current - lost;
      totalCasualties += lost;

      if (country.militaryUnits[unitKey] <= 0) {
        delete country.militaryUnits[unitKey];
      }
    }
    return totalCasualties;
  }
}

export class SignPeaceTreatyCommand implements ICommand {
  static readonly PeacePactDurationMonths = 24;

  initiatorCountryId: string;
  targetCountryId: string;

  constructor(initiatorCountryId: string, targetCountryId: string) {
    this.initiatorCountryId = initiatorCountryId;
    this.targetCountryId = targetCountryId;
  }

  execute(session: GameSession): CommandResult {
    if (this.initiatorCountryId.toLowerCase() === this.targetCountryId.toLowerCase()) {
      return CommandResult.failure('InvalidTarget', 'A country cannot conclude a peace treaty with itself.');
    }

    const initiator = session.countries[this.initiatorCountryId];
    if (!initiator) {
      return CommandResult.failure('CountryNotFound', `Initiator country '${this.initiatorCountryId}' not found.`);
    }

    const target = session.countries[this.targetCountryId];
    if (!target) {
      return CommandResult.failure('CountryNotFound', `Target country '${this.targetCountryId}' not found.`);
    }

    const initiatorStance = initiator.getRelationsWith(this.targetCountryId);
    const targetStance = target.getRelationsWith(this.initiatorCountryId);

    if (!initiatorStance.isAtWar) {
      return CommandResult.failure('NotAtWar', `No active hostilities exist between ${initiator.name} and ${target.name}.`);
    }

    initiatorStance.isAtWar = false;
    targetStance.isAtWar = false;

    initiatorStance.relations = -25.0;
    targetStance.relations = -25.0;

    const pactExpiry = session.clock.currentDate.addDays(SignPeaceTreatyCommand.PeacePactDurationMonths * 30);
    initiatorStance.nonAggressionPactUntil = pactExpiry;
    targetStance.nonAggressionPactUntil = pactExpiry;

    initiator.stability = Math.min(100.0, initiator.stability + 5.0);
    initiator.approvalRating = Math.min(100.0, initiator.approvalRating + 4.0);

    target.stability = Math.min(100.0, target.stability + 5.0);
    target.approvalRating = Math.min(100.0, target.approvalRating + 4.0);

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Peace Treaty Signed',
      message: `${initiator.name} and ${target.name} ceased hostilities. Non-Aggression Pact established until ${pactExpiry.toString()}.`,
      category: EventCategory.Diplomacy,
      severity: EventSeverity.Info,
      actorCountryId: this.initiatorCountryId,
      targetCountryId: this.targetCountryId
    });

    return CommandResult.success();
  }
}



export class DispatchCovertOperationCommand implements ICommand {
  static readonly OperationCost = 150_000_000;
  static readonly CooldownDays = 30;

  sponsorCountryId: string;
  targetCountryId: string;
  operationType: CovertOperationType;
  forceDetectionForTesting: boolean | null = null;

  constructor(sponsorCountryId: string, targetCountryId: string, operationType: CovertOperationType) {
    this.sponsorCountryId = sponsorCountryId;
    this.targetCountryId = targetCountryId;
    this.operationType = operationType;
  }

  execute(session: GameSession): CommandResult {
    if (this.sponsorCountryId.toLowerCase() === this.targetCountryId.toLowerCase()) {
      return CommandResult.failure('InvalidTarget', 'A sovereign nation cannot execute covert espionage on itself.');
    }

    const sponsor = session.countries[this.sponsorCountryId];
    if (!sponsor) {
      return CommandResult.failure('CountryNotFound', `Sponsor country '${this.sponsorCountryId}' not found.`);
    }

    const target = session.countries[this.targetCountryId];
    if (!target) {
      return CommandResult.failure('CountryNotFound', `Target country '${this.targetCountryId}' not found.`);
    }

    const cooldownKey = `CovertOp_${this.targetCountryId}`;
    if (session.cooldowns.isOnCooldown(this.sponsorCountryId, cooldownKey, session.clock.currentDate)) {
      const expiry = session.cooldowns.getExpiry(this.sponsorCountryId, cooldownKey);
      return CommandResult.failure(
        'ActionOnCooldown',
        `Intelligence agents in ${target.name} are establishing cover. Available on ${expiry?.toString()}.`
      );
    }

    if (sponsor.treasury < DispatchCovertOperationCommand.OperationCost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Insufficient treasury for covert operation. Required: $${(DispatchCovertOperationCommand.OperationCost / 1_000_000).toFixed(0)}M, Available: $${(sponsor.treasury / 1_000_000).toFixed(1)}M.`
      );
    }

    sponsor.treasury -= DispatchCovertOperationCommand.OperationCost;
    session.cooldowns.setCooldown(
      this.sponsorCountryId,
      cooldownKey,
      session.clock.currentDate.addDays(DispatchCovertOperationCommand.CooldownDays)
    );

    const isDetected =
      this.forceDetectionForTesting != null
        ? this.forceDetectionForTesting
        : (session.seed + session.clock.currentDate.dayNumber + Math.floor(target.stability)) % 5 === 0;

    const sponsorStance = sponsor.getRelationsWith(this.targetCountryId);
    const targetStance = target.getRelationsWith(this.sponsorCountryId);

    if (isDetected) {
      sponsorStance.modifyRelations(-25.0);
      targetStance.modifyRelations(-25.0);

      session.addEvent({
        id: Math.random().toString(36).substring(2, 11),
        date: session.clock.currentDate,
        title: 'Covert Operative Compromised!',
        message: `An intelligence agent dispatched by ${sponsor.name} was apprehended by counter-intelligence in ${target.name}! Diplomatic relations plummeted by -25.`,
        category: EventCategory.Diplomacy,
        severity: EventSeverity.Warning,
        actorCountryId: this.sponsorCountryId
      });

      return CommandResult.success();
    }

    let outcomeNarrative = 'Operation concluded without incident.';
    switch (this.operationType) {
      case CovertOperationType.GatherIntelligence: {
        const registry = session.militaryRegistry ?? MilitaryUnitRegistry.createDefaultCatalog();
        const totalUnits = target.getTotalMilitaryUnits();
        const combatPower = target.calculateTotalCombatPower(registry);
        const defensePower = target.calculateTotalDefensePower(registry);
        outcomeNarrative = `Infiltrators mapped ${target.name}'s defense network: ${totalUnits.toLocaleString()} total military units, ${combatPower.toLocaleString()} offensive power, ${defensePower.toLocaleString()} defense power.`;
        break;
      }
      case CovertOperationType.SabotageInfrastructure: {
        const factories = target.facilities['Factories'] ?? 0;
        const plants = target.facilities['PowerPlants'] ?? 0;
        if (factories > 0) target.facilities['Factories'] = factories - 1;
        if (plants > 0) target.facilities['PowerPlants'] = plants - 1;
        target.stability = Math.max(0.0, target.stability - 3.0);
        outcomeNarrative = `Covert saboteurs triggered explosive detonations across ${target.name}'s industrial grid, destroying strategic infrastructure.`;
        break;
      }
      case CovertOperationType.EmbezzleFunds: {
        const stolen = Math.min(target.treasury > 0 ? target.treasury * 0.05 : 0, 500_000_000);
        target.treasury -= stolen;
        sponsor.treasury += stolen;
        outcomeNarrative = `Covert cyber-espionage drained $${(stolen / 1_000_000).toFixed(0)}M from ${target.name}'s reserve bank accounts directly into sovereign black funds.`;
        break;
      }
    }

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: `Covert Operation Succeeded: ${this.operationType}`,
      message: outcomeNarrative,
      category: EventCategory.Military,
      severity: EventSeverity.Info,
      actorCountryId: this.sponsorCountryId
    });

    return CommandResult.success();
  }
}
