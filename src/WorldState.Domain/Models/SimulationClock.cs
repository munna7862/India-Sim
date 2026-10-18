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

    public SimulationClock() { }

    public SimulationClock(DateOnly startDate, ClockSpeed speed = ClockSpeed.Normal)
    {
        CurrentDate = startDate;
        Speed = speed;
    }

    public SimulationClock(DateTime startDate, ClockSpeed speed = ClockSpeed.Normal)
        : this(DateOnly.FromDateTime(startDate), speed)
    {
    }

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

    public const int MaxDaysPerTick = 365;

    public void Tick(double deltaSeconds)
    {
        if (Speed == ClockSpeed.Paused || deltaSeconds <= 0 || double.IsNaN(deltaSeconds) || double.IsInfinity(deltaSeconds))
        {
            return;
        }

        AccumulatorSeconds += deltaSeconds;
        double secondsPerDay = GetSecondsPerDay();

        int daysAdvanced = 0;
        while (AccumulatorSeconds >= secondsPerDay && secondsPerDay > 0 && daysAdvanced < MaxDaysPerTick)
        {
            AccumulatorSeconds -= secondsPerDay;
            AdvanceDay();
            daysAdvanced++;
        }

        // Prevent infinite accumulator accumulation during massive frame stalls
        if (daysAdvanced >= MaxDaysPerTick)
        {
            AccumulatorSeconds = 0.0;
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

    public void AdvanceDays(int days)
    {
        for (int i = 0; i < days; i++)
        {
            AdvanceDay();
        }
    }
}
