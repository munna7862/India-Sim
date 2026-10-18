using WorldState.Domain.Events;
using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class ResolveEventChoiceCommand : ICommand
{
    public string CountryId { get; }
    public string EventId { get; }
    public string ChoiceId { get; }

    public ResolveEventChoiceCommand(string countryId, string eventId, string choiceId)
    {
        CountryId = countryId;
        EventId = eventId;
        ChoiceId = choiceId;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found in world state.");
        }

        if (session.PendingEvent == null)
        {
            return CommandResult.Failure("NoPendingEvent", "There is no active pending emergency event awaiting resolution.");
        }

        if (!string.Equals(session.PendingEvent.EventId, EventId, StringComparison.OrdinalIgnoreCase))
        {
            return CommandResult.Failure(
                "MismatchedEvent",
                $"Pending event '{session.PendingEvent.EventId}' does not match requested event '{EventId}'."
            );
        }

        var choice = session.PendingEvent.Choices.FirstOrDefault(c =>
            string.Equals(c.Id, ChoiceId, StringComparison.OrdinalIgnoreCase));

        if (choice == null)
        {
            return CommandResult.Failure("InvalidChoice", $"Choice '{ChoiceId}' is not valid for event '{EventId}'.");
        }

        if (country.Treasury < choice.TreasuryCost)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Treasury ({country.Treasury:C0}) cannot cover response cost of {choice.TreasuryCost:C0}."
            );
        }

        // Apply state mutations
        country.Treasury -= choice.TreasuryCost;
        country.ApprovalRating = Math.Clamp(country.ApprovalRating + choice.ApprovalDelta, 0.0, 100.0);
        country.Stability = Math.Clamp(country.Stability + choice.StabilityDelta, 0.0, 100.0);
        country.CorruptionIndex = Math.Clamp(country.CorruptionIndex + choice.CorruptionDelta, 0.0, 100.0);

        // Set event cooldown
        session.EventCooldowns[EventId] = 12; // 12-month standard cooldown

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: $"Resolved: {session.PendingEvent.Title}",
            Message: $"{country.Name} enacted decision '{choice.Title}': {choice.Description} (Cost: ${choice.TreasuryCost / 1_000_000m:F0}M).",
            Category: EventCategory.Emergency,
            Severity: EventSeverity.Info,
            ActorCountryId: CountryId
        ));

        // Clear active event prompt
        session.PendingEvent = null;

        return CommandResult.Success();
    }
}
