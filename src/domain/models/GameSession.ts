import { SimulationClock } from './SimulationClock';
import { CountryState } from './CountryState';
import { GameEvent } from './GameEvent';
import { TimedOperation } from './TimedOperation';
import { CooldownTracker } from './Cooldowns';
import { ActiveEventPrompt } from '../events/DomesticEvent';
import { GameTask } from '../tasks/GameTask';
import { TaskRegistry } from '../tasks/TaskRegistry';
import { AchievementProgress } from '../achievements/Achievement';
import { AchievementRegistry } from '../achievements/AchievementRegistry';
import { EconomicPolicyRegistry } from '../economy/EconomicPolicy';
import { MilitaryUnitRegistry } from '../military/MilitaryUnitDefinition';
import { WorldEventRegistry } from '../events/WorldEventRegistry';
import { DomesticEventRegistry } from '../events/DomesticEventRegistry';

export class GameSession {
  sessionId: string = Math.random().toString(36).substring(2, 15);
  schemaVersion: number = 1;
  playerCountryId: string = 'IND';
  scenarioId: string = 'modern_2026';
  seed: number = 42;
  difficulty: string = 'Standard';
  clock: SimulationClock = new SimulationClock();
  countries: Record<string, CountryState> = {};
  events: GameEvent[] = [];
  tasks: Record<string, GameTask> = {};
  claimedRewardIds: string[] = [];
  timedOperations: TimedOperation[] = [];
  cooldowns: CooldownTracker = new CooldownTracker();
  pendingEvent: ActiveEventPrompt | null = null;
  eventCooldowns: Record<string, number> = {};

  achievements: Record<string, AchievementProgress> = {};
  claimedAchievementRewardIds: string[] = [];

  policyRegistry?: EconomicPolicyRegistry;
  militaryRegistry?: MilitaryUnitRegistry;
  achievementRegistry?: AchievementRegistry;
  worldEventRegistry?: WorldEventRegistry;
  domesticEventRegistry?: DomesticEventRegistry;

  addEvent(gameEvent: GameEvent): void {
    if (!gameEvent) return;
    this.events.push(gameEvent);
  }

  getPlayerCountry(): CountryState | null {
    return this.countries[this.playerCountryId] ?? null;
  }

  static createNewGame(protagonistCountryId: string = 'IND'): GameSession {
    const session = new GameSession();
    session.playerCountryId = protagonistCountryId;
    session.tasks = TaskRegistry.createDefaultModernTasks();

    const achievementCatalog = AchievementRegistry.createDefaultCatalog();
    session.achievementRegistry = achievementCatalog;
    for (const ach of achievementCatalog.achievements) {
      session.achievements[ach.id] = {
        achievementId: ach.id,
        isUnlocked: false,
        unlockedDate: null,
        isRewardClaimed: false,
        claimedDate: null,
        progressPercent: 0.0
      };
    }

    const playerCountry = CountryState.createIndiaDefault();
    playerCountry.id = protagonistCountryId;
    session.countries[protagonistCountryId] = playerCountry;

    // Foreign countries for interactive diplomacy & sandbox
    const foreignNations = [
      { id: 'USA', name: 'United States of America', treasury: 150_000_000_000, population: 330_000_000, rel: 35.0, barracks: 30 },
      { id: 'RUS', name: 'Russian Federation', treasury: 40_000_000_000, population: 144_000_000, rel: 50.0, barracks: 25 },
      { id: 'CHN', name: "People's Republic of China", treasury: 90_000_000_000, population: 1_410_000_000, rel: -20.0, barracks: 35 },
      { id: 'GBR', name: 'United Kingdom', treasury: 45_000_000_000, population: 67_000_000, rel: 40.0, barracks: 15 },
      { id: 'PAK', name: 'Islamic Republic of Pakistan', treasury: 10_000_000_000, population: 240_000_000, rel: -60.0, barracks: 18 }
    ];

    for (const f of foreignNations) {
      if (f.id.toUpperCase() === protagonistCountryId.toUpperCase()) continue;

      const fState = new CountryState();
      fState.id = f.id;
      fState.name = f.name;
      fState.treasury = f.treasury;
      fState.population = f.population;
      fState.approvalRating = 60.0;
      fState.qualityOfLiving = 60.0;
      fState.stability = 65.0;
      fState.facilities = { Barracks: f.barracks, Factories: 30 };

      const b = f.barracks;
      fState.militaryUnits = {
        infantry_division: b * 15,
        main_battle_tank: b * 3,
        artillery_corps: b * 2,
        multirole_fighter: b * 2,
        guided_missile_destroyer: Math.max(1, Math.floor(b / 2))
      };

      session.countries[f.id] = fState;

      // Setup reciprocal relations
      playerCountry.getRelationsWith(f.id).relations = f.rel;
      fState.getRelationsWith(protagonistCountryId).relations = f.rel;
    }

    return session;
  }
}
