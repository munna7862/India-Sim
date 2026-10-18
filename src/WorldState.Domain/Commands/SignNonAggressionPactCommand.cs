using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class SignNonAggressionPactCommand : ICommand
{
    public const decimal TreatyProcessingFee = 50_000_000m; // $50M diplomatic protocol fee
    public const double MinimumRelationsRequired = -20.0;
    public const double RelationsBonus = 10.0;

    public string InitiatorCountryId { get; }
    public string TargetCountryId { get; }
    public int DurationMonths { get; }

    public SignNonAggressionPactCommand(string initiatorCountryId, string targetCountryId, int durationMonths = 24)
    {
        InitiatorCountryId = initiatorCountryId;
        TargetCountryId = targetCountryId;
        DurationMonths = durationMonths > 0 ? durationMonths : 24;
    }

    public CommandResult Execute(GameSession session)
    {
        if (string.Equals(InitiatorCountryId, TargetCountryId, StringComparison.OrdinalIgnoreCase))
        {
            return CommandResult.Failure("InvalidTarget", "A country cannot sign a Non-Aggression Pact with itself.");
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

        if (initiatorStance.IsAtWar)
        {
            return CommandResult.Failure("AtWar", $"Cannot sign a Non-Aggression Pact with '{target.Name}' while actively at war.");
        }

        if (initiatorStance.Relations < MinimumRelationsRequired)
        {
            return CommandResult.Failure(
                "RelationsTooLow",
                $"Relations with {target.Name} ({initiatorStance.Relations:F0}) are too hostile to sign a Non-Aggression Pact. Minimum required: {MinimumRelationsRequired:F0}."
            );
        }

        if (initiatorStance.HasActiveNonAggressionPact(session.Clock.CurrentDate))
        {
            return CommandResult.Failure(
                "ActivePactExists",
                $"A Non-Aggression Pact with {target.Name} is already active until {initiatorStance.NonAggressionPactUntil:dd MMM yyyy}."
            );
        }

        if (initiator.Treasury < TreatyProcessingFee)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Insufficient treasury to ratify diplomatic treaty. Required: ${TreatyProcessingFee / 1_000_000m:F0}M, Available: ${initiator.Treasury / 1_000_000m:F1}M."
            );
        }

        initiator.Treasury -= TreatyProcessingFee;
        var expiryDate = session.Clock.CurrentDate.AddMonths(DurationMonths);

        initiatorStance.NonAggressionPactUntil = expiryDate;
        targetStance.NonAggressionPactUntil = expiryDate;

        initiatorStance.ModifyRelations(RelationsBonus);
        targetStance.ModifyRelations(RelationsBonus);

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Non-Aggression Pact Ratified",
            Message: $"{initiator.Name} and {target.Name} signed a bilateral Non-Aggression Pact valid until {expiryDate:dd MMM yyyy}. Relations strengthened by +{RelationsBonus:F0}.",
            Category: EventCategory.Diplomacy,
            Severity: EventSeverity.Info,
            ActorCountryId: InitiatorCountryId
        ));

        return CommandResult.Success();
    }
}
