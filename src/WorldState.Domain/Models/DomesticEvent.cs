namespace WorldState.Domain.Models;

public record EventChoice(
    string Id,
    string Title,
    string Description,
    decimal TreasuryCost,
    double ApprovalDelta,
    double StabilityDelta,
    double CorruptionDelta
);

public class DomesticEventDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Emergency";
    public double? MinCorruption { get; set; }
    public double? MaxApproval { get; set; }
    public double? MaxStability { get; set; }
    public double RandomWeight { get; set; } = 0.30;
    public int CooldownMonths { get; set; } = 12;
    public List<EventChoice> Choices { get; set; } = new();

    public bool CanTrigger(CountryState country, int activeCooldownMonths)
    {
        if (activeCooldownMonths > 0) return false;
        if (MinCorruption.HasValue && country.CorruptionIndex < MinCorruption.Value) return false;
        if (MaxApproval.HasValue && country.ApprovalRating > MaxApproval.Value) return false;
        if (MaxStability.HasValue && country.Stability > MaxStability.Value) return false;
        return true;
    }
}

public class ActiveEventPrompt
{
    public string EventId { get; set; } = string.Empty;
    public string CountryId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EventCategory Category { get; set; } = EventCategory.Emergency;
    public List<EventChoice> Choices { get; set; } = new();
}
