import { ClockSpeed } from './types';
import { DateOnly } from './DateOnly';

export class SimulationClock {
  currentDate: DateOnly;
  speed: ClockSpeed;
  accumulatorSeconds: number;

  onDayPassed?: (date: DateOnly) => void;
  onMonthPassed?: (date: DateOnly) => void;

  static readonly MaxDaysPerTick = 365;

  constructor(startDate: DateOnly = new DateOnly(2026, 1, 1), speed: ClockSpeed = ClockSpeed.Normal) {
    this.currentDate = startDate;
    this.speed = speed;
    this.accumulatorSeconds = 0.0;
  }

  getSecondsPerDay(): number {
    switch (this.speed) {
      case ClockSpeed.Paused:
        return Infinity;
      case ClockSpeed.Normal:
        return 1.0;
      case ClockSpeed.Fast:
        return 0.5;
      case ClockSpeed.Ultra:
        return 0.25;
      default:
        return 1.0;
    }
  }

  tick(deltaSeconds: number): void {
    if (this.speed === ClockSpeed.Paused || deltaSeconds <= 0 || !isFinite(deltaSeconds)) {
      return;
    }

    this.accumulatorSeconds += deltaSeconds;
    const secondsPerDay = this.getSecondsPerDay();

    let daysAdvanced = 0;
    while (this.accumulatorSeconds >= secondsPerDay && secondsPerDay > 0 && daysAdvanced < SimulationClock.MaxDaysPerTick) {
      this.accumulatorSeconds -= secondsPerDay;
      this.advanceDay();
      daysAdvanced++;
    }

    if (daysAdvanced >= SimulationClock.MaxDaysPerTick) {
      this.accumulatorSeconds = 0.0;
    }
  }

  advanceDay(): void {
    const previousMonth = this.currentDate.month;
    this.currentDate = this.currentDate.addDays(1);
    this.onDayPassed?.(this.currentDate);

    if (this.currentDate.month !== previousMonth) {
      this.onMonthPassed?.(this.currentDate);
    }
  }

  advanceDays(days: number): void {
    for (let i = 0; i < days; i++) {
      this.advanceDay();
    }
  }
}
