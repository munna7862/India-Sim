using WorldState.Domain.Economy;
using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class EnactEconomicPolicyCommand : ICommand
{
    public string CountryId { get; }
    public string PolicyId { get; }

    public EnactEconomicPolicyCommand(string countryId, string policyId)
    {
        CountryId = countryId;
        PolicyId = policyId;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found.");
        }

        var registry = session.PolicyRegistry ?? EconomicPolicyRegistry.CreateDefaultCatalog();
        if (!registry.TryGet(PolicyId, out var policy) || policy == null)
        {
            return CommandResult.Failure("PolicyNotFound", $"Economic policy '{PolicyId}' not found in registry.");
        }

        if (country.ActivePolicies.Contains(PolicyId, StringComparer.OrdinalIgnoreCase))
        {
            return CommandResult.Failure("PolicyAlreadyActive", $"Policy '{policy.Name}' is already active for {country.Name}.");
        }

        if (country.Treasury < policy.UpfrontCost)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Insufficient funds to enact '{policy.Name}'. Required: ${policy.UpfrontCost / 1_000_000m:F1}M, Available: ${country.Treasury / 1_000_000m:F1}M."
            );
        }

        // Deduct upfront cost
        country.Treasury -= policy.UpfrontCost;
        country.ActivePolicies.Add(policy.Id);

        // Apply immediate indicator deltas
        if (policy.ApprovalModifier != 0)
        {
            country.ApprovalRating = Math.Clamp(country.ApprovalRating + policy.ApprovalModifier, 0.0, 100.0);
        }
        if (policy.LivingStandardModifier != 0)
        {
            country.QualityOfLiving = Math.Clamp(country.QualityOfLiving + policy.LivingStandardModifier, 0.0, 100.0);
        }
        if (policy.StabilityModifier != 0)
        {
            country.Stability = Math.Clamp(country.Stability + policy.StabilityModifier, 0.0, 100.0);
        }

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Economic Policy Enacted",
            Message: $"{country.Name} enacted '{policy.Name}' (Upfront: ${policy.UpfrontCost / 1_000_000m:F1}M, Upkeep: ${policy.MonthlyUpkeep / 1_000_000m:F1}M/mo).",
            Category: EventCategory.Economy,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
