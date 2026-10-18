using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class SignPeaceTreatyCommand : ICommand
{
    public const int PeacePactDurationMonths = 24;

    public string InitiatorCountryId { get; }
    public string TargetCountryId { get; }

    public SignPeaceTreatyCommand(string initiatorCountryId, string targetCountryId)
    {
        InitiatorCountryId = initiatorCountryId;
        TargetCountryId = targetCountryId;
    }

    public CommandResult Execute(GameSession session)
    {
        if (string.Equals(InitiatorCountryId, TargetCountryId, StringComparison.OrdinalIgnoreCase))
        {
            return CommandResult.Failure("InvalidTarget", "A country cannot conclude a peace treaty with itself.");
        }

        if (!session.Countries.TryGetValue(InitiatorCountryId, out var initiator))
        {
            return CommandResult.Failure("CountryNotFound", $"Initiator country '{InitiatorCountryId}' not found.");
        }

        if (!session.Countries.TryGetValue(TargetCountryId, out var target))
        {
            return CommandResult.Failure("CountryNotFound", $"Target country '{TargetCountryId}' not found.");
        }

        var initiatorStance = initiator.GetRelationsWith(TargetCountryId);
        var targetStance = target.GetRelationsWith(InitiatorCountryId);

        if (!initiatorStance.IsAtWar)
        {
            return CommandResult.Failure("NotAtWar", $"No active hostilities exist between {initiator.Name} and {target.Name}.");
        }

        // Terminate State of War
        initiatorStance.IsAtWar = false;
        targetStance.IsAtWar = false;

        // Reset relations to Cold Peace (-25.0)
        initiatorStance.Relations = -25.0;
        targetStance.Relations = -25.0;

        // Establish binding 24-month Non-Aggression Pact
        var pactExpiry = session.Clock.CurrentDate.AddMonths(PeacePactDurationMonths);
        initiatorStance.NonAggressionPactUntil = pactExpiry;
        targetStance.NonAggressionPactUntil = pactExpiry;

        // Domestic relief from peace
        initiator.Stability = Math.Min(100.0, initiator.Stability + 5.0);
        initiator.ApprovalRating = Math.Min(100.0, initiator.ApprovalRating + 4.0);

        target.Stability = Math.Min(100.0, target.Stability + 5.0);
        target.ApprovalRating = Math.Min(100.0, target.ApprovalRating + 4.0);

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Peace Treaty Concluded",
            Message: $"{initiator.Name} and {target.Name} concluded a formal armistice and signed a mutual {PeacePactDurationMonths}-month Non-Aggression Pact until {pactExpiry:dd MMM yyyy}.",
            Category: EventCategory.Diplomacy,
            Severity: EventSeverity.Info,
            ActorCountryId: InitiatorCountryId
        ));

        return CommandResult.Success();
    }
}
