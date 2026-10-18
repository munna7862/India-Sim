using WorldState.Domain.Military;
using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public enum CombatOutcome
{
    AttackerVictory,
    Stalemate,
    DefenderVictory
}

public record CombatReport(
    DateOnly Date,
    string AttackerCountryId,
    string DefenderCountryId,
    CombatOutcome Outcome,
    int AttackerCasualties,
    int DefenderCasualties,
    int AttackerCombatPower,
    int DefenderDefensePower,
    string Narrative
);

public class ResolveCombatCommand : ICommand
{
    public const int CombatCooldownDays = 15;

    public string AttackerCountryId { get; }
    public string DefenderCountryId { get; }

    public CombatReport? LastReport { get; private set; }

    public ResolveCombatCommand(string attackerCountryId, string defenderCountryId)
    {
        AttackerCountryId = attackerCountryId;
        DefenderCountryId = defenderCountryId;
    }

    public CommandResult Execute(GameSession session)
    {
        if (string.Equals(AttackerCountryId, DefenderCountryId, StringComparison.OrdinalIgnoreCase))
        {
            return CommandResult.Failure("InvalidTarget", "A country cannot conduct offensive operations against itself.");
        }

        if (!session.Countries.TryGetValue(AttackerCountryId, out var attacker))
        {
            return CommandResult.Failure("CountryNotFound", $"Attacker country '{AttackerCountryId}' not found.");
        }

        if (!session.Countries.TryGetValue(DefenderCountryId, out var defender))
        {
            return CommandResult.Failure("CountryNotFound", $"Defender country '{DefenderCountryId}' not found.");
        }

        var stance = attacker.GetRelationsWith(DefenderCountryId);
        if (!stance.IsAtWar)
        {
            return CommandResult.Failure("NotAtWar", $"Cannot engage in combat with {defender.Name}; not currently in a state of war.");
        }

        string cooldownKey = $"Combat_{DefenderCountryId}";
        if (session.Cooldowns.IsOnCooldown(AttackerCountryId, cooldownKey, session.Clock.CurrentDate))
        {
            var expiry = session.Cooldowns.GetExpiry(AttackerCountryId, cooldownKey);
            return CommandResult.Failure(
                "CombatOnCooldown",
                $"Offensive command staff is reorganizing. Next operation available on {expiry:dd MMM yyyy}."
            );
        }

        int attackerUnits = attacker.GetTotalMilitaryUnits();
        if (attackerUnits <= 0)
        {
            return CommandResult.Failure("NoMilitaryUnits", $"{attacker.Name} has no operational combat units in inventory.");
        }

        var registry = session.MilitaryRegistry ?? MilitaryUnitRegistry.CreateDefaultCatalog();

        // 1. Calculate Powers
        int rawAttackerPower = attacker.CalculateTotalCombatPower(registry);
        bool hasDrillsBuff = session.Cooldowns.IsOnCooldown(AttackerCountryId, "ArmyDrills", session.Clock.CurrentDate);
        double drillMultiplier = hasDrillsBuff ? 1.25 : 1.0;
        int effectiveAttackerPower = (int)(rawAttackerPower * drillMultiplier);

        int rawDefenderPower = defender.CalculateTotalDefensePower(registry);
        // Homeland defense bonus +10%
        int effectiveDefenderPower = (int)(rawDefenderPower * 1.10);

        int totalPower = Math.Max(1, effectiveAttackerPower + effectiveDefenderPower);
        double attackerRatio = (double)effectiveAttackerPower / totalPower;

        // 2. Determine Outcome
        CombatOutcome outcome;
        double attackerLossRate;
        double defenderLossRate;
        string outcomeText;

        if (attackerRatio >= 0.55)
        {
            outcome = CombatOutcome.AttackerVictory;
            attackerLossRate = 0.08;
            defenderLossRate = 0.22;
            outcomeText = "Decisive Offensive Breakthrough";
            attacker.ApprovalRating = Math.Min(100.0, attacker.ApprovalRating + 2.0);
            defender.Stability = Math.Max(0.0, defender.Stability - 4.0);
        }
        else if (attackerRatio >= 0.45)
        {
            outcome = CombatOutcome.Stalemate;
            attackerLossRate = 0.15;
            defenderLossRate = 0.15;
            outcomeText = "Contested Frontline Stalemate";
        }
        else
        {
            outcome = CombatOutcome.DefenderVictory;
            attackerLossRate = 0.22;
            defenderLossRate = 0.08;
            outcomeText = "Offensive Repelled by Defense Fortifications";
            attacker.Stability = Math.Max(0.0, attacker.Stability - 3.0);
        }

        // 3. Deduct Losses
        int attackerLosses = ApplyCasualties(attacker, attackerLossRate);
        int defenderLosses = ApplyCasualties(defender, defenderLossRate);

        session.Cooldowns.SetCooldown(AttackerCountryId, cooldownKey, session.Clock.CurrentDate.AddDays(CombatCooldownDays));

        string narrative = $"{outcomeText}! {attacker.Name} power {effectiveAttackerPower:N0} vs {defender.Name} power {effectiveDefenderPower:N0}. Losses: {attacker.Name} lost {attackerLosses:N0} units, {defender.Name} lost {defenderLosses:N0} units.";

        LastReport = new CombatReport(
            Date: session.Clock.CurrentDate,
            AttackerCountryId: AttackerCountryId,
            DefenderCountryId: DefenderCountryId,
            Outcome: outcome,
            AttackerCasualties: attackerLosses,
            DefenderCasualties: defenderLosses,
            AttackerCombatPower: effectiveAttackerPower,
            DefenderDefensePower: effectiveDefenderPower,
            Narrative: narrative
        );

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: $"Battle Report: {outcomeText}",
            Message: narrative,
            Category: EventCategory.Military,
            Severity: outcome == CombatOutcome.AttackerVictory ? EventSeverity.Info : EventSeverity.Warning,
            ActorCountryId: AttackerCountryId
        ));

        return CommandResult.Success();
    }

    private static int ApplyCasualties(CountryState country, double casualtyRate)
    {
        int totalCasualties = 0;
        foreach (var unitKey in country.MilitaryUnits.Keys.ToList())
        {
            int current = country.MilitaryUnits[unitKey];
            int lost = (int)Math.Ceiling(current * casualtyRate);
            if (lost > current) lost = current;
            country.MilitaryUnits[unitKey] = current - lost;
            totalCasualties += lost;

            if (country.MilitaryUnits[unitKey] <= 0)
            {
                country.MilitaryUnits.Remove(unitKey);
            }
        }
        return totalCasualties;
    }
}
