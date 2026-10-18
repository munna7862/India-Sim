using WorldState.Domain.Military;
using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public enum CovertOperationType
{
    GatherIntelligence,
    SabotageInfrastructure,
    EmbezzleFunds
}

public class DispatchCovertOperationCommand : ICommand
{
    public const decimal OperationCost = 150_000_000m; // $150M intelligence operation fee
    public const int CooldownDays = 30; // 30 days cooldown per target nation

    public string SponsorCountryId { get; }
    public string TargetCountryId { get; }
    public CovertOperationType OperationType { get; }
    public bool? ForceDetectionForTesting { get; set; }

    public DispatchCovertOperationCommand(
        string sponsorCountryId,
        string targetCountryId,
        CovertOperationType operationType)
    {
        SponsorCountryId = sponsorCountryId;
        TargetCountryId = targetCountryId;
        OperationType = operationType;
    }

    public CommandResult Execute(GameSession session)
    {
        if (string.Equals(SponsorCountryId, TargetCountryId, StringComparison.OrdinalIgnoreCase))
        {
            return CommandResult.Failure("InvalidTarget", "A sovereign nation cannot execute covert espionage on itself.");
        }

        if (!session.Countries.TryGetValue(SponsorCountryId, out var sponsor))
        {
            return CommandResult.Failure("CountryNotFound", $"Sponsor country '{SponsorCountryId}' not found.");
        }

        if (!session.Countries.TryGetValue(TargetCountryId, out var target))
        {
            return CommandResult.Failure("CountryNotFound", $"Target country '{TargetCountryId}' not found.");
        }

        string cooldownKey = $"CovertOp_{TargetCountryId}";
        if (session.Cooldowns.IsOnCooldown(SponsorCountryId, cooldownKey, session.Clock.CurrentDate))
        {
            var expiry = session.Cooldowns.GetExpiry(SponsorCountryId, cooldownKey);
            return CommandResult.Failure(
                "ActionOnCooldown",
                $"Intelligence agents in {target.Name} are establishing cover. Available on {expiry:dd MMM yyyy}."
            );
        }

        if (sponsor.Treasury < OperationCost)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Insufficient treasury for covert operation. Required: ${OperationCost / 1_000_000m:F0}M, Available: ${sponsor.Treasury / 1_000_000m:F1}M."
            );
        }

        sponsor.Treasury -= OperationCost;
        session.Cooldowns.SetCooldown(SponsorCountryId, cooldownKey, session.Clock.CurrentDate.AddDays(CooldownDays));

        // Deterministic detection model based on session seed, day, and target stability
        bool isDetected = ForceDetectionForTesting ??
            (((session.Seed + session.Clock.CurrentDate.DayOfYear + (int)target.Stability) % 5) == 0);

        var sponsorStance = sponsor.GetRelationsWith(TargetCountryId);
        var targetStance = target.GetRelationsWith(SponsorCountryId);

        if (isDetected)
        {
            // Diplomatic crisis
            sponsorStance.ModifyRelations(-25.0);
            targetStance.ModifyRelations(-25.0);

            session.AddEvent(new GameEvent(
                Id: Guid.NewGuid().ToString("N"),
                Date: session.Clock.CurrentDate,
                Title: "Covert Operative Compromised!",
                Message: $"An intelligence agent dispatched by {sponsor.Name} was apprehended by counter-intelligence in {target.Name}! Diplomatic relations plummeted by -25.",
                Category: EventCategory.Diplomacy,
                Severity: EventSeverity.Warning,
                ActorCountryId: SponsorCountryId
            ));

            return CommandResult.Success();
        }

        // Successful Operation Outcome
        string outcomeNarrative = OperationType switch
        {
            CovertOperationType.GatherIntelligence => ExecuteGatherIntel(target, session),
            CovertOperationType.SabotageInfrastructure => ExecuteSabotage(target),
            CovertOperationType.EmbezzleFunds => ExecuteEmbezzle(sponsor, target),
            _ => "Operation concluded without incident."
        };

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: $"Covert Operation Succeeded: {OperationType}",
            Message: outcomeNarrative,
            Category: EventCategory.Military,
            Severity: EventSeverity.Info,
            ActorCountryId: SponsorCountryId
        ));

        return CommandResult.Success();
    }

    private static string ExecuteGatherIntel(CountryState target, GameSession session)
    {
        var registry = session.MilitaryRegistry ?? MilitaryUnitRegistry.CreateDefaultCatalog();
        int totalUnits = target.GetTotalMilitaryUnits();
        int combatPower = target.CalculateTotalCombatPower(registry);
        int defensePower = target.CalculateTotalDefensePower(registry);

        return $"Infiltrators mapped {target.Name}'s defense network: {totalUnits:N0} total military units, {combatPower:N0} offensive power, {defensePower:N0} defense power.";
    }

    private static string ExecuteSabotage(CountryState target)
    {
        int factories = target.Facilities.GetValueOrDefault("Factories", 0);
        int plants = target.Facilities.GetValueOrDefault("PowerPlants", 0);

        if (factories > 0) target.Facilities["Factories"] = factories - 1;
        if (plants > 0) target.Facilities["PowerPlants"] = plants - 1;

        target.Stability = Math.Max(0.0, target.Stability - 3.0);
        return $"Covert saboteurs triggered explosive detonations across {target.Name}'s industrial grid, destroying strategic infrastructure.";
    }

    private static string ExecuteEmbezzle(CountryState sponsor, CountryState target)
    {
        decimal stolen = Math.Min(target.Treasury > 0 ? target.Treasury * 0.05m : 0m, 500_000_000m);
        target.Treasury -= stolen;
        sponsor.Treasury += stolen;

        return $"Covert cyber-espionage drained ${stolen / 1_000_000m:F0}M from {target.Name}'s reserve bank accounts directly into sovereign black funds.";
    }
}
