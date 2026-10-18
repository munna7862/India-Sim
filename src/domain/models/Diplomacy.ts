import { DateOnly } from './DateOnly';

export enum DiplomaticGiftTier {
  Small = 'Small',
  Medium = 'Medium',
  Large = 'Large'
}

export interface TradeContract {
  id: string;
  exporterCountryId: string;
  importerCountryId: string;
  resourceType: string;
  monthlyQuantity: number;
  monthlyPayment: number;
  startDate: DateOnly;
  expiryDate?: DateOnly | null;
}

export class DiplomaticStance {
  relations: number; // -100.0 (Hostile/War) to +100.0 (Allied)
  isAtWar: boolean;
  nonAggressionPactUntil: DateOnly | null;
  hasAlliance: boolean;
  activeTradeContracts: TradeContract[];

  constructor(
    relations: number = 0.0,
    isAtWar: boolean = false,
    nonAggressionPactUntil: DateOnly | null = null,
    hasAlliance: boolean = false,
    activeTradeContracts: TradeContract[] = []
  ) {
    this.relations = relations;
    this.isAtWar = isAtWar;
    this.nonAggressionPactUntil = nonAggressionPactUntil;
    this.hasAlliance = hasAlliance;
    this.activeTradeContracts = activeTradeContracts;
  }

  get hasNonAggressionPact(): boolean {
    return this.nonAggressionPactUntil !== null;
  }

  hasActiveNonAggressionPact(currentDate: DateOnly): boolean {
    return this.nonAggressionPactUntil !== null && this.nonAggressionPactUntil.isSameOrAfter(currentDate);
  }

  modifyRelations(delta: number): void {
    this.relations = Math.max(-100.0, Math.min(100.0, this.relations + delta));
  }

  getStatusText(): string {
    if (this.isAtWar) return 'At War';
    if (this.hasAlliance) return 'Allied';
    if (this.relations >= 60.0) return 'Strategic Partner';
    if (this.relations >= 20.0) return 'Friendly';
    if (this.relations >= 0.0) return 'Neutral';
    if (this.relations >= -50.0) return 'Cold';
    return 'Hostile';
  }
}
