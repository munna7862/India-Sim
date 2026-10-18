import { GameTask, TaskCategory, SubTaskState } from './GameTask';
import { GameTaskStatus } from '../models/types';

export interface SubTaskDefinition {
  id: string;
  description: string;
  targetCount: number;
}

export interface TaskDefinition {
  id: string;
  title: string;
  description: string;
  category: string;
  isPrincipal?: boolean;
  prerequisiteTaskId?: string | null;
  subTasks: SubTaskDefinition[];
  reward: {
    treasuryBonus?: number;
    approvalBonus?: number;
    stabilityBonus?: number;
    achievementId?: string | null;
  };
}

export class TaskRegistry {
  private _definitions: Map<string, TaskDefinition> = new Map();

  get definitions(): TaskDefinition[] {
    return Array.from(this._definitions.values());
  }

  register(definition: TaskDefinition): void {
    if (!definition || !definition.id) {
      throw new Error('Task ID cannot be null or empty.');
    }
    this._definitions.set(definition.id, definition);
  }

  tryGet(taskId: string): TaskDefinition | undefined {
    return this._definitions.get(taskId);
  }

  loadFromJson(json: string | TaskDefinition[]): void {
    const list = typeof json === 'string' ? JSON.parse(json) : json;
    if (Array.isArray(list)) {
      for (const def of list) {
        this.register(def);
      }
    }
  }

  createInitialTaskState(): Record<string, GameTask> {
    const tasks: Record<string, GameTask> = {};
    for (const def of this._definitions.values()) {
      const task = new GameTask();
      task.id = def.id;
      task.title = def.title;
      task.description = def.description;
      task.category = (def.category as TaskCategory) || TaskCategory.Economic;
      task.isPrincipal = !!def.isPrincipal;
      task.prerequisiteTaskId = def.prerequisiteTaskId ?? null;
      task.status = def.prerequisiteTaskId ? GameTaskStatus.Inactive : GameTaskStatus.Active;
      task.reward = {
        treasuryBonus: def.reward.treasuryBonus ?? 0,
        approvalBonus: def.reward.approvalBonus ?? 0,
        stabilityBonus: def.reward.stabilityBonus ?? 0,
        achievementId: def.reward.achievementId ?? null
      };
      task.subTasks = def.subTasks.map(
        (st) => new SubTaskState(st.id, st.description, st.targetCount, 0)
      );
      tasks[def.id] = task;
    }
    return tasks;
  }

  static createDefaultModernTasks(): Record<string, GameTask> {
    const registry = new TaskRegistry();

    registry.register({
      id: 'TSK_INFRA_01',
      title: 'Modernize National Energy Grid',
      description: 'Construct sufficient power generating stations to eliminate regional blackouts and fuel industrial growth.',
      category: 'Economic',
      isPrincipal: true,
      subTasks: [{ id: 'ST_POWER', description: 'Construct 40 Power Plants across industrial corridors', targetCount: 40 }],
      reward: { treasuryBonus: 2_000_000_000, approvalBonus: 4.0, stabilityBonus: 3.0, achievementId: 'ACH_POWER_GRID' }
    });

    registry.register({
      id: 'TSK_AGRI_01',
      title: 'National Food Security Mission',
      description: 'Expand agricultural output and grain storage facilities to guarantee nutritional sovereignty.',
      category: 'Economic',
      isPrincipal: true,
      subTasks: [{ id: 'ST_FARMS', description: 'Operate 60 Commercial Agricultural Farms', targetCount: 60 }],
      reward: { treasuryBonus: 1_500_000_000, approvalBonus: 5.0, stabilityBonus: 2.0, achievementId: 'ACH_BUMPER_HARVEST' }
    });

    registry.register({
      id: 'TSK_IND_01',
      title: 'Industrial Manufacturing Corridor',
      description: 'Commission state-of-the-art heavy fabrication and consumer production factories.',
      category: 'Economic',
      isPrincipal: false,
      prerequisiteTaskId: 'TSK_INFRA_01',
      subTasks: [{ id: 'ST_FACTORIES', description: 'Establish 50 Manufacturing Factories', targetCount: 50 }],
      reward: { treasuryBonus: 3_000_000_000, approvalBonus: 3.0, stabilityBonus: 2.0, achievementId: 'ACH_INDUSTRIAL_GIANT' }
    });

    registry.register({
      id: 'TSK_MIN_01',
      title: 'Strategic Mineral Extraction',
      description: 'Expand domestic mining concessions to ensure raw iron and mineral independence.',
      category: 'Economic',
      isPrincipal: false,
      subTasks: [{ id: 'ST_MINES', description: 'Develop 35 Iron & Mineral Mines', targetCount: 35 }],
      reward: { treasuryBonus: 1_800_000_000, approvalBonus: 2.0, stabilityBonus: 2.0, achievementId: 'ACH_MINERAL_WEALTH' }
    });

    registry.register({
      id: 'TSK_HLTH_01',
      title: 'Universal Public Healthcare Initiative',
      description: 'Commission district hospitals and boost healthcare allocations to elevate citizen life expectancy.',
      category: 'Welfare',
      isPrincipal: true,
      subTasks: [{ id: 'ST_HOSPITALS', description: 'Construct 30 Regional Specialty Hospitals', targetCount: 30 }],
      reward: { treasuryBonus: 1_200_000_000, approvalBonus: 7.0, stabilityBonus: 4.0, achievementId: 'ACH_HEALTH_SHIELD' }
    });

    registry.register({
      id: 'TSK_EDU_01',
      title: 'Foundational Literacy & Technology Schools',
      description: 'Expand primary and vocational academies to build a skilled modern workforce.',
      category: 'Welfare',
      isPrincipal: false,
      subTasks: [{ id: 'ST_SCHOOLS', description: 'Establish 40 Model Public Schools', targetCount: 40 }],
      reward: { treasuryBonus: 1_500_000_000, approvalBonus: 6.0, stabilityBonus: 3.0, achievementId: 'ACH_ENLIGHTENED_NATION' }
    });

    registry.register({
      id: 'TSK_DEF_01',
      title: 'Territorial Sovereign Defense Readiness',
      description: 'Fortify border garrisons and military installations to guarantee deterrence against adversaries.',
      category: 'Defense',
      isPrincipal: false,
      subTasks: [{ id: 'ST_BARRACKS', description: 'Construct and garrison 25 Military Barracks', targetCount: 25 }],
      reward: { treasuryBonus: 2_500_000_000, approvalBonus: 4.0, stabilityBonus: 6.0, achievementId: 'ACH_IRON_FORTRESS' }
    });

    registry.register({
      id: 'TSK_DIP_01',
      title: 'National Prestige & Democratic Mandate',
      description: 'Attain decisive domestic public confidence and unwavering political stability.',
      category: 'Diplomatic',
      isPrincipal: true,
      subTasks: [
        { id: 'ST_APPROVAL_70', description: 'Achieve public approval rating of at least 70%', targetCount: 70 },
        { id: 'ST_STABILITY_75', description: 'Achieve national stability score of at least 75%', targetCount: 75 }
      ],
      reward: { treasuryBonus: 5_000_000_000, approvalBonus: 10.0, stabilityBonus: 10.0, achievementId: 'ACH_SOVEREIGN_REPUBLIC' }
    });

    return registry.createInitialTaskState();
  }
}
