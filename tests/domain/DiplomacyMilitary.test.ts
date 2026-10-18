import { describe, it, expect } from 'vitest';
import { GameSession } from '../../src/domain/models/GameSession';
import { DiplomaticGiftTier, TradeContract } from '../../src/domain/models/Diplomacy';
import { MilitaryUnitRegistry, MilitaryBranch } from '../../src/domain/military/MilitaryUnitDefinition';
import { FundingLevel } from '../../src/domain/models/types';
import { SaveManager, MemorySaveStorage } from '../../src/domain/services/SaveManager';
import {
  SendDiplomaticGiftCommand,
  SignNonAggressionPactCommand,
  SignTradeContractCommand,
  CancelTradeContractCommand,
  RecruitMilitaryUnitsCommand,
  DisbandMilitaryUnitsCommand,
  DeclareWarCommand,
  ResolveCombatCommand,
  SignPeaceTreatyCommand,
  DispatchCovertOperationCommand,
  CovertOperationType,
} from '../../src/domain/commands/DiplomacyMilitaryCommands';
import {
  FundPresidentialProtectionCommand,
  LaunchCounterTerrorismCommand,
} from '../../src/domain/commands/GovernmentCommands';
import { ExecuteMonthlyLedgerCommand } from '../../src/domain/commands/EconomyCommands';

describe('Diplomacy & Military System', () => {
  it('SendDiplomaticGift_ValidSmallTier_TransfersFundsAndImprovesRelations', () => {
    const session = GameSession.createNewGame('IND');
    const india = session.getPlayerCountry()!;
    const usa = session.countries['USA'];

    const indiaStartingTreasury = india.treasury;
    const usaStartingTreasury = usa.treasury;
    const startingRelations = india.getRelationsWith('USA').relations;

    const cmd = new SendDiplomaticGiftCommand('IND', 'USA', DiplomaticGiftTier.Small);
    const result = cmd.execute(session);

    expect(result.isSuccess).toBe(true);
    expect(india.treasury).toBe(indiaStartingTreasury - 250_000_000);
    expect(usa.treasury).toBe(usaStartingTreasury + 250_000_000);
    expect(india.getRelationsWith('USA').relations).toBe(startingRelations + 5.0);
    expect(usa.getRelationsWith('IND').relations).toBe(startingRelations + 5.0);
    expect(session.cooldowns.isOnCooldown('IND', 'DiplomaticGift_USA', session.clock.currentDate)).toBe(true);
  });

  it('SendDiplomaticGift_OnCooldown_RejectsCommand', () => {
    const session = GameSession.createNewGame('IND');
    const cmd = new SendDiplomaticGiftCommand('IND', 'USA', DiplomaticGiftTier.Small);
    cmd.execute(session);

    const result2 = cmd.execute(session);
    expect(result2.isSuccess).toBe(false);
    expect(result2.errorCode).toBe('ActionOnCooldown');
  });

  it('SendDiplomaticGift_InvalidSelfOrAtWar_RejectsCommand', () => {
    const session = GameSession.createNewGame('IND');
    const selfGift = new SendDiplomaticGiftCommand('IND', 'IND', DiplomaticGiftTier.Small);
    const selfResult = selfGift.execute(session);
    expect(selfResult.isSuccess).toBe(false);
    expect(selfResult.errorCode).toBe('InvalidTarget');

    session.countries['IND'].getRelationsWith('PAK').isAtWar = true;
    const warGift = new SendDiplomaticGiftCommand('IND', 'PAK', DiplomaticGiftTier.Small);
    const warResult = warGift.execute(session);
    expect(warResult.isSuccess).toBe(false);
    expect(warResult.errorCode).toBe('AtWar');
  });

  it('SignNonAggressionPact_ValidConditions_Signs24MonthPactAndImprovesRelations', () => {
    const session = GameSession.createNewGame('IND');
    const india = session.getPlayerCountry()!;
    const rus = session.countries['RUS'];

    const startingTreasury = india.treasury;
    const startingRel = india.getRelationsWith('RUS').relations;

    const cmd = new SignNonAggressionPactCommand('IND', 'RUS', 24);
    const result = cmd.execute(session);

    expect(result.isSuccess).toBe(true);
    expect(india.treasury).toBe(startingTreasury - 50_000_000);
    expect(india.getRelationsWith('RUS').relations).toBe(startingRel + 10.0);
    expect(rus.getRelationsWith('IND').relations).toBe(startingRel + 10.0);

    expect(india.getRelationsWith('RUS').hasActiveNonAggressionPact(session.clock.currentDate)).toBe(true);
    expect(rus.getRelationsWith('IND').hasActiveNonAggressionPact(session.clock.currentDate)).toBe(true);
  });

  it('SignNonAggressionPact_HostileNation_RejectsDueToLowRelations', () => {
    const session = GameSession.createNewGame('IND');
    const cmd = new SignNonAggressionPactCommand('IND', 'PAK', 24);
    const result = cmd.execute(session);

    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe('RelationsTooLow');
  });

  it('SignNonAggressionPact_ActivePactExists_RejectsDuplicate', () => {
    const session = GameSession.createNewGame('IND');
    const cmd = new SignNonAggressionPactCommand('IND', 'RUS', 24);
    cmd.execute(session);

    const duplicateResult = cmd.execute(session);
    expect(duplicateResult.isSuccess).toBe(false);
    expect(duplicateResult.errorCode).toBe('ActivePactExists');
  });

  it('SignTradeContract_ValidSurplus_EstablishesTradeAgreement', () => {
    const session = GameSession.createNewGame('IND');
    const india = session.getPlayerCountry()!;
    const gbr = session.countries['GBR'];

    const startingRel = india.getRelationsWith('GBR').relations;

    const cmd = new SignTradeContractCommand('IND', 'GBR', 'Food', 5000.0, 200_000_000, 24);
    const result = cmd.execute(session);

    expect(result.isSuccess).toBe(true);
    const indStance = india.getRelationsWith('GBR');
    const gbrStance = gbr.getRelationsWith('IND');

    expect(indStance.activeTradeContracts.length).toBe(1);
    expect(gbrStance.activeTradeContracts.length).toBe(1);
    expect(indStance.relations).toBe(startingRel + 5.0);
  });

  it('CancelTradeContract_RemovesContractFromBothNations_WithPenalty', () => {
    const session = GameSession.createNewGame('IND');
    const cmd = new SignTradeContractCommand('IND', 'GBR', 'Food', 5000.0, 200_000_000, 24);
    cmd.execute(session);

    const contract = session.getPlayerCountry()!.getRelationsWith('GBR').activeTradeContracts[0];
    const cancelCmd = new CancelTradeContractCommand('IND', contract.id);
    const cancelResult = cancelCmd.execute(session);

    expect(cancelResult.isSuccess).toBe(true);
    expect(session.getPlayerCountry()!.getRelationsWith('GBR').activeTradeContracts.length).toBe(0);
    expect(session.countries['GBR'].getRelationsWith('IND').activeTradeContracts.length).toBe(0);
  });

  it('ExecuteMonthlyLedger_ProcessesTradeRevenuesAndResourceExchanges', () => {
    const session = GameSession.createNewGame('IND');
    const india = session.getPlayerCountry()!;

    const cmd = new SignTradeContractCommand('IND', 'GBR', 'Food', 10000.0, 300_000_000, 24);
    cmd.execute(session);

    const ledgerCmd = new ExecuteMonthlyLedgerCommand('IND');
    const ledgerResult = ledgerCmd.execute(session);

    expect(ledgerResult.isSuccess).toBe(true);
    expect(india.lastBudgetReport).not.toBeNull();
    expect(india.lastBudgetReport!.tradeExportRevenue).toBe(300_000_000);
  });

  it('MilitaryUnitRegistry_Contains12DistinctUnitsAcrossBranches', () => {
    const registry = MilitaryUnitRegistry.createDefaultCatalog();
    expect(registry.getAll().length).toBe(12);

    const inf = registry.tryGet('infantry_division');
    expect(inf).toBeDefined();
    expect(inf?.branch).toBe(MilitaryBranch.Land);

    const fighter = registry.tryGet('multirole_fighter');
    expect(fighter).toBeDefined();
    expect(fighter?.branch).toBe(MilitaryBranch.Air);

    const carrier = registry.tryGet('aircraft_carrier');
    expect(carrier).toBeDefined();
    expect(carrier?.branch).toBe(MilitaryBranch.Naval);
  });

  it('RecruitMilitaryUnits_ValidResourcesAndBarracks_AddsUnitsToInventory', () => {
    const session = GameSession.createNewGame('IND');
    const india = session.getPlayerCountry()!;

    const initialTanks = india.militaryUnits['main_battle_tank'] ?? 0;
    const initialTreasury = india.treasury;
    const initialIron = india.stockpiles.iron;
    const initialOil = india.stockpiles.oil;

    const cmd = new RecruitMilitaryUnitsCommand('IND', 'main_battle_tank', 5);
    const result = cmd.execute(session);

    expect(result.isSuccess).toBe(true);
    expect(india.militaryUnits['main_battle_tank']).toBe(initialTanks + 5);
    expect(india.treasury).toBe(initialTreasury - 200_000_000 * 5);
    expect(india.stockpiles.iron).toBe(initialIron - 800.0 * 5);
    expect(india.stockpiles.oil).toBe(initialOil - 300.0 * 5);
  });

  it('RecruitMilitaryUnits_InsufficientBarracks_RejectsCommand', () => {
    const session = GameSession.createNewGame('IND');
    const india = session.getPlayerCountry()!;
    india.facilities['Barracks'] = 2; // Carrier requires 20

    const cmd = new RecruitMilitaryUnitsCommand('IND', 'aircraft_carrier', 1);
    const result = cmd.execute(session);

    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe('InsufficientBarracks');
  });

  it('DisbandMilitaryUnits_ValidQuantity_ReducesInventory', () => {
    const session = GameSession.createNewGame('IND');
    const india = session.getPlayerCountry()!;

    const initialTanks = india.militaryUnits['main_battle_tank'] ?? 10;
    const cmd = new DisbandMilitaryUnitsCommand('IND', 'main_battle_tank', 2);
    const result = cmd.execute(session);

    expect(result.isSuccess).toBe(true);
    expect(india.militaryUnits['main_battle_tank']).toBe(initialTanks - 2);
  });

  it('DeclareWar_ValidHostilities_EstablishesWarAndTerminatesTrade', () => {
    const session = GameSession.createNewGame('IND');
    const india = session.getPlayerCountry()!;
    const pak = session.countries['PAK'];

    const trade: TradeContract = {
      id: 'TC_TEST',
      exporterCountryId: 'IND',
      importerCountryId: 'PAK',
      resourceType: 'Food',
      monthlyQuantity: 100,
      monthlyPayment: 10_000_000,
      startDate: session.clock.currentDate,
    };
    india.getRelationsWith('PAK').activeTradeContracts.push(trade);
    pak.getRelationsWith('IND').activeTradeContracts.push(trade);

    const warCmd = new DeclareWarCommand('IND', 'PAK');
    const warResult = warCmd.execute(session);

    expect(warResult.isSuccess).toBe(true);
    expect(india.getRelationsWith('PAK').isAtWar).toBe(true);
    expect(pak.getRelationsWith('IND').isAtWar).toBe(true);
    expect(india.getRelationsWith('PAK').relations).toBe(-100.0);
    expect(india.getRelationsWith('PAK').activeTradeContracts.length).toBe(0);
  });

  it('DeclareWar_WhenActiveNAP_BlockedByTreaty', () => {
    const session = GameSession.createNewGame('IND');
    const india = session.getPlayerCountry()!;
    india.getRelationsWith('USA').nonAggressionPactUntil = session.clock.currentDate.addDays(365);

    const warCmd = new DeclareWarCommand('IND', 'USA');
    const warResult = warCmd.execute(session);

    expect(warResult.isSuccess).toBe(false);
    expect(warResult.errorCode).toBe('TreatyViolationBlocked');
    expect(india.getRelationsWith('USA').isAtWar).toBe(false);
  });

  it('ResolveCombat_AtWar_CalculatesCasualtiesAndOutcome', () => {
    const session = GameSession.createNewGame('IND');
    session.getPlayerCountry()!.getRelationsWith('PAK').isAtWar = true;
    session.countries['PAK'].getRelationsWith('IND').isAtWar = true;

    const indUnitsBefore = session.getPlayerCountry()!.getTotalMilitaryUnits();
    const pakUnitsBefore = session.countries['PAK'].getTotalMilitaryUnits();

    const combatCmd = new ResolveCombatCommand('IND', 'PAK');
    const result = combatCmd.execute(session);

    expect(result.isSuccess).toBe(true);
    expect(combatCmd.lastReport).not.toBeNull();

    const indUnitsAfter = session.getPlayerCountry()!.getTotalMilitaryUnits();
    const pakUnitsAfter = session.countries['PAK'].getTotalMilitaryUnits();

    expect(indUnitsAfter).toBeLessThan(indUnitsBefore);
    expect(pakUnitsAfter).toBeLessThan(pakUnitsBefore);
    expect(session.cooldowns.isOnCooldown('IND', 'Combat_PAK', session.clock.currentDate)).toBe(true);
  });

  it('SignPeaceTreaty_WhenAtWar_EndsHostilitiesAndEstablishesNAP', () => {
    const session = GameSession.createNewGame('IND');
    const ind = session.getPlayerCountry()!;
    const pak = session.countries['PAK'];

    ind.getRelationsWith('PAK').isAtWar = true;
    pak.getRelationsWith('IND').isAtWar = true;

    const peaceCmd = new SignPeaceTreatyCommand('IND', 'PAK');
    const peaceResult = peaceCmd.execute(session);

    expect(peaceResult.isSuccess).toBe(true);
    expect(ind.getRelationsWith('PAK').isAtWar).toBe(false);
    expect(pak.getRelationsWith('IND').isAtWar).toBe(false);
    expect(ind.getRelationsWith('PAK').relations).toBe(-25.0);
    expect(ind.getRelationsWith('PAK').hasActiveNonAggressionPact(session.clock.currentDate)).toBe(true);
  });

  it('InternalSecurityAndEspionage_ActionsExecuteCleanly', () => {
    const session = GameSession.createNewGame('IND');
    const ind = session.getPlayerCountry()!;

    // Presidential Protection
    const protCmd = new FundPresidentialProtectionCommand('IND', FundingLevel.Maximum);
    expect(protCmd.execute(session).isSuccess).toBe(true);
    expect(ind.presidentialProtectionLevel).toBe(FundingLevel.Maximum);

    // Counter-Terrorism
    const ctCmd = new LaunchCounterTerrorismCommand('IND');
    expect(ctCmd.execute(session).isSuccess).toBe(true);
    expect(session.cooldowns.isOnCooldown('IND', 'CounterTerrorism', session.clock.currentDate)).toBe(true);

    // Covert Operation (Intel Gathering)
    const spyCmd = new DispatchCovertOperationCommand('IND', 'USA', CovertOperationType.GatherIntelligence);
    spyCmd.forceDetectionForTesting = false;
    expect(spyCmd.execute(session).isSuccess).toBe(true);

    // Covert Operation (Compromised agent detection)
    const detectedSpyCmd = new DispatchCovertOperationCommand('IND', 'CHN', CovertOperationType.SabotageInfrastructure);
    detectedSpyCmd.forceDetectionForTesting = true;
    const chnRelBefore = ind.getRelationsWith('CHN').relations;
    expect(detectedSpyCmd.execute(session).isSuccess).toBe(true);
    expect(ind.getRelationsWith('CHN').relations).toBe(chnRelBefore - 25.0);
  });
});
