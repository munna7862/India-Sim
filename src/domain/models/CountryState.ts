import { FundingLevel, IdeologyType, MinistryType } from './types';
import { ResourceStockpile, DEFAULT_STARTING_STOCKPILE } from './Resources';
import { MinistryState } from './MinistryState';
import { DiplomaticStance } from './Diplomacy';
import { MonthlyBudgetReport } from './MonthlyBudgetReport';

export class CountryState {
  id: string = '';
  name: string = '';
  treasury: number = 0;
  population: number = 0;
  approvalRating: number = 60.0; // 0.0 to 100.0
  qualityOfLiving: number = 60.0; // 0.0 to 100.0
  stability: number = 60.0; // 0.0 to 100.0

  // Tax Sliders (0.0 to 1.0)
  incomeTaxRate: number = 0.20;
  corporateTaxRate: number = 0.25;
  vatRate: number = 0.15;

  // Government & Ideology
  ideology: IdeologyType = IdeologyType.ConstitutionalDemocracy;
  ideologyCooldownMonths: number = 0;

  // Cabinet ministries
  ministries: Record<string, MinistryState> = {};

  // Resources
  stockpiles: ResourceStockpile = { ...DEFAULT_STARTING_STOCKPILE };

  // National Indicators & Macroeconomics
  debt: number = 0;
  creditRating: string = 'AAA';
  corruptionIndex: number = 25.0; // 0.0 (Clean) to 100.0 (Severe Corruption)
  inflationRate: number = 0.05; // 5.0%

  // Active Economic Policies
  activePolicies: string[] = [];

  // Internal Security & Espionage
  presidentialProtectionLevel: FundingLevel = FundingLevel.Medium;

  // Military Inventory (Unit ID -> Count)
  militaryUnits: Record<string, number> = {};

  // Fiscal Ledger & Trend History
  budgetHistory: MonthlyBudgetReport[] = [];
  lastBudgetReport: MonthlyBudgetReport | null = null;

  // Foreign Relations
  diplomaticRelations: Record<string, DiplomaticStance> = {};

  // Facility counts
  facilities: Record<string, number> = {
    Farms: 50,
    Mines: 25,
    PowerPlants: 30,
    Factories: 40,
    Hospitals: 20,
    Schools: 30,
    Barracks: 15,
    OilWells: 20,
    UraniumMines: 5
  };

  calculateMonthlyGdp(): number {
    // Base monthly output: $100/person, modified by factories and quality of living
    const baseGdp = this.population * 100;
    const factories = this.facilities['Factories'] ?? 10;
    const industrialMultiplier = 1.0 + (factories / 200);
    const livingMultiplier = 0.8 + (this.qualityOfLiving / 250);
    return baseGdp * industrialMultiplier * livingMultiplier;
  }

  calculateCreditRating(): string {
    const annualGdp = this.calculateMonthlyGdp() * 12;
    if (annualGdp <= 0) return 'D';

    const debtToGdp = this.debt / annualGdp;

    if (this.treasury < -50_000_000_000) return 'D';
    if (this.treasury < -10_000_000_000 || debtToGdp >= 1.30) return 'CCC';
    if (debtToGdp >= 1.10) return 'B';
    if (debtToGdp >= 0.90) return 'BB';
    if (debtToGdp >= 0.70) return 'BBB';
    if (debtToGdp >= 0.50) return 'A';
    if (debtToGdp >= 0.30) return 'AA';
    return 'AAA';
  }

  getMonthlyInterestRate(): number {
    switch (this.creditRating) {
      case 'AAA':
        return 0.0050; // 6.0% annual
      case 'AA':
        return 0.0060;  // 7.2% annual
      case 'A':
        return 0.0075;   // 9.0% annual
      case 'BBB':
        return 0.0090; // 10.8% annual
      case 'BB':
        return 0.0120;  // 14.4% annual
      case 'B':
        return 0.0150;   // 18.0% annual
      case 'CCC':
        return 0.0200; // 24.0% annual
      default:
        return 0.0300; // 36.0% annual for Default / D
    }
  }

  getRelationsWith(otherCountryId: string): DiplomaticStance {
    if (!this.diplomaticRelations[otherCountryId]) {
      this.diplomaticRelations[otherCountryId] = new DiplomaticStance();
    }
    return this.diplomaticRelations[otherCountryId];
  }

  getTotalMilitaryUnits(): number {
    return Object.values(this.militaryUnits).reduce((sum, count) => sum + count, 0);
  }

  calculateTotalCombatPower(registry?: any): number {
    let total = 0;
    for (const [unitId, count] of Object.entries(this.militaryUnits)) {
      const def = registry?.get?.(unitId);
      if (def) {
        total += def.combatPower * count;
      }
    }
    return total;
  }

  calculateTotalDefensePower(registry?: any): number {
    let total = 0;
    for (const [unitId, count] of Object.entries(this.militaryUnits)) {
      const def = registry?.get?.(unitId);
      if (def) {
        total += def.defensePower * count;
      }
    }
    return total;
  }

  calculateMonthlyMilitaryUpkeep(registry?: any): number {
    let total = 0;
    for (const [unitId, count] of Object.entries(this.militaryUnits)) {
      const def = registry?.get?.(unitId);
      if (def) {
        total += def.monthlyUpkeepTreasury * count;
      }
    }
    return total;
  }

  static createIndiaDefault(): CountryState {
    const india = new CountryState();
    india.id = 'IND';
    india.name = 'Republic of India';
    india.treasury = 50_000_000_000; // $50B liquid starting funds
    india.population = 1_400_000_000; // 1.4 Billion
    india.approvalRating = 65.0;
    india.qualityOfLiving = 55.0;
    india.stability = 70.0;
    india.incomeTaxRate = 0.20;
    india.corporateTaxRate = 0.25;
    india.vatRate = 0.15;
    india.presidentialProtectionLevel = FundingLevel.Medium;
    india.stockpiles = { ...DEFAULT_STARTING_STOCKPILE };
    india.militaryUnits = {
      infantry_division: 250,
      main_battle_tank: 50,
      artillery_corps: 35,
      mechanized_infantry: 30,
      multirole_fighter: 40,
      attack_helicopter: 20,
      strategic_bomber: 5,
      air_defense_system: 20,
      recon_drone_fleet: 25,
      guided_missile_destroyer: 10,
      attack_submarine: 6,
      aircraft_carrier: 2
    };

    for (const ministry of Object.values(MinistryType)) {
      india.ministries[ministry] = new MinistryState(ministry, null, FundingLevel.Medium);
    }

    return india;
  }
}
