using WorldState.Domain.Navigation;

namespace WorldState.Domain.Models;

public enum EventCategory
{
    General,
    Government,
    Economy,
    Military,
    Diplomacy,
    Emergency,
    Tasks,
    Achievements,
    World
}

public enum EventSeverity
{
    Info,
    Warning,
    Critical
}

public record GameEvent(
    string Id,
    DateOnly Date,
    string Title,
    string Message,
    EventCategory Category = EventCategory.General,
    EventSeverity Severity = EventSeverity.Info,
    string? ActorCountryId = null,
    string? TargetCountryId = null,
    ScreenRoute? ActionRoute = null,
    bool IsRead = false
);
