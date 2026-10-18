using WorldState.Domain.Economy;
using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class RevokeEconomicPolicyCommand : ICommand
{
    public string CountryId { get; }
    public string PolicyId { get; }

    public RevokeEconomicPolicyCommand(string countryId, string policyId)
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
        var policy = registry.TryGet(PolicyId, out var p) ? p : null;
        string policyName = policy?.Name ?? PolicyId;

        int index = country.ActivePolicies.FindIndex(id => string.Equals(id, PolicyId, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            return CommandResult.Failure("PolicyNotActive", $"Policy '{policyName}' is not currently active for {country.Name}.");
        }

        country.ActivePolicies.RemoveAt(index);

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Economic Policy Revoked",
            Message: $"{country.Name} cancelled policy '{policyName}'.",
            Category: EventCategory.Economy,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
