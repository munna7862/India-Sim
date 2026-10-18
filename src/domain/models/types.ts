export enum ClockSpeed {
  Paused = 0,
  Normal = 1,  // 1 in-game day per 1.0 real second
  Fast = 2,    // 1 in-game day per 0.5 real seconds
  Ultra = 3    // 1 in-game day per 0.25 real seconds
}

export enum TaxType {
  IncomeTax = 'IncomeTax',
  CorporateTax = 'CorporateTax',
  Vat = 'Vat'
}

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

export enum IdeologyType {
  ConstitutionalDemocracy = 'ConstitutionalDemocracy',
  DemocraticSocialism = 'DemocraticSocialism',
  AuthoritarianRepublic = 'AuthoritarianRepublic',
  NationalistFederation = 'NationalistFederation',
  Technocracy = 'Technocracy'
}

export enum EventCategory {
  General = 'General',
  Emergency = 'Emergency',
  Economy = 'Economy',
  Military = 'Military',
  Diplomacy = 'Diplomacy',
  Government = 'Government',
  Tasks = 'Tasks',
  Achievements = 'Achievements',
  World = 'World'
}

export enum EventSeverity {
  Info = 'Info',
  Warning = 'Warning',
  Critical = 'Critical',
  Emergency = 'Emergency'
}

export enum CovertOperationType {
  GatherIntelligence = 'GatherIntelligence',
  SabotageInfrastructure = 'SabotageInfrastructure',
  EmbezzleFunds = 'EmbezzleFunds'
}

export enum GameTaskStatus {
  Inactive = 'Inactive',
  Active = 'Active',
  Completed = 'Completed'
}
