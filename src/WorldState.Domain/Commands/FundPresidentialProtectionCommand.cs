using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class FundPresidentialProtectionCommand : ICommand
{
    public string CountryId { get; }
    public FundingLevel NewLevel { get; }

    public FundPresidentialProtectionCommand(string countryId, FundingLevel newLevel)
    {
        CountryId = countryId;
        NewLevel = newLevel;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country '{CountryId}' not found.");
        }

        country.PresidentialProtectionLevel = NewLevel;

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Presidential Security Adjusted",
            Message: $"{country.Name} updated the executive protection allocation to {NewLevel}.",
            Category: EventCategory.Government,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
