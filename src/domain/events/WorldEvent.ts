import { CountryState } from '../models/CountryState';

export class WorldEventDefinition {
  id: string = '';
  title: string = '';
  description: string = '';
  category: string = 'World';
  severity: string = 'Info';
  triggerChance: number = 0.20;
  cooldownMonths: number = 12;
  treasuryImpact: number = 0;
  stabilityImpact: number = 0;
  approvalImpact: number = 0;
  relationsImpact: number = 0;
  actionRoute?: string | null = null;

  applyEffects(_session: any, playerCountry: CountryState): void {
    if (!playerCountry) return;

    if (this.treasuryImpact !== 0) {
      playerCountry.treasury += this.treasuryImpact;
    }

    if (this.stabilityImpact !== 0) {
      playerCountry.stability = Math.max(0.0, Math.min(100.0, playerCountry.stability + this.stabilityImpact));
    }

    if (this.approvalImpact !== 0) {
      playerCountry.approvalRating = Math.max(0.0, Math.min(100.0, playerCountry.approvalRating + this.approvalImpact));
    }

    if (this.relationsImpact !== 0) {
      for (const rel of Object.values(playerCountry.diplomaticRelations)) {
        if (!rel.isAtWar) {
          rel.relations = Math.max(-100.0, Math.min(100.0, rel.relations + this.relationsImpact));
        }
      }
    }
  }
}
