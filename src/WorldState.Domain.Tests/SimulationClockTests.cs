using WorldState.Domain.Models;
using Xunit;

namespace WorldState.Domain.Tests;

public class SimulationClockTests
{
    [Fact]
    public void Tick_WhenPaused_DoesNotAdvanceDate()
    {
        // Arrange
        var clock = new SimulationClock
        {
            CurrentDate = new DateOnly(2026, 1, 1),
            Speed = ClockSpeed.Paused
        };

        // Act
        clock.Tick(10.0); // 10 seconds elapse

        // Assert
        Assert.Equal(new DateOnly(2026, 1, 1), clock.CurrentDate);
    }

    [Fact]
    public void Tick_AtNormalSpeed_AdvancesDaysDeterministically()
    {
        // Arrange
        var clock = new SimulationClock
        {
            CurrentDate = new DateOnly(2026, 1, 1),
            Speed = ClockSpeed.Normal // 1 second per day
        };

        // Act
        clock.Tick(3.0); // 3 seconds elapse = 3 days

        // Assert
        Assert.Equal(new DateOnly(2026, 1, 4), clock.CurrentDate);
    }

    [Fact]
    public void AdvanceDay_WhenMonthChanges_FiresOnMonthPassedEvent()
    {
        // Arrange
        var clock = new SimulationClock
        {
            CurrentDate = new DateOnly(2026, 1, 31),
            Speed = ClockSpeed.Normal
        };

        bool monthEventFired = false;
        DateOnly? eventDate = null;
        clock.OnMonthPassed += date =>
        {
            monthEventFired = true;
            eventDate = date;
        };

        // Act
        clock.AdvanceDay();

        // Assert
        Assert.Equal(new DateOnly(2026, 2, 1), clock.CurrentDate);
        Assert.True(monthEventFired);
        Assert.Equal(new DateOnly(2026, 2, 1), eventDate);
    }
}
