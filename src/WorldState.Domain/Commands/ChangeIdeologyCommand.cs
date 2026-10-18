using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class ChangeIdeologyCommand : ICommand
{
    public string CountryId { get; }
    public IdeologyType TargetIdeology { get; }

    public ChangeIdeologyCommand(string countryId, IdeologyType targetIdeology)
    {
        CountryId = countryId;
        TargetIdeology = targetIdeology;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found in world state.");
        }

        if (country.Ideology == TargetIdeology)
        {
            return CommandResult.Failure("AlreadyActiveIdeology", $"{TargetIdeology} is already the state ideology of {country.Name}.");
        }

        if (country.IdeologyCooldownMonths > 0)
        {
            return CommandResult.Failure("IdeologyCooldownActive", $"Ideology cannot be changed during active constitutional reform cooldown ({country.IdeologyCooldownMonths} months remaining).");
        }

        var definition = IdeologyDefinition.Get(TargetIdeology);
        if (country.Treasury < definition.TransitionCost)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Treasury ({country.Treasury:C0}) cannot cover constitutional reform cost of {definition.TransitionCost:C0}."
            );
        }

        // Apply state transition
        country.Treasury -= definition.TransitionCost;
        country.Ideology = TargetIdeology;
        country.IdeologyCooldownMonths = 12; // 1-year cooldown

        // Transition impact: immediate adjustment to stability and approval
        country.Stability = Math.Clamp(country.Stability - 5.0 + definition.StabilityModifier, 0.0, 100.0);
        country.ApprovalRating = Math.Clamp(country.ApprovalRating - 5.0 + definition.ApprovalModifier, 0.0, 100.0);

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "State Ideology Reformed",
            Message: $"{country.Name} has officially adopted '{definition.DisplayName}'. {definition.Description} (Transition cost: ${definition.TransitionCost / 1_000_000m:F0}M).",
            Category: EventCategory.Government,
            Severity: EventSeverity.Warning,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
