using WorldState.Domain.Models;
using WorldState.Domain.Navigation;

namespace WorldState.Domain.ViewModels;

public record EventItemViewModel(
    string Id,
    string DateText,
    string Title,
    string Message,
    EventCategory Category,
    EventSeverity Severity,
    string CategoryText,
    string SeverityBadgeText,
    bool IsRead,
    ScreenRoute? ActionRoute,
    string? ActionButtonText
);

public class EventHistoryViewModel
{
    public IReadOnlyList<EventItemViewModel> Events { get; init; } = Array.Empty<EventItemViewModel>();
    public int TotalEvents { get; init; }
    public int UnreadCount { get; init; }
    public string ActiveFilter { get; init; } = "All";
    public string HeaderSummary => $"National Newsfeed & Event History: {UnreadCount} Unread Alerts ({TotalEvents} Total)";

    public static EventHistoryViewModel FromSession(GameSession session, string filter = "All")
    {
        ArgumentNullException.ThrowIfNull(session);

        var allEvents = session.Events;
        int unread = allEvents.Count(e => !e.IsRead);

        IEnumerable<GameEvent> filtered = allEvents;
        if (!string.Equals(filter, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (Enum.TryParse<EventCategory>(filter, true, out var cat))
            {
                filtered = allEvents.Where(e => e.Category == cat);
            }
        }

        // Most recent first
        var items = filtered.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id).Select(e =>
        {
            string? btnText = e.ActionRoute switch
            {
                ScreenRoute.GovernmentCabinet => "View Cabinet",
                ScreenRoute.EconomyAndBudget => "View Economy",
                ScreenRoute.DiplomacyAndTreaties => "View Diplomacy",
                ScreenRoute.MilitaryAndDefense => "View Military",
                ScreenRoute.TasksAndObjectives => "View Objectives",
                ScreenRoute.Achievements => "View Achievements",
                _ => null
            };

            return new EventItemViewModel(
                Id: e.Id,
                DateText: e.Date.ToString("yyyy-MM-dd"),
                Title: e.Title,
                Message: e.Message,
                Category: e.Category,
                Severity: e.Severity,
                CategoryText: e.Category.ToString(),
                SeverityBadgeText: e.Severity switch
                {
                    EventSeverity.Critical => "CRITICAL",
                    EventSeverity.Warning => "WARNING",
                    _ => "INFO"
                },
                IsRead: e.IsRead,
                ActionRoute: e.ActionRoute,
                ActionButtonText: btnText
            );
        }).ToList();

        return new EventHistoryViewModel
        {
            Events = items,
            TotalEvents = allEvents.Count,
            UnreadCount = unread,
            ActiveFilter = filter
        };
    }
}
