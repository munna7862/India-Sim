import { describe, it, expect } from 'vitest';
import { GameSession } from '../../src/domain/models/GameSession';
import { SimulationEngine } from '../../src/domain/services/SimulationEngine';
import { SaveManager, MemorySaveStorage } from '../../src/domain/services/SaveManager';
import { TaxType, GameTaskStatus } from '../../src/domain/models/types';
import {
  AdjustTaxRateCommand,
  EnactEconomicPolicyCommand,
} from '../../src/domain/commands/EconomyCommands';
import {
  SignNonAggressionPactCommand,
  ConductArmyDrillsCommand,
} from '../../src/domain/commands/DiplomacyMilitaryCommands';
import {
  ClaimAchievementRewardCommand,
  ClaimTaskRewardCommand,
  DismissNotificationCommand,
} from '../../src/domain/commands/EventTaskCommands';

describe('Cross-System Gameplay Parity', () => {
  it('CrossSystemParity_EndToEndGameplayProgression_UnlocksTasksAndAchievements', () => {
    // 1. Initialize India game session
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const player = session.getPlayerCountry()!;

    // 2. Adjust Taxes
    const taxCmd = new AdjustTaxRateCommand('IND', TaxType.IncomeTax, 0.18);
    const taxRes = engine.dispatch(taxCmd);
    expect(taxRes.isSuccess).toBe(true);

    // 3. Enact Economic Policies
    const pol1 = new EnactEconomicPolicyCommand('IND', 'agricultural_subsidies');
    expect(engine.dispatch(pol1).isSuccess).toBe(true);

    const pol2 = new EnactEconomicPolicyCommand('IND', 'industrial_modernization');
    expect(engine.dispatch(pol2).isSuccess).toBe(true);

    // 4. Verify Policy-based Achievement Unlocked via evaluateAchievements
    engine.evaluateAchievements();
    expect(session.achievements['ACH_IND_REFORM'].isUnlocked).toBe(true);
    const treasuryBeforeClaim = player.treasury;

    const claimAchCmd = new ClaimAchievementRewardCommand('ACH_IND_REFORM', 'IND');
    const claimAchRes = engine.dispatch(claimAchCmd);
    expect(claimAchRes.isSuccess).toBe(true);
    expect(player.treasury).toBe(treasuryBeforeClaim + 1_000_000_000);

    // 5. Foreign Diplomacy: Sign NAPs with USA and RUS
    const nap1 = new SignNonAggressionPactCommand('IND', 'USA');
    expect(engine.dispatch(nap1).isSuccess).toBe(true);

    const nap2 = new SignNonAggressionPactCommand('IND', 'RUS');
    expect(engine.dispatch(nap2).isSuccess).toBe(true);

    // 6. Verify Peace Maker Achievement
    engine.evaluateAchievements();
    expect(session.achievements['ACH_PEACE_MAKER'].isUnlocked).toBe(true);
    const claimPeaceRes = engine.dispatch(new ClaimAchievementRewardCommand('ACH_PEACE_MAKER', 'IND'));
    expect(claimPeaceRes.isSuccess).toBe(true);

    // 7. Complete Task via facility expansion
    player.facilities['PowerPlants'] = 35;
    player.facilities['Farms'] = 60;
    player.facilities['Factories'] = 50;

    engine.evaluateTasks();

    const agriTask = Object.values(session.tasks).find((t) => t.id === 'TASK_MOD_01');
    if (agriTask) {
      expect(agriTask.status === GameTaskStatus.Completed || agriTask.checkCompletion?.()).toBe(true);
      const claimTaskCmd = new ClaimTaskRewardCommand('IND', agriTask.id);
      expect(engine.dispatch(claimTaskCmd).isSuccess).toBe(true);
      expect(session.claimedRewardIds).toContain(agriTask.id);
    }

    // 8. Event Feed & Notifications check
    expect(session.events.length).toBeGreaterThan(0);

    // Dismiss all notifications
    const dismissRes = engine.dispatch(new DismissNotificationCommand(null));
    expect(dismissRes.isSuccess).toBe(true);
    expect(session.events.every((e) => e.isDismissed)).toBe(true);
  });

  it('CrossSystemParity_MidOperationSaveAndLoad_PreservesAuthoritativeStateAndTimers', () => {
    const memoryStorage = new MemorySaveStorage();
    const saveManager = new SaveManager(memoryStorage);

    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);

    // 1. Enqueue Timed Operation (Army Drills)
    const drillsCmd = new ConductArmyDrillsCommand('IND');
    expect(engine.dispatch(drillsCmd).isSuccess).toBe(true);
    expect(session.timedOperations.length).toBe(1);

    const op = session.timedOperations[0];
    expect(op.operationType).toBe('ArmyDrills');

    // 2. Advance 15 days
    session.clock.advanceDays(15);
    engine.tick(0.0);

    // 3. Save session
    const saveResult = saveManager.save('drills_progress', session);
    expect(saveResult.isSuccess).toBe(true);

    // 4. Restore session from save
    const loadResult = saveManager.load('drills_progress');
    expect(loadResult.isSuccess).toBe(true);

    const loadedSession = loadResult.session!;
    expect(loadedSession.timedOperations.length).toBe(1);
    const loadedOp = loadedSession.timedOperations[0];
    expect(loadedOp.operationType).toBe('ArmyDrills');
    expect(loadedOp.startDate.equals(op.startDate)).toBe(true);
    expect(loadedSession.clock.currentDate.equals(session.clock.currentDate)).toBe(true);

    // 5. Complete operation by advancing remaining days
    const loadedEngine = new SimulationEngine(loadedSession);
    loadedSession.clock.advanceDays(16); // 15 + 16 = 31 > 30 days
    loadedEngine.tick(0.0);

    expect(loadedSession.timedOperations.length).toBe(0);
    expect(loadedSession.events.some((e) => e.title.includes('Army Drills Concluded'))).toBe(true);
  });
});
