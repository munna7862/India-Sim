import { GameSession } from '../models/GameSession';
import { CommandResult, ICommand } from './CommandResult';
import { EventCategory, EventSeverity, FundingLevel, IdeologyType, MinistryType } from '../models/types';
import { Minister, MinistryState } from '../models/MinistryState';
import { Ideology } from '../models/Ideology';

export class AppointMinisterCommand implements ICommand {
  countryId: string;
  ministry: MinistryType;
  candidate: Minister;
  hiringFee: number;

  constructor(countryId: string, ministry: MinistryType, candidate: Minister, hiringFee: number = 10_000_000) {
    this.countryId = countryId;
    this.ministry = ministry;
    this.candidate = candidate;
    this.hiringFee = hiringFee;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found in world state.`);
    }

    if (country.treasury < this.hiringFee) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Country treasury ($${(country.treasury / 1_000_000).toFixed(1)}M) cannot cover hiring fee of $${(this.hiringFee / 1_000_000).toFixed(1)}M.`
      );
    }

    let ministryState = country.ministries[this.ministry];
    if (!ministryState) {
      ministryState = new MinistryState(this.ministry, null, FundingLevel.Medium);
    }

    country.treasury -= this.hiringFee;
    ministryState.activeMinister = this.candidate;
    country.ministries[this.ministry] = ministryState;

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Minister Appointed',
      message: `${this.candidate.name} appointed as Minister of ${this.ministry} for ${country.name} (Hiring fee: $${(this.hiringFee / 1_000_000).toFixed(1)}M).`,
      category: EventCategory.Government,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class DismissMinisterCommand implements ICommand {
  countryId: string;
  ministry: MinistryType;

  constructor(countryId: string, ministry: MinistryType) {
    this.countryId = countryId;
    this.ministry = ministry;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found in world state.`);
    }

    const ministryState = country.ministries[this.ministry];
    if (!ministryState || !ministryState.activeMinister) {
      return CommandResult.failure('NoMinisterAppointed', `No minister currently appointed for ${this.ministry}.`);
    }

    const dismissedMinister = ministryState.activeMinister;
    ministryState.activeMinister = null;

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Minister Dismissed',
      message: `${dismissedMinister.name} has been relieved of their duties as Minister of ${this.ministry} in ${country.name}.`,
      category: EventCategory.Government,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class AdjustMinistryFundingCommand implements ICommand {
  countryId: string;
  ministry: MinistryType;
  newFunding: FundingLevel;

  constructor(countryId: string, ministry: MinistryType, newFunding: FundingLevel) {
    this.countryId = countryId;
    this.ministry = ministry;
    this.newFunding = newFunding;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found.`);
    }

    let ministryState = country.ministries[this.ministry];
    if (!ministryState) {
      ministryState = new MinistryState(this.ministry, null, this.newFunding);
    }
    ministryState.funding = this.newFunding;
    country.ministries[this.ministry] = ministryState;

    const severity = this.newFunding === FundingLevel.Zero ? EventSeverity.Warning : EventSeverity.Info;
    const warningText = this.newFunding === FundingLevel.Zero ? ' Warning: Zero funding triggers sector unrest!' : '';

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Ministry Budget Modified',
      message: `${this.ministry} funding set to ${this.newFunding} (${this.newFunding}%).${warningText}`,
      category: EventCategory.Government,
      severity: severity,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class ChangeIdeologyCommand implements ICommand {
  countryId: string;
  targetIdeology: IdeologyType;

  constructor(countryId: string, targetIdeology: IdeologyType) {
    this.countryId = countryId;
    this.targetIdeology = targetIdeology;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found in world state.`);
    }

    if (country.ideology === this.targetIdeology) {
      return CommandResult.failure('AlreadyActiveIdeology', `${this.targetIdeology} is already the state ideology of ${country.name}.`);
    }

    if (country.ideologyCooldownMonths > 0) {
      return CommandResult.failure(
        'IdeologyCooldownActive',
        `Ideology cannot be changed during active constitutional reform cooldown (${country.ideologyCooldownMonths} months remaining).`
      );
    }

    const definition = Ideology.get(this.targetIdeology);
    if (country.treasury < definition.transitionCost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Treasury cannot cover constitutional reform cost of $${(definition.transitionCost / 1_000_000).toFixed(0)}M.`
      );
    }

    country.treasury -= definition.transitionCost;
    country.ideology = this.targetIdeology;
    country.ideologyCooldownMonths = 12;

    country.stability = Math.max(0.0, Math.min(100.0, country.stability - 5.0 + definition.stabilityModifier));
    country.approvalRating = Math.max(0.0, Math.min(100.0, country.approvalRating - 5.0 + definition.approvalModifier));

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'State Ideology Reformed',
      message: `${country.name} has officially adopted '${definition.displayName}'. ${definition.description} (Transition cost: $${(definition.transitionCost / 1_000_000).toFixed(0)}M).`,
      category: EventCategory.Government,
      severity: EventSeverity.Warning,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class FundPresidentialProtectionCommand implements ICommand {
  countryId: string;
  newLevel: FundingLevel;

  constructor(countryId: string, newLevel: FundingLevel) {
    this.countryId = countryId;
    this.newLevel = newLevel;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country '${this.countryId}' not found.`);
    }

    country.presidentialProtectionLevel = this.newLevel;

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Presidential Security Adjusted',
      message: `${country.name} updated the executive protection allocation to ${this.newLevel}.`,
      category: EventCategory.Government,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class LaunchAntiCorruptionCampaignCommand implements ICommand {
  countryId: string;
  campaignCost: number;

  constructor(countryId: string, campaignCost: number = 500_000_000) {
    this.countryId = countryId;
    this.campaignCost = campaignCost;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country ${this.countryId} not found in world state.`);
    }

    const actionKey = 'AntiCorruption';
    if (session.cooldowns.isOnCooldown(this.countryId, actionKey, session.clock.currentDate)) {
      return CommandResult.failure('CooldownActive', 'An anti-corruption campaign is already in progress or recently completed.');
    }

    if (country.treasury < this.campaignCost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Treasury cannot cover anti-corruption campaign budget of $${(this.campaignCost / 1_000_000).toFixed(0)}M.`
      );
    }

    country.treasury -= this.campaignCost;
    country.corruptionIndex = Math.max(0.0, country.corruptionIndex - 6.0);
    country.approvalRating = Math.min(100.0, country.approvalRating + 2.0);
    country.stability = Math.min(100.0, country.stability + 1.0);

    session.cooldowns.setCooldown(this.countryId, actionKey, session.clock.currentDate.addDays(180));

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Anti-Corruption Campaign Launched',
      message: `${country.name} initiated nationwide audits and anti-graft enforcement. Corruption reduced to ${country.corruptionIndex.toFixed(1)}%.`,
      category: EventCategory.Government,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}

export class LaunchCounterTerrorismCommand implements ICommand {
  countryId: string;
  static readonly OperationCost = 200_000_000;
  static readonly CooldownDays = 60;

  constructor(countryId: string) {
    this.countryId = countryId;
  }

  execute(session: GameSession): CommandResult {
    const country = session.countries[this.countryId];
    if (!country) {
      return CommandResult.failure('CountryNotFound', `Country '${this.countryId}' not found.`);
    }

    const cooldownKey = 'CounterTerrorism';
    if (session.cooldowns.isOnCooldown(this.countryId, cooldownKey, session.clock.currentDate)) {
      const expiry = session.cooldowns.getExpiry(this.countryId, cooldownKey);
      return CommandResult.failure(
        'ActionOnCooldown',
        `Counter-terrorism task force is redeploying. Available on ${expiry?.toString()}.`
      );
    }

    if (country.treasury < LaunchCounterTerrorismCommand.OperationCost) {
      return CommandResult.failure(
        'InsufficientFunds',
        `Insufficient treasury to mobilize counter-terrorism commandos. Required: $${(LaunchCounterTerrorismCommand.OperationCost / 1_000_000).toFixed(0)}M, Available: $${(country.treasury / 1_000_000).toFixed(1)}M.`
      );
    }

    country.treasury -= LaunchCounterTerrorismCommand.OperationCost;
    country.stability = Math.min(100.0, country.stability + 4.0);
    country.approvalRating = Math.min(100.0, country.approvalRating + 3.0);
    country.corruptionIndex = Math.max(0.0, country.corruptionIndex - 3.0);

    session.cooldowns.setCooldown(
      this.countryId,
      cooldownKey,
      session.clock.currentDate.addDays(LaunchCounterTerrorismCommand.CooldownDays)
    );

    session.addEvent({
      id: Math.random().toString(36).substring(2, 11),
      date: session.clock.currentDate,
      title: 'Counter-Terrorism Strike Executed',
      message: `${country.name} internal security forces neutralized active terror syndicates. National stability raised to ${country.stability.toFixed(0)}%.`,
      category: EventCategory.Government,
      severity: EventSeverity.Info,
      actorCountryId: this.countryId
    });

    return CommandResult.success();
  }
}
