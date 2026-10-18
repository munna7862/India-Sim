import { DateOnly } from './DateOnly';

export class TimedOperation {
  id: string;
  countryId: string;
  title: string;
  operationType: string; // "Construction", "ArmyDrills", "Recruitment", "Espionage"
  startDate: DateOnly;
  completionDate: DateOnly;
  totalDays: number;
  parameters: Record<string, string>;
  isCompleted: boolean;

  constructor(
    countryId: string = '',
    title: string = '',
    operationType: string = '',
    startDate: DateOnly = new DateOnly(2026, 1, 1),
    completionDate: DateOnly = new DateOnly(2026, 1, 1),
    totalDays: number = 0,
    parameters: Record<string, string> = {},
    isCompleted: boolean = false,
    id?: string
  ) {
    this.id = id || Math.random().toString(36).substring(2, 11);
    this.countryId = countryId;
    this.title = title;
    this.operationType = operationType;
    this.startDate = startDate;
    this.completionDate = completionDate;
    this.totalDays = totalDays;
    this.parameters = parameters;
    this.isCompleted = isCompleted;
  }

  getDaysRemaining(currentDate: DateOnly): number {
    return Math.max(0, this.completionDate.dayNumber - currentDate.dayNumber);
  }

  getProgressRatio(currentDate: DateOnly): number {
    if (this.totalDays <= 0) return 1.0;
    const elapsed = currentDate.dayNumber - this.startDate.dayNumber;
    return Math.max(0.0, Math.min(1.0, elapsed / this.totalDays));
  }
}
