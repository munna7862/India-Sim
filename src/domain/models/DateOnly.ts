export class DateOnly {
  readonly year: number;
  readonly month: number;
  readonly day: number;

  constructor(year: number, month: number, day: number) {
    this.year = year;
    this.month = month;
    this.day = day;
  }

  get dayNumber(): number {
    return Math.floor(Date.UTC(this.year, this.month - 1, this.day) / 86400000);
  }

  static fromString(str: string): DateOnly {
    const parts = str.split('-').map(Number);
    if (parts.length === 3 && !parts.some(isNaN)) {
      return new DateOnly(parts[0], parts[1], parts[2]);
    }
    const d = new Date(str);
    return new DateOnly(d.getUTCFullYear(), d.getUTCMonth() + 1, d.getUTCDate());
  }

  static fromDate(d: Date): DateOnly {
    return new DateOnly(d.getUTCFullYear(), d.getUTCMonth() + 1, d.getUTCDate());
  }

  addDays(days: number): DateOnly {
    const utc = new Date(Date.UTC(this.year, this.month - 1, this.day + days));
    return new DateOnly(utc.getUTCFullYear(), utc.getUTCMonth() + 1, utc.getUTCDate());
  }

  compareTo(other: DateOnly): number {
    if (this.year !== other.year) return this.year - other.year;
    if (this.month !== other.month) return this.month - other.month;
    return this.day - other.day;
  }

  equals(other: DateOnly | null | undefined): boolean {
    if (!other) return false;
    return this.year === other.year && this.month === other.month && this.day === other.day;
  }

  isAfter(other: DateOnly): boolean {
    return this.compareTo(other) > 0;
  }

  isBefore(other: DateOnly): boolean {
    return this.compareTo(other) < 0;
  }

  isSameOrAfter(other: DateOnly): boolean {
    return this.compareTo(other) >= 0;
  }

  isSameOrBefore(other: DateOnly): boolean {
    return this.compareTo(other) <= 0;
  }

  toString(): string {
    const mm = String(this.month).padStart(2, '0');
    const dd = String(this.day).padStart(2, '0');
    return `${this.year}-${mm}-${dd}`;
  }

  toJSON(): string {
    return this.toString();
  }
}
