using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class AdjustMinistryFundingCommand : ICommand
{
    public string CountryId { get; }
    public MinistryType Ministry { get; }
    public FundingLevel NewFunding { get; }

    public AdjustMinistryFundingCommand(string countryId, MinistryType ministry, FundingLevel newFunding)
    {
        CountryId = countryId;
        Ministry = ministry;
        NewFunding = newFunding;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found.");
        }

        if (!country.Ministries.TryGetValue(Ministry, out var ministryState))
        {
            ministryState = new MinistryState(Ministry, null, NewFunding);
        }

        country.Ministries[Ministry] = ministryState with { Funding = NewFunding };

        var severity = NewFunding == FundingLevel.Zero ? EventSeverity.Warning : EventSeverity.Info;
        string warningText = NewFunding == FundingLevel.Zero ? " Warning: Zero funding triggers sector unrest!" : "";

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Ministry Budget Modified",
            Message: $"{Ministry} funding set to {NewFunding} ({((int)NewFunding)}%).{warningText}",
            Category: EventCategory.Government,
            Severity: severity,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
