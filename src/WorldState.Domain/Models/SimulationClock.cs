namespace WorldState.Domain.Models;

public enum ClockSpeed
{
    Paused = 0,
    Normal = 1,  // 1 in-game day per 1.0 real second
    Fast = 2,    // 1 in-game day per 0.5 real seconds
    Ultra = 3    // 1 in-game day per 0.25 real seconds
}

public class SimulationClock
{
    public DateOnly CurrentDate { get; set; } = new(2026, 1, 1);
    public ClockSpeed Speed { get; set; } = ClockSpeed.Normal;
    public double AccumulatorSeconds { get; set; } = 0.0;

    public event Action<DateOnly>? OnDayPassed;
    public event Action<DateOnly>? OnMonthPassed;

    public double GetSecondsPerDay() => Speed switch
    {
        ClockSpeed.Paused => double.PositiveInfinity,
        ClockSpeed.Normal => 1.0,
        ClockSpeed.Fast => 0.5,
        ClockSpeed.Ultra => 0.25,
        _ => 1.0
    };

    public void Tick(double deltaSeconds)
    {
        if (Speed == ClockSpeed.Paused || deltaSeconds <= 0)
        {
            return;
        }

        AccumulatorSeconds += deltaSeconds;
        double secondsPerDay = GetSecondsPerDay();

        while (AccumulatorSeconds >= secondsPerDay && secondsPerDay > 0)
        {
            AccumulatorSeconds -= secondsPerDay;
            AdvanceDay();
        }
    }

    public void AdvanceDay()
    {
        int previousMonth = CurrentDate.Month;
        CurrentDate = CurrentDate.AddDays(1);
        OnDayPassed?.Invoke(CurrentDate);

        if (CurrentDate.Month != previousMonth)
        {
            OnMonthPassed?.Invoke(CurrentDate);
        }
    }
}
