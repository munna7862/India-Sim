import { GameSession } from '../models/GameSession';
import { CommandResult, ICommand } from '../commands/CommandResult';
import { DateOnly } from '../models/DateOnly';
import { TimedOperation } from '../models/TimedOperation';
import { EventCategory, EventSeverity, FundingLevel, GameTaskStatus, MinistryType } from '../models/types';
import { CountryState } from '../models/CountryState';
import { Ideology } from '../models/Ideology';
import { DomesticEventRegistry } from '../events/DomesticEventRegistry';
import { EconomicPolicyRegistry } from '../economy/EconomicPolicy';
import { AchievementRegistry } from '../achievements/AchievementRegistry';
import { WorldEventRegistry } from '../events/WorldEventRegistry';
import { ExecuteMonthlyLedgerCommand } from '../commands/EconomyCommands';
import { ActiveEventPrompt } from '../events/DomesticEvent';
import { AchievementProgress } from '../achievements/Achievement';
import { MinistryState } from '../models/MinistryState';

export class SimulationEngine {
  private _session: GameSession;

  get session(): GameSession {
    return this._session;
  }
  set session(s: GameSession) {
    this._session = s;
    this.wireClockEvents();
  }

  onCommandExecuted?: (command: ICommand, result: CommandResult) => void;
  onDayTicked?: (date: DateOnly) => void;
  onMonthRollover?: (date: DateOnly) => void;

  constructor(session: GameSession) {
    if (!session) throw new Error('GameSession is required');
    this._session = session;
    this._session.policyRegistry ??= EconomicPolicyRegistry.createDefaultCatalog();
    this._session.achievementRegistry ??= AchievementRegistry.createDefaultCatalog();
    this._session.worldEventRegistry ??= WorldEventRegistry.createDefaultCatalog();
    this._session.domesticEventRegistry ??= DomesticEventRegistry.createDefaultCatalog();
    this.wireClockEvents();
  }

  private wireClockEvents(): void {
    this._session.clock.onDayPassed = (d) => this.handleDayPassed(d);
    this._session.clock.onMonthPassed = (m) => this.handleMonthPassed(m);
  }

  tick(deltaSeconds: number): { dateChanged: boolean; monthRolledOver: boolean } {
    const prevDate = this._session.clock.currentDate;
    const prevMonth = prevDate.month;
    this._session.clock.tick(deltaSeconds);
    const newDate = this._session.clock.currentDate;
    const dateChanged = !newDate.equals(prevDate);
    const monthRolledOver = newDate.month !== prevMonth;
    return { dateChanged, monthRolledOver };
  }

  dispatch(command: ICommand): CommandResult {
    if (!command) throw new Error('Command is null');
    const result = command.execute(this._session);
    this.onCommandExecuted?.(command, result);
    return result;
  }

  private handleDayPassed(newDate: DateOnly): void {
    // 1. Process Timed Operations
    const remainingOps: TimedOperation[] = [];
    for (const op of this._session.timedOperations) {
      if (!op.isCompleted && newDate.isSameOrAfter(op.completionDate)) {
        this.completeTimedOperation(op);
      } else {
        remainingOps.push(op);
      }
    }
    this._session.timedOperations = remainingOps;

    // 2. Clear expired cooldowns
    this._session.cooldowns.clearExpired(newDate);

    this.onDayTicked?.(newDate);
  }

  private completeTimedOperation(op: TimedOperation): void {
    op.isCompleted = true;
    const country = this._session.countries[op.countryId];
    if (!country) return;

    switch (op.operationType) {
      case 'Construction': {
        const facility = op.parameters['FacilityType'] || 'Farms';
        const qty = parseInt(op.parameters['Quantity'] || '1', 10) || 1;
        const count = country.facilities[facility] ?? 0;
        country.facilities[facility] = count + qty;

        this._session.addEvent({
          id: Math.random().toString(36).substring(2, 11),
          date: this._session.clock.currentDate,
          title: 'Facility Construction Finished',
          message: `${country.name} completed construction of ${qty}x ${facility} (Total: ${country.facilities[facility]}).`,
          category: EventCategory.Economy,
          severity: EventSeverity.Info,
          actorCountryId: op.countryId
        });
        break;
      }
      case 'ArmyDrills': {
        country.stability = Math.min(100.0, country.stability + 2.0);
        this._session.addEvent({
          id: Math.random().toString(36).substring(2, 11),
          date: this._session.clock.currentDate,
          title: 'Army Drills Concluded',
          message: `${country.name} successfully concluded comprehensive army drills, improving force readiness and national stability.`,
          category: EventCategory.Military,
          severity: EventSeverity.Info,
          actorCountryId: op.countryId
        });
        break;
      }
    }
  }

  private handleMonthPassed(newDate: DateOnly): void {
    // 1. Execute invariant monthly ledger across all countries
    for (const country of Object.values(this._session.countries)) {
      const ledgerCmd = new ExecuteMonthlyLedgerCommand(country.id);
      ledgerCmd.execute(this._session);

      // Update national indicators
      this.updateNationalIndicators(country);
    }

    // 2. Process domestic events
    this.processDomesticEvents(newDate);

    // 3. Process world events
    this.processWorldEvents(newDate);

    // 4. Evaluate tasks against authoritative world state
    this.evaluateTasks();

    // 5. Evaluate achievements
    this.evaluateAchievements();

    this.onMonthRollover?.(newDate);
  }

  private getMinistryEfficiency(ministry?: MinistryState): number {
    if (!ministry) return 0.50;
    if (typeof ministry.calculateEfficiency === 'function') {
      return ministry.calculateEfficiency();
    }
    const competence = ministry.activeMinister?.competence ?? 0.70;
    let fundingMult = 1.0;
    const f = Number(ministry.funding ?? (ministry as any).fundingLevel ?? 50);
    if (f === 0) fundingMult = 0.0;
    else if (f === 25 || f === 1) fundingMult = 0.70;
    else if (f === 50 || f === 2) fundingMult = 1.0;
    else if (f === 75 || f === 3) fundingMult = 1.20;
    else if (f === 100 || f === 4) fundingMult = 1.35;
    return Math.min(1.0, competence * fundingMult);
  }

  updateNationalIndicators(country: CountryState): void {
    // 1. Quality of living
    const hlth = country.ministries[MinistryType.Health];
    const healthEff = this.getMinistryEfficiency(hlth);
    const hospitals = country.facilities['Hospitals'] ?? 10;
    const healthScore = Math.max(0.0, Math.min(1.0, healthEff * (hospitals / 30.0)));

    const edu = country.ministries[MinistryType.Education];
    const eduEff = this.getMinistryEfficiency(edu);
    const schools = country.facilities['Schools'] ?? 10;
    const eduScore = Math.max(0.0, Math.min(1.0, eduEff * (schools / 40.0)));

    const infra = country.ministries[MinistryType.Infrastructure];
    const infraEff = this.getMinistryEfficiency(infra);
    const power = country.facilities['PowerPlants'] ?? 10;
    const infraScore = Math.max(0.0, Math.min(1.0, infraEff * (power / 40.0)));

    const econScore = Math.max(0.0, Math.min(1.0, (1.0 - country.incomeTaxRate) * (1.0 - (country.corruptionIndex / 100.0))));

    const targetQol = (healthScore * 0.30 + eduScore * 0.25 + infraScore * 0.25 + econScore * 0.20) * 100.0;
    country.qualityOfLiving = Math.max(0.0, Math.min(100.0, country.qualityOfLiving + (targetQol - country.qualityOfLiving) * 0.10));

    // 2. Approval Rating
    let targetApproval = country.qualityOfLiving;
    const avgTax = (country.incomeTaxRate + country.corporateTaxRate + country.vatRate) / 3.0;
    if (avgTax > 0.25) {
      targetApproval -= (avgTax - 0.25) * 40.0;
    }

    for (const m of Object.values(country.ministries)) {
      if (m.funding === FundingLevel.Zero) {
        targetApproval -= 4.0;
      }
    }

    const ideologyDef = Ideology.get(country.ideology);
    targetApproval += ideologyDef.approvalModifier;

    country.approvalRating = Math.max(0.0, Math.min(100.0, country.approvalRating + (targetApproval - country.approvalRating) * 0.10));

    // 3. National Stability
    let targetStability = 50.0 + (country.approvalRating - 50.0) * 0.5;
    if (country.corruptionIndex > 30.0) {
      targetStability -= (country.corruptionIndex - 30.0) * 0.3;
    }

    const intAff = country.ministries[MinistryType.InternalAffairs];
    const intAffairsEff = this.getMinistryEfficiency(intAff);
    targetStability += (intAffairsEff - 0.50) * 20.0;
    targetStability += ideologyDef.stabilityModifier;

    country.stability = Math.max(0.0, Math.min(100.0, country.stability + (targetStability - country.stability) * 0.10));

    // 4. Corruption drift
    const just = country.ministries[MinistryType.Justice];
    const justEff = this.getMinistryEfficiency(just);
    const netCorruptionDelta = 0.15 - (justEff * 0.15 + intAffairsEff * 0.10);
    country.corruptionIndex = Math.max(0.0, Math.min(100.0, country.corruptionIndex + netCorruptionDelta));
  }

  processDomesticEvents(newDate: DateOnly): void {
    const keys = Object.keys(this._session.eventCooldowns);
    for (const k of keys) {
      if (this._session.eventCooldowns[k] > 0) {
        this._session.eventCooldowns[k]--;
        if (this._session.eventCooldowns[k] <= 0) {
          delete this._session.eventCooldowns[k];
        }
      }
    }

    if (this._session.pendingEvent) return;

    const playerCountry = this._session.getPlayerCountry();
    if (!playerCountry) return;

    const catalog = this._session.domesticEventRegistry ?? DomesticEventRegistry.createDefaultCatalog();
    const monthIndex = newDate.year * 12 + newDate.month;
    // Deterministic pseudo-random based on seed & month
    const rand = Math.sin(this._session.seed + monthIndex) * 10000;
    const randomVal = rand - Math.floor(rand);

    for (const evtDef of catalog.events) {
      const cd = this._session.eventCooldowns[evtDef.id] ?? 0;
      if (evtDef.canTrigger(playerCountry, cd)) {
        if (randomVal < evtDef.randomWeight) {
          const prompt = new ActiveEventPrompt();
          prompt.eventId = evtDef.id;
          prompt.countryId = playerCountry.id;
          prompt.date = newDate;
          prompt.title = evtDef.title;
          prompt.description = evtDef.description;
          prompt.category = (evtDef.category as EventCategory) || EventCategory.Emergency;
          prompt.choices = [...evtDef.choices];

          this._session.pendingEvent = prompt;

          this._session.addEvent({
            id: Math.random().toString(36).substring(2, 11),
            date: newDate,
            title: `EMERGENCY: ${evtDef.title}`,
            message: evtDef.description,
            category: EventCategory.Emergency,
            severity: EventSeverity.Critical,
            actorCountryId: playerCountry.id
          });

          break; // Max 1 new emergency per month
        }
      }
    }
  }

  evaluateTasks(): void {
    const playerCountry = this._session.getPlayerCountry();
    if (!playerCountry) return;

    for (const task of Object.values(this._session.tasks)) {
      if (task.status === GameTaskStatus.Completed || task.status === GameTaskStatus.Inactive) {
        continue;
      }

      let stateChanged = false;
      for (const subTask of task.subTasks) {
        let currentVal = subTask.currentCount;

        if (subTask.id.startsWith('ST_')) {
          let facilityKey = '';
          switch (subTask.id) {
            case 'ST_POWER':
              facilityKey = 'PowerPlants';
              break;
            case 'ST_FARMS':
              facilityKey = 'Farms';
              break;
            case 'ST_MINES':
              facilityKey = 'Mines';
              break;
            case 'ST_FACTORIES':
              facilityKey = 'Factories';
              break;
            case 'ST_HOSPITALS':
              facilityKey = 'Hospitals';
              break;
            case 'ST_SCHOOLS':
              facilityKey = 'Schools';
              break;
            case 'ST_BARRACKS':
              facilityKey = 'Barracks';
              break;
          }

          if (facilityKey && playerCountry.facilities[facilityKey] != null) {
            currentVal = playerCountry.facilities[facilityKey];
          } else if (subTask.id === 'ST_APPROVAL_70') {
            currentVal = Math.floor(playerCountry.approvalRating);
          } else if (subTask.id === 'ST_STABILITY_75') {
            currentVal = Math.floor(playerCountry.stability);
          } else if (subTask.id === 'ST_CORRUPTION_20') {
            currentVal = Math.max(0, Math.floor(100.0 - playerCountry.corruptionIndex));
          }
        }

        if (subTask.currentCount !== currentVal) {
          subTask.currentCount = currentVal;
          stateChanged = true;
        }
      }

      const isComplete = typeof task.checkCompletion === 'function'
        ? task.checkCompletion()
        : Boolean(task.subTasks && task.subTasks.length > 0 && task.subTasks.every((st: any) => st.currentCount >= st.targetCount));

      if (isComplete && stateChanged) {
        this._session.addEvent({
          id: Math.random().toString(36).substring(2, 11),
          date: this._session.clock.currentDate,
          title: 'Presidential Task Completed',
          message: `Goal reached: '${task.title}'! Reward ready to claim in Presidential Objectives.`,
          category: EventCategory.Tasks,
          severity: EventSeverity.Info,
          actorCountryId: playerCountry.id
        });

        // Unlock dependent tasks
        for (const otherTask of Object.values(this._session.tasks)) {
          if (otherTask.prerequisiteTaskId === task.id && otherTask.status === GameTaskStatus.Inactive) {
            otherTask.status = GameTaskStatus.Active;
            this._session.addEvent({
              id: Math.random().toString(36).substring(2, 11),
              date: this._session.clock.currentDate,
              title: 'New Objective Available',
              message: `New task unlocked: '${otherTask.title}'. Check Presidential Objectives for briefing.`,
              category: EventCategory.Tasks,
              severity: EventSeverity.Info,
              actorCountryId: playerCountry.id
            });
          }
        }
      }
    }
  }

  processWorldEvents(newDate: DateOnly): void {
    const playerCountry = this._session.getPlayerCountry();
    if (!playerCountry) return;

    const catalog = this._session.worldEventRegistry ?? WorldEventRegistry.createDefaultCatalog();
    const monthIndex = newDate.year * 12 + newDate.month;
    const rand = Math.sin(this._session.seed + monthIndex * 37 + 11) * 10000;
    const randomVal = rand - Math.floor(rand);

    for (const evtDef of catalog.events) {
      const cd = this._session.eventCooldowns[evtDef.id] ?? 0;
      if (cd <= 0 && randomVal < evtDef.triggerChance) {
        evtDef.applyEffects(this._session, playerCountry);
        this._session.eventCooldowns[evtDef.id] = evtDef.cooldownMonths;

        this._session.addEvent({
          id: Math.random().toString(36).substring(2, 11),
          date: newDate,
          title: evtDef.title,
          message: evtDef.description,
          category: (evtDef.category as EventCategory) || EventCategory.World,
          severity: (evtDef.severity as EventSeverity) || EventSeverity.Info,
          actorCountryId: playerCountry.id,
          actionRoute: evtDef.actionRoute,
          isRead: false
        });

        break;
      }
    }
  }

  evaluateAchievements(): void {
    const playerCountry = this._session.getPlayerCountry();
    if (!playerCountry) return;

    const catalog = this._session.achievementRegistry ?? AchievementRegistry.createDefaultCatalog();
    for (const def of catalog.achievements) {
      let progress = this._session.achievements[def.id];
      if (!progress) {
        progress = new AchievementProgress();
        progress.achievementId = def.id;
        progress.isUnlocked = false;
        progress.progressPercent = 0.0;
        this._session.achievements[def.id] = progress;
      }

      const { isMet, progressPercent } = def.evaluate(playerCountry, this._session);
      progress.progressPercent = progressPercent;

      if (isMet && !progress.isUnlocked) {
        progress.isUnlocked = true;
        progress.unlockedDate = this._session.clock.currentDate;

        this._session.addEvent({
          id: Math.random().toString(36).substring(2, 11),
          date: this._session.clock.currentDate,
          title: `Achievement Unlocked: ${def.title}`,
          message: `Milestone achieved! '${def.title}': ${def.description}. Claim reward in the Achievements menu.`,
          category: EventCategory.Achievements,
          severity: EventSeverity.Info,
          actorCountryId: playerCountry.id,
          actionRoute: 'Achievements',
          isRead: false
        });
      }
    }
  }
}
