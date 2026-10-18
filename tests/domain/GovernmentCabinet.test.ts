import { describe, it, expect } from 'vitest';
import { GameSession } from '../../src/domain/models/GameSession';
import { SimulationEngine } from '../../src/domain/services/SimulationEngine';
import { MinistryType, FundingLevel, IdeologyType, EventCategory } from '../../src/domain/models/types';
import { Minister, MinistryState } from '../../src/domain/models/MinistryState';
import { IDEOLOGY_DEFINITIONS } from '../../src/domain/models/Ideology';
import { MinisterRosterService } from '../../src/domain/services/MinisterRosterService';
import {
  AppointMinisterCommand,
  DismissMinisterCommand,
  SetMinistryFundingCommand,
  ChangeIdeologyCommand,
} from '../../src/domain/commands/GovernmentCommands';

describe('Government & Cabinet System', () => {
  it('AppointMinister_WithSufficientFunds_SetsMinisterAndDeductsFee', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;
    const startingTreasury = country.treasury;

    const candidate: Minister = {
      id: 'M_001',
      name: 'Dr. Soumya Swaminathan',
      competence: 0.92,
      salary: 350_000,
      monthlySalary: 350_000,
    };
    const command = new AppointMinisterCommand('IND', MinistryType.Health, candidate, 20_000_000);

    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(true);
    expect(country.treasury).toBe(startingTreasury - 20_000_000);
    const health = country.ministries[MinistryType.Health];
    expect(health.activeMinister).not.toBeNull();
    expect(health.activeMinister!.name).toBe('Dr. Soumya Swaminathan');
    expect(health.activeMinister!.competence).toBe(0.92);
    expect(session.events.some((e) => e.category === EventCategory.Government && e.title === 'Minister Appointed')).toBe(true);
  });

  it('AppointMinister_WithInsufficientFunds_FailsValidation', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;
    country.treasury = 5_000_000; // Only $5M

    const candidate: Minister = {
      id: 'M_001',
      name: 'Candidate',
      competence: 0.85,
      salary: 300_000,
      monthlySalary: 300_000,
    };
    const command = new AppointMinisterCommand('IND', MinistryType.Health, candidate, 20_000_000);

    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe('InsufficientFunds');
    expect(country.treasury).toBe(5_000_000);
    expect(country.ministries[MinistryType.Health].activeMinister).toBeNull();
  });

  it('DismissMinister_WhenMinisterAppointed_ClearsMinisterAndLogsEvent', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;

    const candidate: Minister = {
      id: 'M_001',
      name: 'Minister to Dismiss',
      competence: 0.8,
      salary: 250_000,
      monthlySalary: 250_000,
    };
    engine.dispatch(new AppointMinisterCommand('IND', MinistryType.Finance, candidate, 10_000_000));
    expect(country.ministries[MinistryType.Finance].activeMinister).not.toBeNull();

    const dismissCommand = new DismissMinisterCommand('IND', MinistryType.Finance);
    const result = engine.dispatch(dismissCommand);

    expect(result.isSuccess).toBe(true);
    expect(country.ministries[MinistryType.Finance].activeMinister).toBeNull();
    expect(session.events.some((e) => e.category === EventCategory.Government && e.title === 'Minister Dismissed')).toBe(true);
  });

  it('DismissMinister_WhenNoMinister_FailsSafely', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);

    const dismissCommand = new DismissMinisterCommand('IND', MinistryType.Defense);
    const result = engine.dispatch(dismissCommand);

    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe('NoMinisterAppointed');
  });

  it('CalculateMinistryEfficiency_WithAndWithoutMinister_RespectsFundingLevel', () => {
    // Without minister (0.70 baseline) and Medium funding (1.0): 0.70 * 1.0 = 0.70 (70%)
    const vacantMedium = new MinistryState(MinistryType.Education, null, FundingLevel.Medium);
    expect(vacantMedium.calculateEfficiency()).toBeCloseTo(0.7, 2);

    // Without minister and Zero funding: 0.70 * 0.0 = 0.0 (0%)
    const vacantZero = new MinistryState(MinistryType.Education, null, FundingLevel.Zero);
    expect(vacantZero.calculateEfficiency()).toBe(0.0);

    // With expert minister (0.90) and Maximum funding (1.35): 0.90 * 1.35 = 1.0 (clamped 100%)
    const expert: Minister = {
      id: 'EXP',
      name: 'Expert',
      competence: 0.9,
      salary: 400_000,
      monthlySalary: 400_000,
    };
    const fundedExpert = new MinistryState(MinistryType.Education, expert, FundingLevel.Maximum);
    expect(fundedExpert.calculateEfficiency()).toBeCloseTo(1.0, 2);
  });

  it('ChangeIdeology_WithSufficientFunds_UpdatesIdeologyAppliesModifiersAndSetsCooldown', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;
    const startingTreasury = country.treasury;
    const startingStability = country.stability;
    const startingApproval = country.approvalRating;

    const targetIdeology = IdeologyType.AuthoritarianRepublic;
    const def = IDEOLOGY_DEFINITIONS[targetIdeology];
    const command = new ChangeIdeologyCommand('IND', targetIdeology);

    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(true);
    expect(country.ideology).toBe(targetIdeology);
    expect(country.ideologyCooldownMonths).toBe(12);
    expect(country.treasury).toBe(startingTreasury - def.transitionCost);
    // Stability changed by -5.0 + 15.0 = +10.0
    expect(country.stability).toBe(Math.min(100.0, Math.max(0.0, startingStability + 10.0)));
    // Approval changed by -5.0 - 10.0 = -15.0
    expect(country.approvalRating).toBe(Math.min(100.0, Math.max(0.0, startingApproval - 15.0)));
    expect(session.events.some((e) => e.category === EventCategory.Government && e.title === 'State Ideology Reformed')).toBe(true);
  });

  it('ChangeIdeology_WhenCooldownActive_FailsValidation', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;
    country.ideologyCooldownMonths = 6;

    const command = new ChangeIdeologyCommand('IND', IdeologyType.Technocracy);
    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe('IdeologyCooldownActive');
    expect(country.ideology).toBe(IdeologyType.ConstitutionalDemocracy);
  });

  it('ChangeIdeology_WhenSameIdeology_FailsValidation', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);

    const command = new ChangeIdeologyCommand('IND', IdeologyType.ConstitutionalDemocracy);
    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe('AlreadyActiveIdeology');
  });

  it('ChangeIdeology_WithInsufficientFunds_FailsValidation', () => {
    const session = GameSession.createNewGame('IND');
    const engine = new SimulationEngine(session);
    const country = session.getPlayerCountry()!;
    country.treasury = 100_000_000; // Only $100M, transition costs > $1B

    const command = new ChangeIdeologyCommand('IND', IdeologyType.DemocraticSocialism);
    const result = engine.dispatch(command);

    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe('InsufficientFunds');
    expect(country.treasury).toBe(100_000_000);
  });

  it('MinisterRosterService_ReturnsCuratedCandidatesForEachMinistry', () => {
    const ministries = Object.values(MinistryType);
    for (const type of ministries) {
      const candidates = MinisterRosterService.getCandidates(type);
      expect(candidates.length).toBeGreaterThan(0);
      for (const c of candidates) {
        expect(c.ministry).toBe(type);
        expect(c.competence).toBeGreaterThanOrEqual(0.5);
        expect(c.competence).toBeLessThanOrEqual(1.0);
        expect(c.hiringFee).toBeGreaterThan(0);
        expect(c.monthlySalary).toBeGreaterThan(0);
        expect(c.name.trim().length).toBeGreaterThan(0);
      }
    }
  });
});
