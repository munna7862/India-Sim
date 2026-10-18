export enum MilitaryBranch {
  Land = 'Land',
  Air = 'Air',
  Naval = 'Naval'
}

export interface MilitaryUnitDefinition {
  id: string;
  name: string;
  branch: MilitaryBranch;
  costTreasury: number;
  costIron: number;
  costOil: number;
  costUranium: number;
  monthlyUpkeepTreasury: number;
  combatPower: number;
  defensePower: number;
  requiredBarracks: number;
}

export class MilitaryUnitRegistry {
  private _units: Map<string, MilitaryUnitDefinition> = new Map();

  get units(): Record<string, MilitaryUnitDefinition> {
    const res: Record<string, MilitaryUnitDefinition> = {};
    for (const [k, v] of this._units) {
      res[k] = v;
    }
    return res;
  }

  get count(): number {
    return this._units.size;
  }

  getAll(): MilitaryUnitDefinition[] {
    return Array.from(this._units.values());
  }

  register(def: MilitaryUnitDefinition): void {
    if (!def || !def.id) {
      throw new Error('MilitaryUnitDefinition id cannot be empty');
    }
    this._units.set(def.id.toLowerCase(), def);
  }

  tryGet(id: string): MilitaryUnitDefinition | undefined {
    return this._units.get(id.toLowerCase());
  }

  get(id: string): MilitaryUnitDefinition {
    const unit = this.tryGet(id);
    if (!unit) {
      throw new Error(`Military unit '${id}' not found in registry.`);
    }
    return unit;
  }

  loadFromJson(json: string | MilitaryUnitDefinition[]): void {
    const list = typeof json === 'string' ? JSON.parse(json) : json;
    if (Array.isArray(list)) {
      for (const item of list) {
        this.register(item);
      }
    }
  }

  static createDefaultCatalog(): MilitaryUnitRegistry {
    const registry = new MilitaryUnitRegistry();

    // Land Branch
    registry.register({
      id: 'infantry_division',
      name: 'Infantry Division',
      branch: MilitaryBranch.Land,
      costTreasury: 50_000_000,
      costIron: 200.0,
      costOil: 50.0,
      costUranium: 0.0,
      monthlyUpkeepTreasury: 2_000_000,
      combatPower: 15,
      defensePower: 20,
      requiredBarracks: 5
    });

    registry.register({
      id: 'main_battle_tank',
      name: 'Main Battle Tank Brigade',
      branch: MilitaryBranch.Land,
      costTreasury: 200_000_000,
      costIron: 800.0,
      costOil: 300.0,
      costUranium: 0.0,
      monthlyUpkeepTreasury: 8_000_000,
      combatPower: 45,
      defensePower: 40,
      requiredBarracks: 10
    });

    registry.register({
      id: 'artillery_corps',
      name: 'Artillery Corps',
      branch: MilitaryBranch.Land,
      costTreasury: 120_000_000,
      costIron: 500.0,
      costOil: 100.0,
      costUranium: 0.0,
      monthlyUpkeepTreasury: 5_000_000,
      combatPower: 35,
      defensePower: 25,
      requiredBarracks: 8
    });

    registry.register({
      id: 'mechanized_infantry',
      name: 'Mechanized Armor Division',
      branch: MilitaryBranch.Land,
      costTreasury: 150_000_000,
      costIron: 600.0,
      costOil: 250.0,
      costUranium: 0.0,
      monthlyUpkeepTreasury: 6_000_000,
      combatPower: 32,
      defensePower: 32,
      requiredBarracks: 8
    });

    // Air Branch
    registry.register({
      id: 'multirole_fighter',
      name: 'Multi-Role Fighter Squadron',
      branch: MilitaryBranch.Air,
      costTreasury: 350_000_000,
      costIron: 400.0,
      costOil: 600.0,
      costUranium: 0.0,
      monthlyUpkeepTreasury: 15_000_000,
      combatPower: 65,
      defensePower: 55,
      requiredBarracks: 12
    });

    registry.register({
      id: 'attack_helicopter',
      name: 'Attack Helicopter Wing',
      branch: MilitaryBranch.Air,
      costTreasury: 180_000_000,
      costIron: 300.0,
      costOil: 400.0,
      costUranium: 0.0,
      monthlyUpkeepTreasury: 8_000_000,
      combatPower: 40,
      defensePower: 30,
      requiredBarracks: 10
    });

    registry.register({
      id: 'strategic_bomber',
      name: 'Stealth Strategic Bomber',
      branch: MilitaryBranch.Air,
      costTreasury: 600_000_000,
      costIron: 500.0,
      costOil: 800.0,
      costUranium: 0.0,
      monthlyUpkeepTreasury: 25_000_000,
      combatPower: 95,
      defensePower: 35,
      requiredBarracks: 15
    });

    registry.register({
      id: 'air_defense_system',
      name: 'Surface-to-Air Missile Battery',
      branch: MilitaryBranch.Air,
      costTreasury: 250_000_000,
      costIron: 500.0,
      costOil: 100.0,
      costUranium: 0.0,
      monthlyUpkeepTreasury: 10_000_000,
      combatPower: 25,
      defensePower: 75,
      requiredBarracks: 10
    });

    registry.register({
      id: 'recon_drone_fleet',
      name: 'Armed Recon Drone Fleet',
      branch: MilitaryBranch.Air,
      costTreasury: 100_000_000,
      costIron: 150.0,
      costOil: 150.0,
      costUranium: 0.0,
      monthlyUpkeepTreasury: 4_000_000,
      combatPower: 30,
      defensePower: 15,
      requiredBarracks: 5
    });

    // Naval Branch
    registry.register({
      id: 'guided_missile_destroyer',
      name: 'Guided Missile Destroyer',
      branch: MilitaryBranch.Naval,
      costTreasury: 500_000_000,
      costIron: 1500.0,
      costOil: 800.0,
      costUranium: 0.0,
      monthlyUpkeepTreasury: 20_000_000,
      combatPower: 80,
      defensePower: 75,
      requiredBarracks: 12
    });

    registry.register({
      id: 'attack_submarine',
      name: 'Nuclear Attack Submarine',
      branch: MilitaryBranch.Naval,
      costTreasury: 800_000_000,
      costIron: 2000.0,
      costOil: 200.0,
      costUranium: 100.0,
      monthlyUpkeepTreasury: 30_000_000,
      combatPower: 100,
      defensePower: 85,
      requiredBarracks: 15
    });

    registry.register({
      id: 'aircraft_carrier',
      name: 'Fleet Aircraft Carrier',
      branch: MilitaryBranch.Naval,
      costTreasury: 2_500_000_000,
      costIron: 5000.0,
      costOil: 2000.0,
      costUranium: 200.0,
      monthlyUpkeepTreasury: 80_000_000,
      combatPower: 220,
      defensePower: 180,
      requiredBarracks: 20
    });

    return registry;
  }
}
