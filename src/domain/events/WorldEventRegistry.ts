import { WorldEventDefinition } from './WorldEvent';

export class WorldEventRegistry {
  private _events: Map<string, WorldEventDefinition> = new Map();

  get events(): WorldEventDefinition[] {
    return Array.from(this._events.values());
  }

  register(definition: WorldEventDefinition): void {
    if (!definition || !definition.id) {
      throw new Error('WorldEvent Id cannot be null or whitespace.');
    }
    this._events.set(definition.id.toLowerCase(), definition);
  }

  tryGet(eventId: string): WorldEventDefinition | undefined {
    return this._events.get(eventId.toLowerCase());
  }

  loadFromJson(json: string | { events?: WorldEventDefinition[] } | WorldEventDefinition[]): void {
    let list: WorldEventDefinition[] | undefined;
    if (typeof json === 'string') {
      const parsed = JSON.parse(json);
      list = Array.isArray(parsed) ? parsed : parsed.events;
    } else if (Array.isArray(json)) {
      list = json;
    } else if (json.events) {
      list = json.events;
    }

    if (list) {
      for (const item of list) {
        const def = Object.assign(new WorldEventDefinition(), item);
        this.register(def);
      }
    }
  }

  static createDefaultCatalog(): WorldEventRegistry {
    const registry = new WorldEventRegistry();

    const defaults: Partial<WorldEventDefinition>[] = [
      {
        id: 'WEV_GLOBAL_TRADE_BOOM',
        title: 'Global Trade Expansion',
        description: 'International shipping demand surges across major sea lanes, increasing global commercial revenues and providing a stimulus to export nations.',
        category: 'Economy',
        severity: 'Info',
        triggerChance: 0.25,
        cooldownMonths: 12,
        treasuryImpact: 1_500_000_000,
        stabilityImpact: 2.0,
        approvalImpact: 3.0,
        relationsImpact: 5.0,
        actionRoute: 'EconomyAndBudget'
      },
      {
        id: 'WEV_ENERGY_CRISIS',
        title: 'Global Energy Disruption',
        description: 'Volatility in international petroleum markets triggers price spikes, testing national reserve stability and industrial power supply.',
        category: 'Economy',
        severity: 'Warning',
        triggerChance: 0.20,
        cooldownMonths: 18,
        treasuryImpact: -1_000_000_000,
        stabilityImpact: -3.0,
        approvalImpact: -2.0,
        relationsImpact: 0.0,
        actionRoute: 'EconomyAndBudget'
      },
      {
        id: 'WEV_UN_PEACE_SUMMIT',
        title: 'Geneva World Peace Accord',
        description: 'Delegates at the United Nations pass a consensus resolution affirming sovereign non-aggression, fostering international goodwill and diplomatic thaw.',
        category: 'Diplomacy',
        severity: 'Info',
        triggerChance: 0.20,
        cooldownMonths: 15,
        treasuryImpact: 0,
        stabilityImpact: 4.0,
        approvalImpact: 2.0,
        relationsImpact: 8.0,
        actionRoute: 'DiplomacyAndTreaties'
      },
      {
        id: 'WEV_REGIONAL_TENSION',
        title: 'Cross-Border Military Drills',
        description: 'Heightened maneuvers along contested strategic frontiers cause neighboring armed forces to elevate readiness alerts.',
        category: 'Military',
        severity: 'Warning',
        triggerChance: 0.25,
        cooldownMonths: 12,
        treasuryImpact: -500_000_000,
        stabilityImpact: -2.0,
        approvalImpact: 0.0,
        relationsImpact: -6.0,
        actionRoute: 'MilitaryAndDefense'
      },
      {
        id: 'WEV_GLOBAL_PANDEMIC_ALERT',
        title: 'WHO Respiratory Virus Alert',
        description: 'An infectious respiratory cluster reported abroad triggers health monitoring protocols across all entry ports and international airports.',
        category: 'Emergency',
        severity: 'Critical',
        triggerChance: 0.15,
        cooldownMonths: 24,
        treasuryImpact: -2_000_000_000,
        stabilityImpact: -4.0,
        approvalImpact: -3.0,
        relationsImpact: -2.0,
        actionRoute: 'GovernmentCabinet'
      },
      {
        id: 'WEV_SCIENTIFIC_BREAKTHROUGH',
        title: 'Breakthrough in Clean Fusion Energy',
        description: 'An international consortium announces experimental net-positive fusion energy milestones, uplifting public confidence and scientific optimism.',
        category: 'Economy',
        severity: 'Info',
        triggerChance: 0.15,
        cooldownMonths: 20,
        treasuryImpact: 500_000_000,
        stabilityImpact: 3.0,
        approvalImpact: 4.0,
        relationsImpact: 3.0,
        actionRoute: 'EconomyAndBudget'
      }
    ];

    for (const item of defaults) {
      registry.register(Object.assign(new WorldEventDefinition(), item));
    }

    return registry;
  }
}
