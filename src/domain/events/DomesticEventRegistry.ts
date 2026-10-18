import { DomesticEventDefinition, EventChoice } from './DomesticEvent';

export class DomesticEventRegistry {
  private _events: Map<string, DomesticEventDefinition> = new Map();

  get events(): DomesticEventDefinition[] {
    return Array.from(this._events.values());
  }

  register(definition: DomesticEventDefinition): void {
    if (!definition || !definition.id) {
      throw new Error('Event ID cannot be null or empty.');
    }
    this._events.set(definition.id, definition);
  }

  tryGet(eventId: string): DomesticEventDefinition | undefined {
    return this._events.get(eventId);
  }

  loadFromJson(json: string | DomesticEventDefinition[]): void {
    const list = typeof json === 'string' ? JSON.parse(json) : json;
    if (Array.isArray(list)) {
      for (const def of list) {
        const item = Object.assign(new DomesticEventDefinition(), def);
        this.register(item);
      }
    }
  }

  static createDefaultCatalog(): DomesticEventRegistry {
    const registry = new DomesticEventRegistry();

    const flood = new DomesticEventDefinition();
    flood.id = 'EVT_FLOOD_RELIEF';
    flood.title = 'Monsoon Floods in Agricultural Belt';
    flood.description = 'Unprecedented monsoon rains have flooded major river basins, disrupting farm harvests and displacing rural populations.';
    flood.category = 'Emergency';
    flood.randomWeight = 0.35;
    flood.cooldownMonths = 12;
    flood.choices = [
      { id: 'CH_FLOOD_RELIEF_FULL', title: 'Comprehensive Disaster Relief Package', description: 'Disburse emergency compensation to farmers and repair destroyed irrigation embankments immediately.', treasuryCost: 500_000_000, approvalDelta: 6.0, stabilityDelta: 3.0, corruptionDelta: 0.0 },
      { id: 'CH_FLOOD_RELIEF_PARTIAL', title: 'Targeted Emergency Aid Only', description: 'Deliver grain rations and basic shelter without long-term crop compensation.', treasuryCost: 150_000_000, approvalDelta: 1.0, stabilityDelta: 0.0, corruptionDelta: 0.0 },
      { id: 'CH_FLOOD_RELIEF_IGNORE', title: 'Rely on Local State Administration', description: 'Avoid federal treasury expenditure and instruct regional authorities to manage with local disaster funds.', treasuryCost: 0, approvalDelta: -7.0, stabilityDelta: -4.0, corruptionDelta: 0.0 }
    ];
    registry.register(flood);

    const doctors = new DomesticEventDefinition();
    doctors.id = 'EVT_DOCTORS_STRIKE';
    doctors.title = 'Medical Professionals Union Strike';
    doctors.description = 'Public hospital doctors and healthcare staff have organized strikes demanding modernized facilities and salary parity.';
    doctors.category = 'SocialUnrest';
    doctors.maxApproval = 60.0;
    doctors.randomWeight = 0.30;
    doctors.cooldownMonths = 12;
    doctors.choices = [
      { id: 'CH_DOC_ACCEDE', title: 'Upgrade Medical Salaries and Clinic Equipment', description: 'Fulfill core demands, renovate emergency rooms and expand nurse allowances.', treasuryCost: 300_000_000, approvalDelta: 5.0, stabilityDelta: 4.0, corruptionDelta: 0.0 },
      { id: 'CH_DOC_ENFORCE', title: 'Enact Essential Services Maintenance Act', description: 'Order immediate return to work under penalty of legal suspension; refuse budget hikes.', treasuryCost: 0, approvalDelta: -8.0, stabilityDelta: 2.0, corruptionDelta: 0.0 }
    ];
    registry.register(doctors);

    const scandal = new DomesticEventDefinition();
    scandal.id = 'EVT_CORRUPTION_SCANDAL';
    scandal.title = 'Procurement Bribery Scandal Exposed';
    scandal.description = 'Investigative journalists have leaked financial records implicating senior procurement bureaucrats in kickback schemes.';
    scandal.category = 'Government';
    scandal.minCorruption = 25.0;
    scandal.randomWeight = 0.40;
    scandal.cooldownMonths = 10;
    scandal.choices = [
      { id: 'CH_CORR_PROBE', title: 'Order Independent High-Court Judicial Inquiry', description: 'Suspend implicated officials, seize illicit assets, and demonstrate absolute intolerance for graft.', treasuryCost: 200_000_000, approvalDelta: 7.0, stabilityDelta: -2.0, corruptionDelta: -6.0 },
      { id: 'CH_CORR_BURDEN', title: 'Internal Departmental Review', description: 'Handle the matter quietly through administrative transfers without public trials.', treasuryCost: 20_000_000, approvalDelta: -6.0, stabilityDelta: 1.0, corruptionDelta: 2.0 }
    ];
    registry.register(scandal);

    const tech = new DomesticEventDefinition();
    tech.id = 'EVT_TECH_BOOM';
    tech.title = 'Global Semiconductor Investment Consortium';
    tech.description = 'An international alliance of electronics manufacturers offers to construct cutting-edge silicon fabrication clusters.';
    tech.category = 'Economy';
    tech.maxStability = 55.0;
    tech.randomWeight = 0.30;
    tech.cooldownMonths = 18;
    tech.choices = [
      { id: 'CH_TECH_SUBSIDY', title: 'Provide 50% Capital Subsidy & High-Tech Zone', description: 'Guarantee uninterrupted high-voltage power, water rights, and substantial tax holidays.', treasuryCost: 600_000_000, approvalDelta: 5.0, stabilityDelta: 4.0, corruptionDelta: 0.0 },
      { id: 'CH_TECH_PARTNER', title: 'Joint Public-Private Venture Model', description: 'Co-finance the facility in exchange for permanent 30% sovereign equity ownership.', treasuryCost: 350_000_000, approvalDelta: 3.0, stabilityDelta: 2.0, corruptionDelta: 0.0 }
    ];
    registry.register(tech);

    const border = new DomesticEventDefinition();
    border.id = 'EVT_BORDER_TENSION';
    border.title = 'Border Infiltration Intercepted';
    border.description = 'Border security forces have neutralized heavily armed insurgent infiltrators in sensitive frontier valleys.';
    border.category = 'Emergency';
    border.randomWeight = 0.35;
    border.cooldownMonths = 8;
    border.choices = [
      { id: 'CH_BORDER_SURGE', title: 'Deploy Specialized Quick-Reaction Border Brigades', description: 'Fortify surveillance outposts with thermal cameras and deploy mechanized response teams.', treasuryCost: 250_000_000, approvalDelta: 5.0, stabilityDelta: 5.0, corruptionDelta: 0.0 },
      { id: 'CH_BORDER_DIPLOMATIC', title: 'Lodge Formal Diplomatic Protest and Maintain Status Quo', description: 'Summon the neighboring envoy to present evidence without escalating troop deployments.', treasuryCost: 10_000_000, approvalDelta: -3.0, stabilityDelta: 1.0, corruptionDelta: 0.0 }
    ];
    registry.register(border);

    return registry;
  }
}
