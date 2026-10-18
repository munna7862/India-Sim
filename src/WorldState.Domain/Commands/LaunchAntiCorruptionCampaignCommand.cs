using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class LaunchAntiCorruptionCampaignCommand : ICommand
{
    public string CountryId { get; }
    public decimal CampaignCost { get; }

    public LaunchAntiCorruptionCampaignCommand(string countryId, decimal campaignCost = 500_000_000m)
    {
        CountryId = countryId;
        CampaignCost = campaignCost;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found in world state.");
        }

        const string actionKey = "AntiCorruption";
        if (session.Cooldowns.IsOnCooldown(CountryId, actionKey, session.Clock.CurrentDate))
        {
            return CommandResult.Failure("CooldownActive", "An anti-corruption campaign is already in progress or recently completed.");
        }

        if (country.Treasury < CampaignCost)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Treasury ({country.Treasury:C0}) cannot cover anti-corruption campaign budget of {CampaignCost:C0}."
            );
        }

        // Apply mutations
        country.Treasury -= CampaignCost;
        country.CorruptionIndex = Math.Max(0.0, country.CorruptionIndex - 6.0);
        country.ApprovalRating = Math.Min(100.0, country.ApprovalRating + 2.0);
        country.Stability = Math.Min(100.0, country.Stability + 1.0);

        // Cooldown for 180 days (approx 6 months)
        session.Cooldowns.SetCooldown(CountryId, actionKey, session.Clock.CurrentDate.AddDays(180));

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Anti-Corruption Campaign Launched",
            Message: $"{country.Name} initiated nationwide audits and anti-graft enforcement. Corruption reduced to {country.CorruptionIndex:F1}%.",
            Category: EventCategory.Government,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
