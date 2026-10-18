import { DateOnly } from './DateOnly';

export class CooldownTracker {
  // Key: "{CountryId}:{ActionKey}" -> ExpiryDate
  cooldowns: Record<string, DateOnly>;

  constructor(cooldowns: Record<string, DateOnly> = {}) {
    this.cooldowns = cooldowns;
  }

  isOnCooldown(countryId: string, actionKey: string, currentDate: DateOnly): boolean {
    const key = `${countryId}:${actionKey}`;
    const expiry = this.cooldowns[key];
    return !!expiry && currentDate.isBefore(expiry);
  }

  getExpiry(countryId: string, actionKey: string): DateOnly | null {
    const key = `${countryId}:${actionKey}`;
    return this.cooldowns[key] ?? null;
  }

  setCooldown(countryId: string, actionKey: string, expiryDate: DateOnly): void {
    this.cooldowns[`${countryId}:${actionKey}`] = expiryDate;
  }

  clearExpired(currentDate: DateOnly): void {
    for (const key of Object.keys(this.cooldowns)) {
      if (this.cooldowns[key].isSameOrBefore(currentDate)) {
        delete this.cooldowns[key];
      }
    }
  }
}
