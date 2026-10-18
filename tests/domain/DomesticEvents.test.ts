import { describe, it, expect } from 'vitest';
import { GameSession } from '../../src/domain/models/GameSession';
import { SimulationEngine } from '../../src/domain/services/SimulationEngine';
import { DomesticEventRegistry } from '../../src/domain/events/DomesticEventRegistry';
import { ResolveEventChoiceCommand } from '../../src/domain/commands/EventTaskCommands';
import { LaunchAntiCorruptionCampaignCommand } from '../../src/domain/commands/GovernmentCommands';
import { MinistryType, FundingLevel, EventCategory } from '../../src/domain/models/types';
import { MinistryState } from '../../src/domain/models/MinistryState';

describe('Domestic Events & Stability System', () => {
  it('DomesticEventRegistry_LoadsEvents_Accurately', () => {
    const registry = DomesticEventRegistry.createDefaultCatalog();
    expect(registry.events.length).toBe(5);

    const floodEvt = registry.tryGet('EVT_FLOOD_RELIEF');
    expect(floodEvt).toBeDefined();
    expect(floodEvt?.category).toBe('Emergency');
    expect(floodEvt?.choices.length).toBe(3);
    expect(floodEvt?.choices[0].treasuryCost).toBe(500_000_000);
  });

  it('SimulationEngine_UpdateNationalIndicators_ComputesQualityOfLivingFromFacilitiesAndFunding', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;
    const startingQol = country.qualityOfLiving;

    // Upgrade hospitals and healthcare funding
    country.facilities['Hospitals'] = 30;
    country.ministries[MinistryType.Health] = new MinistryState(
      MinistryType.Health,
      { id: 'M_EXP', name: 'Top Doctor', competence: 0.95, monthlySalary: 300_000 },
      FundingLevel.Maximum
    );

    engine.updateNationalIndicators(country);
    expect(country.qualityOfLiving).toBeGreaterThanOrEqual(startingQol);
  });

  it('SimulationEngine_UpdateNationalIndicators_PenalizesZeroFundedMinistries', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;

    // Cut health and education funding to zero
    country.ministries[MinistryType.Health] = new MinistryState(MinistryType.Health, null, FundingLevel.Zero);
    country.ministries[MinistryType.Education] = new MinistryState(MinistryType.Education, null, FundingLevel.Zero);

    const startingApproval = country.approvalRating;
    engine.updateNationalIndicators(country);

    expect(country.approvalRating).toBeLessThan(startingApproval);
  });

  it('ResolveEventChoice_ValidChoice_MutatesTreasuryAndIndicators_AndClearsPrompt', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;

    const startingTreasury = country.treasury;
    const startingApproval = country.approvalRating;
    const startingStability = country.stability;

    const catalog = DomesticEventRegistry.createDefaultCatalog();
    const floodEvt = catalog.tryGet('EVT_FLOOD_RELIEF')!;

    session.pendingEvent = {
      eventId: floodEvt.id,
      countryId: country.id,
      date: session.clock.currentDate,
      title: floodEvt.title,
      description: floodEvt.description,
      category: EventCategory.Emergency,
      choices: [...floodEvt.choices],
    };

    const command = new ResolveEventChoiceCommand('IND', 'EVT_FLOOD_RELIEF', 'CH_FLOOD_RELIEF_FULL');
    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(true);
    expect(session.pendingEvent).toBeNull();
    expect(country.treasury).toBe(startingTreasury - 500_000_000);
    expect(country.approvalRating).toBe(startingApproval + 6.0);
    expect(country.stability).toBe(startingStability + 3.0);
    expect(Object.keys(session.eventCooldowns)).toContain('EVT_FLOOD_RELIEF');
    expect(session.events.some((e) => e.category === EventCategory.Emergency && e.title.includes('Resolved'))).toBe(true);
  });

  it('ResolveEventChoice_WithInsufficientFunds_FailsValidationWithoutMutatingState', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;
    country.treasury = 50_000_000; // Only $50M

    const catalog = DomesticEventRegistry.createDefaultCatalog();
    const floodEvt = catalog.tryGet('EVT_FLOOD_RELIEF')!;

    session.pendingEvent = {
      eventId: floodEvt.id,
      countryId: country.id,
      date: session.clock.currentDate,
      title: floodEvt.title,
      description: floodEvt.description,
      category: EventCategory.Emergency,
      choices: [...floodEvt.choices],
    };

    const command = new ResolveEventChoiceCommand('IND', 'EVT_FLOOD_RELIEF', 'CH_FLOOD_RELIEF_FULL'); // Cost: $500M
    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe('InsufficientFunds');
    expect(session.pendingEvent).not.toBeNull();
    expect(country.treasury).toBe(50_000_000);
  });

  it('ResolveEventChoice_WhenNoActivePendingEvent_FailsSafely', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    session.pendingEvent = null;

    const command = new ResolveEventChoiceCommand('IND', 'EVT_FLOOD_RELIEF', 'CH_FLOOD_RELIEF_FULL');
    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe('NoPendingEvent');
  });

  it('LaunchAntiCorruptionCampaign_ReducesCorruption_AndEnforcesCooldown', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;

    country.corruptionIndex = 40.0;
    const startingTreasury = country.treasury;

    const command = new LaunchAntiCorruptionCampaignCommand('IND', 500_000_000);

    // Act 1: Launch campaign
    const firstResult = engine.dispatch(command);

    expect(firstResult.isSuccess).toBe(true);
    expect(country.corruptionIndex).toBe(34.0);
    expect(country.treasury).toBe(startingTreasury - 500_000_000);
    expect(session.events.some((e) => e.category === EventCategory.Government && e.title === 'Anti-Corruption Campaign Launched')).toBe(true);

    // Act 2: Immediate second attempt while on cooldown
    const secondResult = engine.dispatch(command);

    expect(secondResult.isSuccess).toBe(false);
    expect(secondResult.errorCode).toBe('CooldownActive');
    expect(country.corruptionIndex).toBe(34.0);
  });
});
