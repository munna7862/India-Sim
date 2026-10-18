export enum IdeologyType {
  ConstitutionalDemocracy = 'ConstitutionalDemocracy',
  DemocraticSocialism = 'DemocraticSocialism',
  AuthoritarianRepublic = 'AuthoritarianRepublic',
  NationalistFederation = 'NationalistFederation',
  Technocracy = 'Technocracy'
}

export interface IdeologyDefinition {
  type: IdeologyType;
  displayName: string;
  description: string;
  taxEfficiencyMultiplier: number;
  stabilityModifier: number;
  approvalModifier: number;
  transitionCost: number;
}

export const IDEOLOGY_DEFINITIONS: Record<IdeologyType, IdeologyDefinition> = {
  [IdeologyType.ConstitutionalDemocracy]: {
    type: IdeologyType.ConstitutionalDemocracy,
    displayName: 'Constitutional Democracy',
    description: 'Representative democracy governed by the rule of law. Boosts public approval and maintains balanced institutions.',
    taxEfficiencyMultiplier: 1.0,
    stabilityModifier: 0.0,
    approvalModifier: 5.0,
    transitionCost: 1_000_000_000
  },
  [IdeologyType.DemocraticSocialism]: {
    type: IdeologyType.DemocraticSocialism,
    displayName: 'Democratic Socialism',
    description: 'Universal social welfare and democratic economic planning. Enhances living standards and equality.',
    taxEfficiencyMultiplier: 0.95,
    stabilityModifier: 2.0,
    approvalModifier: 8.0,
    transitionCost: 1_200_000_000
  },
  [IdeologyType.AuthoritarianRepublic]: {
    type: IdeologyType.AuthoritarianRepublic,
    displayName: 'Authoritarian Republic',
    description: 'Centralized executive control with strict internal security. Significantly increases stability at the cost of civil approval.',
    taxEfficiencyMultiplier: 1.05,
    stabilityModifier: 15.0,
    approvalModifier: -10.0,
    transitionCost: 1_500_000_000
  },
  [IdeologyType.NationalistFederation]: {
    type: IdeologyType.NationalistFederation,
    displayName: 'Nationalist Federation',
    description: 'Strong patriotic federal unity and prioritized defense capabilities. Increases stability and national solidarity.',
    taxEfficiencyMultiplier: 1.0,
    stabilityModifier: 8.0,
    approvalModifier: 2.0,
    transitionCost: 1_000_000_000
  },
  [IdeologyType.Technocracy]: {
    type: IdeologyType.Technocracy,
    displayName: 'Technocracy',
    description: 'Data-driven governance by specialized scientific and industrial experts. Maximizes economic and industrial efficiency.',
    taxEfficiencyMultiplier: 1.10,
    stabilityModifier: 5.0,
    approvalModifier: 0.0,
    transitionCost: 1_800_000_000
  }
};

export class Ideology {
  static get(type: IdeologyType): IdeologyDefinition {
    return IDEOLOGY_DEFINITIONS[type] || IDEOLOGY_DEFINITIONS[IdeologyType.ConstitutionalDemocracy];
  }
}
