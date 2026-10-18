using WorldState.Domain.Models;

namespace WorldState.Domain.ViewModels;

public record EventChoiceViewModel(
    string Id,
    string Title,
    string Description,
    string CostFormatted,
    string ConsequencesSummary,
    bool CanAfford
);

public record ActiveEventPromptViewModel(
    string EventId,
    string Title,
    string Description,
    string Category,
    string DateText,
    IReadOnlyList<EventChoiceViewModel> Choices
);

public record DomesticEventsViewModel(
    bool HasPendingEvent,
    ActiveEventPromptViewModel? PendingEvent,
    IReadOnlyList<GameEvent> RecentEvents
)
{
    public static DomesticEventsViewModel FromSession(GameSession session)
    {
        var playerCountry = session.GetPlayerCountry();
        decimal treasury = playerCountry?.Treasury ?? 0m;

        ActiveEventPromptViewModel? promptVm = null;
        if (session.PendingEvent != null)
        {
            var choiceVms = session.PendingEvent.Choices.Select(c =>
            {
                var deltas = new List<string>();
                if (c.ApprovalDelta != 0.0)
                {
                    deltas.Add($"{(c.ApprovalDelta > 0 ? "+" : "")}{c.ApprovalDelta:F1}% Approval");
                }
                if (c.StabilityDelta != 0.0)
                {
                    deltas.Add($"{(c.StabilityDelta > 0 ? "+" : "")}{c.StabilityDelta:F1}% Stability");
                }
                if (c.CorruptionDelta != 0.0)
                {
                    deltas.Add($"{(c.CorruptionDelta > 0 ? "+" : "")}{c.CorruptionDelta:F1}% Corruption");
                }

                string consequences = deltas.Count > 0 ? string.Join(", ", deltas) : "No indicator change";
                string costText = c.TreasuryCost > 0m
                    ? HudViewModel.FormatCurrency(c.TreasuryCost)
                    : "No Treasury Cost";

                return new EventChoiceViewModel(
                    Id: c.Id,
                    Title: c.Title,
                    Description: c.Description,
                    CostFormatted: costText,
                    ConsequencesSummary: consequences,
                    CanAfford: treasury >= c.TreasuryCost
                );
            }).ToList();

            promptVm = new ActiveEventPromptViewModel(
                EventId: session.PendingEvent.EventId,
                Title: session.PendingEvent.Title,
                Description: session.PendingEvent.Description,
                Category: session.PendingEvent.Category.ToString(),
                DateText: session.PendingEvent.Date.ToString("dd MMM yyyy").ToUpperInvariant(),
                Choices: choiceVms
            );
        }

        var recent = session.Events
            .Where(e => e.Category == EventCategory.Emergency || e.Category == EventCategory.Government)
            .OrderByDescending(e => e.Date)
            .Take(20)
            .ToList();

        return new DomesticEventsViewModel(
            HasPendingEvent: promptVm != null,
            PendingEvent: promptVm,
            RecentEvents: recent
        );
    }
}
