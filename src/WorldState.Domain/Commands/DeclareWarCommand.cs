using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class DeclareWarCommand : ICommand
{
    public string AggressorCountryId { get; }
    public string TargetCountryId { get; }

    public DeclareWarCommand(string aggressorCountryId, string targetCountryId)
    {
        AggressorCountryId = aggressorCountryId;
        TargetCountryId = targetCountryId;
    }

    public CommandResult Execute(GameSession session)
    {
        if (string.Equals(AggressorCountryId, TargetCountryId, StringComparison.OrdinalIgnoreCase))
        {
            return CommandResult.Failure("InvalidTarget", "A country cannot declare war upon itself.");
        }

        if (!session.Countries.TryGetValue(AggressorCountryId, out var aggressor))
        {
            return CommandResult.Failure("CountryNotFound", $"Aggressor country '{AggressorCountryId}' not found.");
        }

        if (!session.Countries.TryGetValue(TargetCountryId, out var target))
        {
            return CommandResult.Failure("CountryNotFound", $"Target country '{TargetCountryId}' not found.");
        }

        var aggressorStance = aggressor.GetRelationsWith(TargetCountryId);
        var targetStance = target.GetRelationsWith(AggressorCountryId);

        if (aggressorStance.IsAtWar)
        {
            return CommandResult.Failure("AlreadyAtWar", $"State of war is already active with {target.Name}.");
        }

        if (aggressorStance.HasActiveNonAggressionPact(session.Clock.CurrentDate))
        {
            return CommandResult.Failure(
                "TreatyViolationBlocked",
                $"Cannot declare war! A binding Non-Aggression Pact is in effect until {aggressorStance.NonAggressionPactUntil:dd MMM yyyy}."
            );
        }

        // Establish State of War
        aggressorStance.IsAtWar = true;
        targetStance.IsAtWar = true;

        aggressorStance.Relations = -100.0;
        targetStance.Relations = -100.0;

        // Sever all active commercial trade agreements
        aggressorStance.ActiveTradeContracts.Clear();
        targetStance.ActiveTradeContracts.Clear();

        // Domestic political cost of offensive war
        aggressor.ApprovalRating = Math.Max(0.0, aggressor.ApprovalRating - 10.0);
        aggressor.Stability = Math.Max(0.0, aggressor.Stability - 5.0);

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "State of War Declared!",
            Message: $"{aggressor.Name} declared formal hostilities upon {target.Name}! All bilateral trade contracts have been severed and relations collapsed to -100.",
            Category: EventCategory.Military,
            Severity: EventSeverity.Critical,
            ActorCountryId: AggressorCountryId
        ));

        return CommandResult.Success();
    }
}
