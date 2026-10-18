using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class DismissMinisterCommand : ICommand
{
    public string CountryId { get; }
    public MinistryType Ministry { get; }

    public DismissMinisterCommand(string countryId, MinistryType ministry)
    {
        CountryId = countryId;
        Ministry = ministry;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found in world state.");
        }

        if (!country.Ministries.TryGetValue(Ministry, out var ministryState) || ministryState.ActiveMinister == null)
        {
            return CommandResult.Failure("NoMinisterAppointed", $"No minister currently appointed for {Ministry}.");
        }

        var dismissedMinister = ministryState.ActiveMinister;
        country.Ministries[Ministry] = ministryState with { ActiveMinister = null };

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Minister Dismissed",
            Message: $"{dismissedMinister.Name} has been relieved of their duties as Minister of {Ministry} in {country.Name}.",
            Category: EventCategory.Government,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
