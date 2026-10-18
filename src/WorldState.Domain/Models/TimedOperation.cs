namespace WorldState.Domain.Models;

public class TimedOperation
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string CountryId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty; // "Construction", "ArmyDrills", "Recruitment", "Espionage"
    public DateOnly StartDate { get; set; }
    public DateOnly CompletionDate { get; set; }
    public int TotalDays { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    public bool IsCompleted { get; set; } = false;

    public int GetDaysRemaining(DateOnly currentDate) =>
        Math.Max(0, CompletionDate.DayNumber - currentDate.DayNumber);

    public double GetProgressRatio(DateOnly currentDate)
    {
        if (TotalDays <= 0) return 1.0;
        int elapsed = currentDate.DayNumber - StartDate.DayNumber;
        return Math.Clamp(elapsed / (double)TotalDays, 0.0, 1.0);
    }
}
