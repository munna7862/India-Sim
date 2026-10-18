import { GameSession } from '../models/GameSession';
import { CommandResult, ICommand } from './CommandResult';
import { EventCategory, EventSeverity, GameTaskStatus } from '../models/types';
import { AchievementRegistry } from '../achievements/AchievementRegistry';

export class ResolveEventChoiceCommand implements ICommand {
  countryId: string;
  eventId: string;
  choiceId: string;

  constructor(countryId: string, eventId: string, choiceId: string) {
    this.countryId = countryId;
    this.eventId = eventId;
    this.choiceId = choiceId;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found in world state.`);
    }

    if (!session.pendingEvent) {
      return CommandResult.failure('NoPendingEvent', 'There is no active pending emergency event awaiting resolution.');
    }

    if (session.pendingEvent.eventId.toLowerCase() !== this.eventId.toLowerCase()) {
      return CommandResult.failure(
        'MismatchedEvent',
        `Pending event '${session.pendingEvent.eventId}' does not match requested event '${this.eventId}'.`
      );
    }

    const choice = session.pendingEvent.choices.find((c) => c.id.toLowerCase() === this.choiceId.toLowerCase());
    if (!choice) {
      return CommandResult.failure('InvalidChoice', `Choice '${this.choiceId}' is not valid for event '${this.eventId}'.`);
    }

    if (country.treasury < choice.treasuryCost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Treasury cannot cover response cost of $${(choice.treasuryCost / 1_000_000).toFixed(0)}M.`
      );
    }

    country.treasury -= choice.treasuryCost;
    country.approvalRating = Math.max(0.0, Math.min(100.0, country.approvalRating + choice.approvalDelta));
    country.stability = Math.max(0.0, Math.min(100.0, country.stability + choice.stabilityDelta));
    country.corruptionIndex = Math.max(0.0, Math.min(100.0, country.corruptionIndex + choice.corruptionDelta));

    session.pendingEvent = null;
    session.eventCooldowns[this.eventId] = 12;

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: `Emergency Resolved: ${choice.title}`,
      message: `${country.name} executed: ${choice.description}`,
      category: EventCategory.Emergency,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class ClaimTaskRewardCommand implements ICommand {
  countryId: string;
  taskId: string;

  constructor(countryId: string, taskId: string) {
    this.countryId = countryId;
    this.taskId = taskId;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    const task = session.tasks[this.taskId];
    if (!task) {
      return CommandResult.failure('TaskNotFound', `Task '${this.taskId}' does not exist.`);
    }

    const isCompleted = typeof task.checkCompletion === 'function'
      ? task.checkCompletion()
      : (task.status === GameTaskStatus.Completed || (task.subTasks && task.subTasks.length > 0 && task.subTasks.every((st: any) => st.currentCount >= st.targetCount)));

    if (!isCompleted) {
      return CommandResult.failure('TaskNotCompleted', `Task '${task.title}' is not yet completed.`);
    }

    if (task.isRewardClaimed || session.claimedRewardIds.includes(this.taskId)) {
      return CommandResult.failure('RewardAlreadyClaimed', `Reward for task '${task.title}' has already been claimed.`);
    }

    country.treasury += task.reward.treasuryBonus;
    country.approvalRating = Math.max(0.0, Math.min(100.0, country.approvalRating + task.reward.approvalBonus));
    country.stability = Math.max(0.0, Math.min(100.0, country.stability + task.reward.stabilityBonus));

    task.isRewardClaimed = true;
    if (!session.claimedRewardIds.includes(this.taskId)) {
      session.claimedRewardIds.push(this.taskId);
    }

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Presidential Task Reward Claimed',
      message: `${country.name} completed '${task.title}' and claimed reward: +$${(task.reward.treasuryBonus / 1_000_000).toFixed(0)}M Treasury, +${task.reward.approvalBonus.toFixed(1)}% Approval, +${task.reward.stabilityBonus.toFixed(1)}% Stability.`,
      category: EventCategory.Tasks,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class ClaimAchievementRewardCommand implements ICommand {
  achievementId: string;
  countryId: string;

  constructor(achievementId: string, countryId: string) {
    if (!achievementId) throw new Error('AchievementId cannot be empty');
    if (!countryId) throw new Error('CountryId cannot be empty');
    this.achievementId = achievementId;
    this.countryId = countryId;
  }

  validate(session: GameSession): CommandResult {
    if (!session.countries[this.countryId]) {
      return CommandResult.failure('CountryNotFound', `Country '${this.countryId}' not found in active session.`);
    }

    const progress = session.achievements[this.achievementId];
    if (!progress) {
      return CommandResult.failure('NoProgress', `Achievement '${this.achievementId}' has no recorded progress.`);
    }

    if (!progress.isUnlocked) {
      return CommandResult.failure('NotUnlocked', `Achievement '${this.achievementId}' has not been unlocked yet.`);
    }

    if (progress.isRewardClaimed || session.claimedAchievementRewardIds.includes(this.achievementId)) {
      return CommandResult.failure('AlreadyClaimed', `Reward for achievement '${this.achievementId}' has already been claimed.`);
    }

    const registry = session.achievementRegistry ?? AchievementRegistry.createDefaultCatalog();
    if (!registry.tryGet(this.achievementId)) {
      return CommandResult.failure('DefinitionNotFound', `Achievement definition for '${this.achievementId}' not found in registry.`);
    }

    return CommandResult.success();
  }

  execute(session: GameSession): CommandResult {
    const val = this.validate(session);
    if (!val.isSuccess) return val;

    const country = session.countries[this.countryId];
    const progress = session.achievements[this.achievementId];
    const registry = session.achievementRegistry ?? AchievementRegistry.createDefaultCatalog();
    const def = registry.get(this.achievementId);

    country.treasury += def.rewardTreasury;
    progress.isRewardClaimed = true;
    progress.claimedDate = session.clock.currentDate;

    if (!session.claimedAchievementRewardIds.includes(this.achievementId)) {
      session.claimedAchievementRewardIds.push(this.achievementId);
    }

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: `Achievement Reward Claimed: ${def.title}`,
      message: `${country.name} claimed prestige reward for '${def.title}': +$${(def.rewardTreasury / 1_000_000).toFixed(0)}M Treasury, +${def.rewardPrestigeScore} Prestige.`,
      category: EventCategory.Achievements,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class DismissNotificationCommand implements ICommand {
  notificationId?: string | null;

  constructor(notificationId?: string | null) {
    this.notificationId = notificationId;
  }

  execute(session: GameSession): CommandResult {
    if (!this.notificationId) {
      for (const e of session.events) {
        e.isRead = true;
        e.isDismissed = true;
      }
      return CommandResult.success();
    }

    const evt = session.events.find((e) => e.id.toLowerCase() === this.notificationId!.toLowerCase());
    if (!evt) {
      return CommandResult.failure('NotificationNotFound', `Notification with ID '${this.notificationId}' was not found.`);
    }

    evt.isRead = true;
    evt.isDismissed = true;
    return CommandResult.success();
  }
}
