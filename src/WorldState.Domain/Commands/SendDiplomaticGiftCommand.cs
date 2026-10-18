using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class SendDiplomaticGiftCommand : ICommand
{
    public const int CooldownDays = 30;

    public string SenderCountryId { get; }
    public string TargetCountryId { get; }
    public DiplomaticGiftTier Tier { get; }

    public SendDiplomaticGiftCommand(string senderCountryId, string targetCountryId, DiplomaticGiftTier tier)
    {
        SenderCountryId = senderCountryId;
        TargetCountryId = targetCountryId;
        Tier = tier;
    }

    public static (decimal Cost, double RelationsBonus) GetTierValues(DiplomaticGiftTier tier) => tier switch
    {
        DiplomaticGiftTier.Small => (250_000_000m, 5.0),
        DiplomaticGiftTier.Medium => (1_000_000_000m, 12.0),
        DiplomaticGiftTier.Large => (5_000_000_000m, 25.0),
        _ => (250_000_000m, 5.0)
    };

    public CommandResult Execute(GameSession session)
    {
        if (string.Equals(SenderCountryId, TargetCountryId, StringComparison.OrdinalIgnoreCase))
        {
            return CommandResult.Failure("InvalidTarget", "A country cannot send a diplomatic gift to itself.");
        }

        if (!session.Countries.TryGetValue(SenderCountryId, out var sender))
        {
            return CommandResult.Failure("CountryNotFound", $"Sender country '{SenderCountryId}' not found.");
        }

        if (!session.Countries.TryGetValue(TargetCountryId, out var target))
        {
            return CommandResult.Failure("CountryNotFound", $"Target country '{TargetCountryId}' not found.");
        }

        var senderStance = sender.GetRelationsWith(TargetCountryId);
        if (senderStance.IsAtWar)
        {
            return CommandResult.Failure("AtWar", $"Cannot send diplomatic aid to '{target.Name}' while actively at war.");
        }

        string cooldownKey = $"DiplomaticGift_{TargetCountryId}";
        if (session.Cooldowns.IsOnCooldown(SenderCountryId, cooldownKey, session.Clock.CurrentDate))
        {
            var expiry = session.Cooldowns.GetExpiry(SenderCountryId, cooldownKey);
            return CommandResult.Failure(
                "ActionOnCooldown",
                $"Diplomatic aid to {target.Name} is on cooldown until {expiry:dd MMM yyyy}."
            );
        }

        var (cost, relationsBonus) = GetTierValues(Tier);

        if (sender.Treasury < cost)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Insufficient treasury to send diplomatic aid. Required: ${cost / 1_000_000m:F0}M, Available: ${sender.Treasury / 1_000_000m:F1}M."
            );
        }

        // Apply financial transfer
        sender.Treasury -= cost;
        target.Treasury += cost;

        // Apply bilateral relation improvement
        senderStance.ModifyRelations(relationsBonus);
        var targetStance = target.GetRelationsWith(SenderCountryId);
        targetStance.ModifyRelations(relationsBonus);

        // Record cooldown
        session.Cooldowns.SetCooldown(
            SenderCountryId,
            cooldownKey,
            session.Clock.CurrentDate.AddDays(CooldownDays)
        );

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Diplomatic Aid Dispatched",
            Message: $"{sender.Name} delivered a {Tier} diplomatic grant of ${cost / 1_000_000m:F0}M to {target.Name}. Bilateral relations improved to {senderStance.Relations:+0.0;-0.0;0.0}.",
            Category: EventCategory.Diplomacy,
            Severity: EventSeverity.Info,
            ActorCountryId: SenderCountryId
        ));

        return CommandResult.Success();
    }
}
