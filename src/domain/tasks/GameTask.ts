import { DateOnly } from '../models/DateOnly';
import { GameTaskStatus } from '../models/types';

export enum TaskCategory {
  Economic = 'Economic',
  Welfare = 'Welfare',
  Defense = 'Defense',
  Diplomatic = 'Diplomatic'
}

export interface TaskReward {
  treasuryBonus: number;
  approvalBonus: number;
  stabilityBonus: number;
  achievementId?: string | null;
}

export class SubTaskState {
  id: string;
  description: string;
  currentCount: number;
  targetCount: number;

  constructor(id: string = '', description: string = '', targetCount: number = 0, currentCount: number = 0) {
    this.id = id;
    this.description = description;
    this.targetCount = targetCount;
    this.currentCount = currentCount;
  }

  get isCompleted(): boolean {
    return this.currentCount >= this.targetCount;
  }
}

export class GameTask {
  id: string = '';
  title: string = '';
  description: string = '';
  category: TaskCategory = TaskCategory.Economic;
  isPrincipal: boolean = false;
  prerequisiteTaskId?: string | null = null;
  deadline?: DateOnly | null = null;
  status: GameTaskStatus = GameTaskStatus.Active;
  subTasks: SubTaskState[] = [];
  reward: TaskReward = { treasuryBonus: 0, approvalBonus: 0, stabilityBonus: 0 };
  isRewardClaimed: boolean = false;

  get progressPercentage(): number {
    if (this.subTasks.length === 0) {
      return this.status === GameTaskStatus.Completed ? 100.0 : 0.0;
    }
    let sum = 0.0;
    for (const st of this.subTasks) {
      if (st.targetCount <= 0) {
        sum += 100.0;
      } else {
        sum += Math.min(100.0, (st.currentCount / st.targetCount) * 100.0);
      }
    }
    return sum / this.subTasks.length;
  }

  checkCompletion(): boolean {
    if (this.status === GameTaskStatus.Completed) {
      return true;
    }

    if (this.subTasks.length > 0 && this.subTasks.every((st) => st.isCompleted)) {
      this.status = GameTaskStatus.Completed;
      return true;
    }

    return false;
  }
}
