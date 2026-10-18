export enum MinistryType {
  Health = 'Health',
  Education = 'Education',
  Infrastructure = 'Infrastructure',
  Culture = 'Culture',
  Sports = 'Sports',
  Tourism = 'Tourism',
  Justice = 'Justice',
  InternalAffairs = 'InternalAffairs',
  Defense = 'Defense',
  SecurityIntelligence = 'SecurityIntelligence',
  Finance = 'Finance',
  IndustryConstruction = 'IndustryConstruction',
  Environment = 'Environment'
}

export enum FundingLevel {
  Zero = 0,
  Low = 25,
  Medium = 50,
  High = 75,
  Maximum = 100
}

export interface Minister {
  id: string;
  name: string;
  competence: number; // 0.0 to 1.0 multiplier
  monthlySalary: number;
}

export class MinistryState {
  type: MinistryType;
  activeMinister: Minister | null;
  funding: FundingLevel;

  constructor(type: MinistryType, activeMinister: Minister | null = null, funding: FundingLevel = FundingLevel.Medium) {
    this.type = type;
    this.activeMinister = activeMinister;
    this.funding = funding;
  }

  calculateEfficiency(): number {
    const competence = this.activeMinister?.competence ?? 0.70;
    let fundingMultiplier = 1.0;
    switch (this.funding) {
      case FundingLevel.Zero:
        fundingMultiplier = 0.0;
        break;
      case FundingLevel.Low:
        fundingMultiplier = 0.70;
        break;
      case FundingLevel.Medium:
        fundingMultiplier = 1.0;
        break;
      case FundingLevel.High:
        fundingMultiplier = 1.20;
        break;
      case FundingLevel.Maximum:
        fundingMultiplier = 1.35;
        break;
      default:
        fundingMultiplier = 1.0;
    }
    return Math.max(0.0, Math.min(1.0, competence * fundingMultiplier));
  }
}
