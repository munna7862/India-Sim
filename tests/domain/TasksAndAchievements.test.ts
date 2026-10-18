import { describe, it, expect } from 'vitest';
import { GameSession } from '../../src/domain/models/GameSession';
import { SimulationEngine } from '../../src/domain/services/SimulationEngine';
import {
  MinistryType,
  FundingLevel,
  TaxType,
  EventCategory,
  EventSeverity,
  GameTaskStatus,
} from '../../src/domain/models/types';
import { GameTask, TaskCategory } from '../../src/domain/tasks/GameTask';
import { AchievementRegistry } from '../../src/domain/achievements/AchievementRegistry';
import {
  AdjustTaxRateCommand,
} from '../../src/domain/commands/EconomyCommands';
import { AdjustMinistryFundingCommand } from '../../src/domain/commands/GovernmentCommands';
import {
  ClaimTaskRewardCommand,
  ClaimAchievementRewardCommand,
} from '../../src/domain/commands/EventTaskCommands';

describe('Tasks and Achievements System', () => {
  it('SimulationEngine_DispatchesCommand_AndFiresEvent', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    let eventFired = false;
    let firedCommand: any = null;
    let firedResult: any = null;

    engine.onCommandExecuted = (cmd, res) => {
      eventFired = true;
      firedCommand = cmd;
      firedResult = res;
    };

    const command = new AdjustTaxRateCommand('IND', TaxType.CorporateTax, 0.3);
    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(true);
    expect(eventFired).toBe(true);
    expect(firedCommand).toBe(command);
    expect(firedResult).toBe(result);
    expect(session.getPlayerCountry()!.corporateTaxRate).toBeCloseTo(0.3, 4);
    expect(session.events.length).toBeGreaterThan(0);
    expect(session.events.some((e) => e.category === EventCategory.Economy && e.title === 'Tax Policy Adjusted')).toBe(true);
  });

  it('AdjustMinistryFunding_UpdatesStateAndLogsWarningOnZero', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);

    const command = new AdjustMinistryFundingCommand('IND', MinistryType.Health, FundingLevel.Zero);
    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(true);
    const healthMinistry = session.getPlayerCountry()!.ministries[MinistryType.Health];
    expect(healthMinistry.funding).toBe(FundingLevel.Zero);

    const loggedEvent = session.events[session.events.length - 1];
    expect(loggedEvent).toBeDefined();
    expect(loggedEvent.severity).toBe(EventSeverity.Warning);
    expect(loggedEvent.message).toContain('Zero funding triggers sector unrest');
  });

  it('ClaimTaskReward_CompletedTask_GrantsRewardsOnceAndRejectsDoubleClaim', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;

    const startingTreasury = country.treasury;
    const startingApproval = country.approvalRating;
    const startingStability = country.stability;

    const task: GameTask = Object.assign(new GameTask(), {
      id: 'TSK_INFRA_01',
      title: 'Modernize National Energy Grid',
      category: TaskCategory.Economic,
      status: GameTaskStatus.Active,
      subTasks: [
        { id: 'ST_POWER', description: 'Construct 10 Power Plants', targetCount: 10, currentCount: 10 },
      ],
      reward: {
        treasuryBonus: 2_000_000_000,
        approvalBonus: 5.0,
        stabilityBonus: 3.0,
      },
      isRewardClaimed: false,
    });

    session.tasks[task.id] = task;

    const claimCommand = new ClaimTaskRewardCommand('IND', task.id);

    // Act 1: First claim
    const firstResult = engine.dispatch(claimCommand);

    expect(firstResult.isSuccess).toBe(true);
    expect(task.isRewardClaimed).toBe(true);
    expect(session.claimedRewardIds).toContain(task.id);
    expect(country.treasury).toBe(startingTreasury + 2_000_000_000);
    expect(country.approvalRating).toBe(startingApproval + 5.0);
    expect(country.stability).toBe(startingStability + 3.0);

    // Act 2: Duplicate claim
    const secondResult = engine.dispatch(claimCommand);

    expect(secondResult.isSuccess).toBe(false);
    expect(secondResult.errorCode).toBe('RewardAlreadyClaimed');
    expect(country.treasury).toBe(startingTreasury + 2_000_000_000);
    expect(country.approvalRating).toBe(startingApproval + 5.0);
    expect(country.stability).toBe(startingStability + 3.0);
  });

  it('ClaimTaskReward_IncompleteTask_FailsWithoutGrantingRewards', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;
    const startingTreasury = country.treasury;

    const task: GameTask = Object.assign(new GameTask(), {
      id: 'TSK_WELFARE_01',
      title: 'Expand Healthcare Network',
      category: TaskCategory.Welfare,
      status: GameTaskStatus.Active,
      subTasks: [
        { id: 'ST_HOSPITALS', description: 'Construct 25 Hospitals', targetCount: 25, currentCount: 12 },
      ],
      reward: {
        treasuryBonus: 1_000_000_000,
        approvalBonus: 2.0,
        stabilityBonus: 2.0,
      },
      isRewardClaimed: false,
    });

    session.tasks[task.id] = task;

    const command = new ClaimTaskRewardCommand('IND', task.id);
    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe('TaskNotCompleted');
    expect(task.isRewardClaimed).toBe(false);
    expect(country.treasury).toBe(startingTreasury);
  });

  it('AchievementRegistry_LoadsDefaultCatalog_ContainsExpectedDefinitions', () => {
    const registry = AchievementRegistry.createDefaultCatalog();
    expect(registry.achievements.length).toBeGreaterThanOrEqual(8);

    const reformAch = registry.tryGet('ACH_IND_REFORM');
    expect(reformAch).toBeDefined();
    expect(reformAch?.title).toBe('Economic Awakening');
    expect(reformAch?.rewardTreasury).toBe(1_000_000_000);
  });

  it('EvaluateAchievements_PolicyCount_UnlocksWhenThresholdMet', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const player = session.getPlayerCountry()!;

    engine.evaluateAchievements();
    const achProgress = session.achievements['ACH_IND_REFORM'];
    expect(achProgress.isUnlocked).toBe(false);
    expect(achProgress.progressPercent).toBe(0.0);

    // Enact 2 policies
    player.activePolicies.push('agri_subsidies', 'industrial_automation');

    engine.evaluateAchievements();
    expect(achProgress.isUnlocked).toBe(true);
    expect(achProgress.progressPercent).toBe(100.0);
    expect(achProgress.unlockedDate).not.toBeNull();
    expect(session.events.some((e) => e.category === EventCategory.Achievements && e.title.includes('Economic Awakening'))).toBe(true);
  });

  it('ClaimAchievementReward_ValidClaim_CreditsTreasuryAndMarksClaimed', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const player = session.getPlayerCountry()!;

    player.activePolicies.push('agri_subsidies', 'industrial_automation');
    engine.evaluateAchievements();

    const initialTreasury = player.treasury;

    const cmd = new ClaimAchievementRewardCommand('ACH_IND_REFORM', player.id);
    const result = engine.dispatch(cmd);

    expect(result.isSuccess).toBe(true);
    expect(player.treasury).toBe(initialTreasury + 1_000_000_000);

    const progress = session.achievements['ACH_IND_REFORM'];
    expect(progress.isRewardClaimed).toBe(true);
    expect(session.claimedAchievementRewardIds).toContain('ACH_IND_REFORM');
  });
});
