using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class DismissNotificationCommand : ICommand
{
    public string? EventId { get; }

    public DismissNotificationCommand(string? eventId = null)
    {
        EventId = eventId;
    }

    public CommandResult Validate(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return CommandResult.Success();
    }

    public CommandResult Execute(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (string.IsNullOrEmpty(EventId))
        {
            // Mark all read
            for (int i = 0; i < session.Events.Count; i++)
            {
                var evt = session.Events[i];
                if (!evt.IsRead)
                {
                    session.Events[i] = evt with { IsRead = true };
                }
            }
            return CommandResult.Success();
        }

        int index = session.Events.FindIndex(e => string.Equals(e.Id, EventId, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            var evt = session.Events[index];
            session.Events[index] = evt with { IsRead = true };
            return CommandResult.Success();
        }

        return CommandResult.Failure("EventNotFound", $"Event with ID '{EventId}' was not found.");
    }
}
