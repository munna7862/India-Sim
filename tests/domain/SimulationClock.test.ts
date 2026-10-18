import { describe, it, expect } from 'vitest';
import { GameSession } from '../../src/domain/models/GameSession';
import { SimulationEngine } from '../../src/domain/services/SimulationEngine';
import { ClockSpeed } from '../../src/domain/models/types';
import { DateOnly } from '../../src/domain/models/DateOnly';

describe('SimulationClock & Cadence', () => {
  it('Tick_FrameRateIndependence_ProducesIdenticalOutcomes', () => {
    // Session A: 60 ticks of 1.0s (60 real seconds)
    const sessionA = GameSession.createNewGame('IND');
    sessionA.clock.speed = ClockSpeed.Normal; // 1s per day
    const engineA = new SimulationEngine(sessionA);

    // Session B: 3600 ticks of (1/60)s (60 real seconds at 60 FPS)
    const sessionB = GameSession.createNewGame('IND');
    sessionB.clock.speed = ClockSpeed.Normal;
    const engineB = new SimulationEngine(sessionB);

    for (let i = 0; i < 60; i++) {
      engineA.tick(1.0);
    }

    const dt = 1.0 / 60.0;
    for (let i = 0; i < 3600; i++) {
      engineB.tick(dt);
    }

    // January has 31 days, Feb 2026 has 28 days -> 59 days = Mar 1. Day 60 is Mar 2.
    expect(sessionA.clock.currentDate.equals(new DateOnly(2026, 3, 2))).toBe(true);
    expect(sessionA.clock.currentDate.equals(sessionB.clock.currentDate)).toBe(true);

    const indiaA = sessionA.getPlayerCountry()!;
    const indiaB = sessionB.getPlayerCountry()!;

    expect(indiaA.treasury).toBe(indiaB.treasury);
    expect(indiaA.stockpiles.food).toBe(indiaB.stockpiles.food);
    expect(indiaA.stockpiles.energy).toBe(indiaB.stockpiles.energy);
    expect(indiaA.stockpiles.iron).toBe(indiaB.stockpiles.iron);
    expect(sessionA.events.length).toBe(sessionB.events.length);
  });

  it('Tick_SpeedSettings_ScaleCadenceAccurately', () => {
    // 1. Paused
    const sessionPaused = GameSession.createNewGame('IND');
    sessionPaused.clock.speed = ClockSpeed.Paused;
    sessionPaused.clock.tick(10.0);
    expect(sessionPaused.clock.currentDate.equals(new DateOnly(2026, 1, 1))).toBe(true);

    // 2. Normal (1s per day)
    const sessionNormal = GameSession.createNewGame('IND');
    sessionNormal.clock.speed = ClockSpeed.Normal;
    sessionNormal.clock.tick(5.0);
    expect(sessionNormal.clock.currentDate.equals(new DateOnly(2026, 1, 6))).toBe(true);

    // 3. Fast (0.5s per day -> 2 days per second)
    const sessionFast = GameSession.createNewGame('IND');
    sessionFast.clock.speed = ClockSpeed.Fast;
    sessionFast.clock.tick(5.0); // 5s * 2 = 10 days
    expect(sessionFast.clock.currentDate.equals(new DateOnly(2026, 1, 11))).toBe(true);

    // 4. Ultra (0.25s per day -> 4 days per second)
    const sessionUltra = GameSession.createNewGame('IND');
    sessionUltra.clock.speed = ClockSpeed.Ultra;
    sessionUltra.clock.tick(5.0); // 5s * 4 = 20 days
    expect(sessionUltra.clock.currentDate.equals(new DateOnly(2026, 1, 21))).toBe(true);
  });

  it('Tick_FrameStallProtection_ClampsToMaxDaysPerTickWithoutHanging', () => {
    const session = GameSession.createNewGame('IND');
    session.clock.speed = ClockSpeed.Normal;

    // 10,000 real seconds in single tick
    session.clock.tick(10000.0);

    // Advances capped MaxDaysPerTick (365 days) -> 2027-01-01
    expect(session.clock.currentDate.equals(new DateOnly(2027, 1, 1))).toBe(true);
    expect(session.clock.accumulatorSeconds).toBe(0.0);
  });

  it('DateOnly handles leap years and month transitions correctly', () => {
    const d1 = new DateOnly(2028, 2, 28); // 2028 is leap year
    const d2 = d1.addDays(1);
    expect(d2.year).toBe(2028);
    expect(d2.month).toBe(2);
    expect(d2.day).toBe(29);

    const d3 = d2.addDays(1);
    expect(d3.year).toBe(2028);
    expect(d3.month).toBe(3);
    expect(d3.day).toBe(1);

    const dEnd = new DateOnly(2026, 12, 31).addDays(1);
    expect(dEnd.year).toBe(2027);
    expect(dEnd.month).toBe(1);
    expect(dEnd.day).toBe(1);
  });
});
