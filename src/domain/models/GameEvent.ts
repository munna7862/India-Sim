import { DateOnly } from './DateOnly';
import { EventCategory, EventSeverity } from './types';

export interface GameEvent {
  id: string;
  date: DateOnly;
  title: string;
  message: string;
  category: EventCategory;
  severity: EventSeverity;
  actorCountryId?: string | null;
  targetCountryId?: string | null;
  actionRoute?: string | null;
  isRead?: boolean;
}
