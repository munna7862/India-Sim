import { AchievementDefinition } from './Achievement';

export class AchievementRegistry {
  private _achievements: Map<string, AchievementDefinition> = new Map();

  get achievements(): AchievementDefinition[] {
    return Array.from(this._achievements.values());
  }

  getAll(): AchievementDefinition[] {
    return this.achievements;
  }

  register(definition: AchievementDefinition): void {
    if (!definition || !definition.id) {
      throw new Error('Achievement Id cannot be null or whitespace.');
    }
    this._achievements.set(definition.id.toLowerCase(), definition);
  }

  tryGet(achievementId: string): AchievementDefinition | undefined {
    return this._achievements.get(achievementId.toLowerCase());
  }

  get(achievementId: string): AchievementDefinition {
    const ach = this.tryGet(achievementId);
    if (!ach) {
      throw new Error(`Achievement '${achievementId}' not found in registry.`);
    }
    return ach;
  }

  loadFromJson(json: string | { achievements?: AchievementDefinition[] } | AchievementDefinition[]): void {
    let list: AchievementDefinition[] | undefined;
    if (typeof json === 'string') {
      const parsed = JSON.parse(json);
      list = Array.isArray(parsed) ? parsed : parsed.achievements;
    } else if (Array.isArray(json)) {
      list = json;
    } else if (json.achievements) {
      list = json.achievements;
    }

    if (list) {
      for (const item of list) {
        const def = Object.assign(new AchievementDefinition(), item);
        this.register(def);
      }
    }
  }

  static createDefaultCatalog(): AchievementRegistry {
    const registry = new AchievementRegistry();

    const defaults: Partial<AchievementDefinition>[] = [
      {
        id: 'ACH_IND_REFORM',
        title: 'Economic Awakening',
        description: 'Enact at least 2 active national economic development policies.',
        category: 'Economy',
        conditionType: 'PolicyCount',
        requiredThreshold: 2,
        rewardTreasury: 1_000_000_000,
        rewardPrestigeScore: 100,
        iconKey: 'trophy_economy'
      },
      {
        id: 'ACH_TITAN_INDUSTRY',
        title: 'Industrial Powerhouse',
        description: 'Construct at least 40 Factories and 30 Power Plants to empower national production.',
        category: 'Economy',
        conditionType: 'FacilityCount',
        targetKey: 'Factories',
        requiredThreshold: 40,
        secondaryTargetKey: 'PowerPlants',
        secondaryThreshold: 30,
        rewardTreasury: 2_500_000_000,
        rewardPrestigeScore: 250,
        iconKey: 'trophy_industry'
      },
      {
        id: 'ACH_PEACE_MAKER',
        title: 'Architect of Peace',
        description: 'Sign and maintain active Non-Aggression Pacts with at least 2 foreign powers.',
        category: 'Diplomacy',
        conditionType: 'PactCount',
        requiredThreshold: 2,
        rewardTreasury: 1_500_000_000,
        rewardPrestigeScore: 200,
        iconKey: 'trophy_diplomacy'
      },
      {
        id: 'ACH_SOVEREIGN_SOLVENCY',
        title: 'Fiscal Solvency',
        description: 'Eliminate all central bank debt ($0 debt) and maintain over $50 Billion in liquid Treasury reserves.',
        category: 'Economy',
        conditionType: 'DebtAndTreasury',
        requiredThreshold: 50_000_000_000,
        rewardTreasury: 3_000_000_000,
        rewardPrestigeScore: 300,
        iconKey: 'trophy_treasury'
      },
      {
        id: 'ACH_POPULAR_MANDATE',
        title: 'Beloved Republic',
        description: 'Attain an Approval Rating of at least 80% and Quality of Living rating of at least 70%.',
        category: 'Governance',
        conditionType: 'ApprovalAndLiving',
        requiredThreshold: 80,
        secondaryThreshold: 70,
        rewardTreasury: 2_000_000_000,
        rewardPrestigeScore: 250,
        iconKey: 'trophy_approval'
      },
      {
        id: 'ACH_IRON_DETERRENCE',
        title: 'Invulnerable Bastion',
        description: 'Expand national armed forces to at least 100 units across divisions and conduct army drills.',
        category: 'Military',
        conditionType: 'MilitaryAndDrills',
        requiredThreshold: 100,
        rewardTreasury: 2_000_000_000,
        rewardPrestigeScore: 200,
        iconKey: 'trophy_military'
      },
      {
        id: 'ACH_CLEAN_GOVERNANCE',
        title: 'Integrity in Office',
        description: 'Drive systemic corruption down below 15% through robust justice oversight.',
        category: 'Security',
        conditionType: 'CorruptionThreshold',
        requiredThreshold: 15,
        rewardTreasury: 1_500_000_000,
        rewardPrestigeScore: 150,
        iconKey: 'trophy_justice'
      },
      {
        id: 'ACH_PRESIDENTIAL_MASTER',
        title: 'Visionary Leader',
        description: 'Complete and collect the presidential rewards for at least 3 major national objectives.',
        category: 'Governance',
        conditionType: 'CompletedTasksCount',
        requiredThreshold: 3,
        rewardTreasury: 5_000_000_000,
        rewardPrestigeScore: 500,
        iconKey: 'trophy_star'
      }
    ];

    for (const item of defaults) {
      registry.register(Object.assign(new AchievementDefinition(), item));
    }

    return registry;
  }
}
