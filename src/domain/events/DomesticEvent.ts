import { DateOnly } from '../models/DateOnly';
import { EventCategory } from '../models/types';
import { CountryState } from '../models/CountryState';

export interface EventChoice {
  id: string;
  title: string;
  description: string;
  treasuryCost: number;
  approvalDelta: number;
  stabilityDelta: number;
  corruptionDelta: number;
}

export class DomesticEventDefinition {
  id: string = '';
  title: string = '';
  description: string = '';
  category: string = 'Emergency';
  minCorruption?: number | null = null;
  maxApproval?: number | null = null;
  maxStability?: number | null = null;
  randomWeight: number = 0.30;
  cooldownMonths: number = 12;
  choices: EventChoice[] = [];

  canTrigger(country: CountryState, activeCooldownMonths: number): boolean {
    if (activeCooldownMonths > 0) return false;
    if (this.minCorruption != null && country.corruptionIndex < this.minCorruption) return false;
    if (this.maxApproval != null && country.approvalRating > this.maxApproval) return false;
    if (this.maxStability != null && country.stability > this.maxStability) return false;
    return true;
  }
}

export class ActiveEventPrompt {
  eventId: string = '';
  countryId: string = '';
  date: DateOnly = new DateOnly(2026, 1, 1);
  title: string = '';
  description: string = '';
  category: EventCategory = EventCategory.Emergency;
  choices: EventChoice[] = [];
}
