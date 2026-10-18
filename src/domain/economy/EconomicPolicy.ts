export interface EconomicPolicyDefinition {
  id: string;
  name: string;
  description: string;
  category: string;
  upfrontCost: number;
  monthlyUpkeep: number;
  gdpMultiplier?: number;
  farmEfficiencyMultiplier?: number;
  factoryEfficiencyMultiplier?: number;
  energyEfficiencyMultiplier?: number;
  taxEfficiencyModifier?: number;
  approvalModifier?: number;
  livingStandardModifier?: number;
  stabilityModifier?: number;
  requiredTreasury?: number;
}

export class EconomicPolicyRegistry {
  private _policies: Map<string, EconomicPolicyDefinition> = new Map();

  get policies(): EconomicPolicyDefinition[] {
    return Array.from(this._policies.values());
  }

  register(policy: EconomicPolicyDefinition): void {
    if (!policy || !policy.id) {
      throw new Error('Policy ID cannot be empty');
    }
    this._policies.set(policy.id.toLowerCase(), policy);
  }

  tryGet(id: string): EconomicPolicyDefinition | undefined {
    return this._policies.get(id.toLowerCase());
  }

  get(id: string): EconomicPolicyDefinition {
    const policy = this.tryGet(id);
    if (!policy) {
      throw new Error(`Economic policy '${id}' was not found in registry.`);
    }
    return policy;
  }

  loadFromJson(json: string | EconomicPolicyDefinition[]): void {
    const list = typeof json === 'string' ? JSON.parse(json) : json;
    if (Array.isArray(list)) {
      for (const item of list) {
        this.register(item);
      }
    }
  }

  static createDefaultCatalog(): EconomicPolicyRegistry {
    const registry = new EconomicPolicyRegistry();

    registry.register({
      id: 'agricultural_subsidies',
      name: 'Agricultural Subsidies & Modernization',
      description: 'Subsidize agricultural machinery, high-yield seeds, and rural irrigation to secure food independence.',
      category: 'Agriculture',
      upfrontCost: 500_000_000,
      monthlyUpkeep: 200_000_000,
      farmEfficiencyMultiplier: 0.25,
      approvalModifier: 3.0,
      livingStandardModifier: 2.0
    });

    registry.register({
      id: 'industrial_modernization',
      name: 'Industrial Automation & Modernization',
      description: 'Equip domestic factories with advanced robotics and automation, boosting high-value goods manufacturing.',
      category: 'Industry',
      upfrontCost: 1_000_000_000,
      monthlyUpkeep: 400_000_000,
      factoryEfficiencyMultiplier: 0.30,
      gdpMultiplier: 0.05,
      taxEfficiencyModifier: 0.05,
      livingStandardModifier: 2.0
    });

    registry.register({
      id: 'green_energy_transition',
      name: 'Renewable Clean Energy Transition',
      description: 'Deploy grid-scale solar and wind arrays to lower power generation costs and improve urban air quality.',
      category: 'Energy',
      upfrontCost: 1_500_000_000,
      monthlyUpkeep: 350_000_000,
      energyEfficiencyMultiplier: 0.20,
      livingStandardModifier: 4.0,
      approvalModifier: 2.0
    });

    registry.register({
      id: 'fdi_promotion_zones',
      name: 'Special Economic Zones & FDI Incentives',
      description: 'Create tariff-free manufacturing export hubs that attract global corporations and direct investments.',
      category: 'Trade',
      upfrontCost: 800_000_000,
      monthlyUpkeep: 250_000_000,
      gdpMultiplier: 0.08,
      taxEfficiencyModifier: 0.08,
      stabilityModifier: 1.0
    });

    registry.register({
      id: 'national_infrastructure_initiative',
      name: 'National Freight & Logistics Corridors',
      description: 'Construct dedicated high-capacity freight rail and port expressways, accelerating commodity transit.',
      category: 'Infrastructure',
      upfrontCost: 2_000_000_000,
      monthlyUpkeep: 500_000_000,
      gdpMultiplier: 0.06,
      livingStandardModifier: 5.0,
      stabilityModifier: 3.0
    });

    registry.register({
      id: 'fiscal_austerity_program',
      name: 'Fiscal Austerity & Civil Expenditure Cap',
      description: 'Cap administrative operational spending and freeze discretionary allowances to accelerate debt reduction.',
      category: 'Fiscal',
      upfrontCost: 100_000_000,
      monthlyUpkeep: -300_000_000, // Net expenditure saving!
      approvalModifier: -5.0,
      livingStandardModifier: -2.0,
      stabilityModifier: -2.0
    });

    return registry;
  }
}
