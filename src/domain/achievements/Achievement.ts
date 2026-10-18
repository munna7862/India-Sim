import { CountryState } from '../models/CountryState';
import { DateOnly } from '../models/DateOnly';

export class AchievementDefinition {
  id: string = '';
  title: string = '';
  description: string = '';
  category: string = 'General';
  conditionType: string = '';
  targetKey?: string | null = null;
  requiredThreshold: number = 0;
  secondaryTargetKey?: string | null = null;
  secondaryThreshold: number = 0;
  rewardTreasury: number = 0;
  rewardPrestigeScore: number = 0;
  iconKey: string = 'trophy';

  evaluate(country: CountryState, session: any): { isMet: boolean; progressPercent: number } {
    if (!country || !session) return { isMet: false, progressPercent: 0 };

    switch (this.conditionType) {
      case 'PolicyCount': {
        const activePolicies = country.activePolicies.length;
        const polProg = Math.max(0.0, Math.min(1.0, activePolicies / this.requiredThreshold));
        return { isMet: activePolicies >= this.requiredThreshold, progressPercent: polProg * 100.0 };
      }

      case 'FacilityCount': {
        const f1 = country.facilities[this.targetKey ?? ''] ?? 0;
        const p1 = Math.max(0.0, Math.min(1.0, f1 / this.requiredThreshold));
        if (this.secondaryTargetKey && this.secondaryThreshold > 0) {
          const f2 = country.facilities[this.secondaryTargetKey] ?? 0;
          const p2 = Math.max(0.0, Math.min(1.0, f2 / this.secondaryThreshold));
          const combined = (p1 + p2) / 2.0;
          return { isMet: f1 >= this.requiredThreshold && f2 >= this.secondaryThreshold, progressPercent: combined * 100.0 };
        }
        return { isMet: f1 >= this.requiredThreshold, progressPercent: p1 * 100.0 };
      }

      case 'PactCount': {
        const currentDate = session.clock.currentDate;
        let pacts = 0;
        for (const r of Object.values(country.diplomaticRelations)) {
          if (r.hasActiveNonAggressionPact(currentDate)) {
            pacts++;
          }
        }
        const pactProg = Math.max(0.0, Math.min(1.0, pacts / this.requiredThreshold));
        return { isMet: pacts >= this.requiredThreshold, progressPercent: pactProg * 100.0 };
      }

      case 'DebtAndTreasury': {
        const debtZero = country.debt <= 0;
        const treasProg = Math.max(0.0, Math.min(1.0, country.treasury / this.requiredThreshold));
        const met = debtZero && country.treasury >= this.requiredThreshold;
        const prog = debtZero ? 50.0 + treasProg * 50.0 : treasProg * 50.0;
        return { isMet: met, progressPercent: Math.max(0.0, Math.min(100.0, prog)) };
      }

      case 'ApprovalAndLiving': {
        const appProg = Math.max(0.0, Math.min(1.0, country.approvalRating / this.requiredThreshold));
        const livProg =
          this.secondaryThreshold > 0
            ? Math.max(0.0, Math.min(1.0, country.qualityOfLiving / this.secondaryThreshold))
            : 1.0;
        const isAppMet =
          country.approvalRating >= this.requiredThreshold &&
          (this.secondaryThreshold <= 0 || country.qualityOfLiving >= this.secondaryThreshold);
        return { isMet: isAppMet, progressPercent: ((appProg + livProg) / 2.0) * 100.0 };
      }

      case 'MilitaryAndDrills': {
        const totalUnits = country.getTotalMilitaryUnits();
        const unitProg = Math.max(0.0, Math.min(1.0, totalUnits / this.requiredThreshold));
        const drillsReady = session.cooldowns.isOnCooldown(country.id, 'ArmyDrills', session.clock.currentDate);
        const milMet = totalUnits >= this.requiredThreshold && drillsReady;
        const milScore = (unitProg * 0.70 + (drillsReady ? 0.30 : 0.0)) * 100.0;
        return { isMet: milMet, progressPercent: Math.max(0.0, Math.min(100.0, milScore)) };
      }

      case 'CorruptionThreshold': {
        // Target: corruption <= RequiredThreshold
        const corruptMet = country.corruptionIndex <= this.requiredThreshold;
        const corruptProg =
          country.corruptionIndex <= this.requiredThreshold
            ? 100.0
            : Math.max(0.0, Math.min(100.0, ((100.0 - country.corruptionIndex) / (100.0 - this.requiredThreshold)) * 100.0));
        return { isMet: corruptMet, progressPercent: corruptProg };
      }

      case 'CompletedTasksCount': {
        const claimedTasks = session.claimedRewardIds.length;
        const taskProg = Math.max(0.0, Math.min(1.0, claimedTasks / this.requiredThreshold));
        return { isMet: claimedTasks >= this.requiredThreshold, progressPercent: taskProg * 100.0 };
      }

      default:
        return { isMet: false, progressPercent: 0.0 };
    }
  }
}

export class AchievementProgress {
  achievementId: string = '';
  isUnlocked: boolean = false;
  unlockedDate: DateOnly | null = null;
  isRewardClaimed: boolean = false;
  claimedDate: DateOnly | null = null;
  progressPercent: number = 0.0;
}
