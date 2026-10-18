namespace WorldState.Domain.Models;

public class WorldEventDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "World";
    public string Severity { get; set; } = "Info";
    public double TriggerChance { get; set; } = 0.20;
    public int CooldownMonths { get; set; } = 12;
    public decimal TreasuryImpact { get; set; }
    public double StabilityImpact { get; set; }
    public double ApprovalImpact { get; set; }
    public double RelationsImpact { get; set; }
    public string? ActionRoute { get; set; }

    public void ApplyEffects(GameSession session, CountryState playerCountry)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(playerCountry);

        if (TreasuryImpact != 0m)
        {
            playerCountry.Treasury += TreasuryImpact;
        }

        if (StabilityImpact != 0.0)
        {
            playerCountry.Stability = Math.Clamp(playerCountry.Stability + StabilityImpact, 0.0, 100.0);
        }

        if (ApprovalImpact != 0.0)
        {
            playerCountry.ApprovalRating = Math.Clamp(playerCountry.ApprovalRating + ApprovalImpact, 0.0, 100.0);
        }

        if (RelationsImpact != 0.0)
        {
            foreach (var rel in playerCountry.DiplomaticRelations.Values)
            {
                if (!rel.IsAtWar)
                {
                    rel.Relations = Math.Clamp(rel.Relations + RelationsImpact, -100.0, 100.0);
                }
            }
        }
    }
}
