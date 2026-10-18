using WorldState.Domain.Commands;
using WorldState.Domain.Models;
using Xunit;

namespace WorldState.Domain.Tests;

public class SimulationTimeAndCadenceTests
{
    [Fact]
    public void Tick_FrameRateIndependence_ProducesIdenticalOutcomes()
    {
        // Arrange
        // Session A: 60 ticks of 1.0s (60 real seconds)
        var sessionA = GameSession.CreateNewGame("IND");
        sessionA.Clock.Speed = ClockSpeed.Normal; // 1s per day
        var engineA = new SimulationEngine(sessionA);

        // Session B: 3600 ticks of (1/60)s (60 real seconds at 60 FPS)
        var sessionB = GameSession.CreateNewGame("IND");
        sessionB.Clock.Speed = ClockSpeed.Normal;
        var engineB = new SimulationEngine(sessionB);

        // Act
        for (int i = 0; i < 60; i++)
        {
            engineA.Tick(1.0);
        }

        double dt = 1.0 / 60.0;
        for (int i = 0; i < 3600; i++)
        {
            engineB.Tick(dt);
        }

        // Assert
        Assert.Equal(new DateOnly(2026, 3, 2), sessionA.Clock.CurrentDate);
        Assert.Equal(sessionA.Clock.CurrentDate, sessionB.Clock.CurrentDate);

        var indiaA = sessionA.GetPlayerCountry()!;
        var indiaB = sessionB.GetPlayerCountry()!;

        Assert.Equal(indiaA.Treasury, indiaB.Treasury);
        Assert.Equal(indiaA.Stockpiles.Food, indiaB.Stockpiles.Food);
        Assert.Equal(indiaA.Stockpiles.Energy, indiaB.Stockpiles.Energy);
        Assert.Equal(indiaA.Stockpiles.Iron, indiaB.Stockpiles.Iron);
        Assert.Equal(sessionA.Events.Count, sessionB.Events.Count);
    }

    [Fact]
    public void Tick_SpeedSettings_ScaleCadenceAccurately()
    {
        // 1. Paused
        var sessionPaused = GameSession.CreateNewGame("IND");
        sessionPaused.Clock.Speed = ClockSpeed.Paused;
        sessionPaused.Clock.Tick(10.0);
        Assert.Equal(new DateOnly(2026, 1, 1), sessionPaused.Clock.CurrentDate);

        // 2. Normal (1s per day)
        var sessionNormal = GameSession.CreateNewGame("IND");
        sessionNormal.Clock.Speed = ClockSpeed.Normal;
        sessionNormal.Clock.Tick(5.0);
        Assert.Equal(new DateOnly(2026, 1, 6), sessionNormal.Clock.CurrentDate);

        // 3. Fast (0.5s per day -> 2 days per second)
        var sessionFast = GameSession.CreateNewGame("IND");
        sessionFast.Clock.Speed = ClockSpeed.Fast;
        sessionFast.Clock.Tick(5.0); // 5s * 2 days/s = 10 days
        Assert.Equal(new DateOnly(2026, 1, 11), sessionFast.Clock.CurrentDate);

        // 4. Ultra (0.25s per day -> 4 days per second)
        var sessionUltra = GameSession.CreateNewGame("IND");
        sessionUltra.Clock.Speed = ClockSpeed.Ultra;
        sessionUltra.Clock.Tick(5.0); // 5s * 4 days/s = 20 days
        Assert.Equal(new DateOnly(2026, 1, 21), sessionUltra.Clock.CurrentDate);
    }

    [Fact]
    public void Tick_FrameStallProtection_ClampsToMaxDaysPerTickWithoutHanging()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        session.Clock.Speed = ClockSpeed.Normal;

        // Act: 10,000 real seconds in a single tick (e.g. system sleep/resume)
        session.Clock.Tick(10_000.0);

        // Assert: Advances capped MaxDaysPerTick (365 days)
        Assert.Equal(new DateOnly(2027, 1, 1), session.Clock.CurrentDate);
        Assert.Equal(0.0, session.Clock.AccumulatorSeconds);
    }

    [Fact]
    public void TimedOperation_CompletesOnTargetDate_AndAppliesEffects()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        int startingPowerPlants = country.Facilities.GetValueOrDefault("PowerPlants", 0);

        var startCommand = new StartTimedConstructionCommand("IND", "PowerPlants", quantity: 3, buildDays: 30);
        var result = engine.Dispatch(startCommand);
        Assert.True(result.IsSuccess);
        Assert.Single(session.TimedOperations);

        var op = session.TimedOperations[0];
        Assert.Equal(30, op.TotalDays);

        // Act 1: Advance 15 days
        engine.Tick(15.0); // 15 days pass
        Assert.Equal(new DateOnly(2026, 1, 16), session.Clock.CurrentDate);
        Assert.Equal(15, op.GetDaysRemaining(session.Clock.CurrentDate));
        Assert.Equal(0.5, op.GetProgressRatio(session.Clock.CurrentDate), precision: 2);
        // Facilities should NOT be added yet
        Assert.Equal(startingPowerPlants, country.Facilities["PowerPlants"]);

        // Act 2: Advance remaining 15 days
        engine.Tick(15.0); // Now 30 days have elapsed (Jan 31)
        Assert.Equal(new DateOnly(2026, 1, 31), session.Clock.CurrentDate);

        // Assert
        Assert.Empty(session.TimedOperations); // Completed operation was cleaned up
        Assert.Equal(startingPowerPlants + 3, country.Facilities["PowerPlants"]);
        Assert.Contains(session.Events, e => e.Title == "Facility Construction Finished");
    }

    [Fact]
    public void ConductArmyDrills_AppliesCooldownAndTimedBuff()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        double startingStability = country.Stability;

        var drillCmd = new ConductArmyDrillsCommand("IND");

        // Act 1: Initiate drills
        var drillResult = engine.Dispatch(drillCmd);
        Assert.True(drillResult.IsSuccess);
        Assert.True(session.Cooldowns.IsOnCooldown("IND", "ArmyDrills", session.Clock.CurrentDate));

        // Act 2: Try drilling again while on cooldown
        var secondDrillResult = engine.Dispatch(drillCmd);
        Assert.False(secondDrillResult.IsSuccess);
        Assert.Equal("ActionOnCooldown", secondDrillResult.ErrorCode);

        // Act 3: Advance 30 days to finish drill exercise
        engine.Tick(30.0);
        Assert.Equal(startingStability + 2.0, country.Stability);
        Assert.Contains(session.Events, e => e.Title == "Army Drills Concluded");

        // Still on cooldown (cooldown is 180 days)
        Assert.True(session.Cooldowns.IsOnCooldown("IND", "ArmyDrills", session.Clock.CurrentDate));

        // Act 4: Advance past 180 days
        engine.Tick(155.0); // Total days passed = 185
        Assert.False(session.Cooldowns.IsOnCooldown("IND", "ArmyDrills", session.Clock.CurrentDate));

        // Can drill again now
        var freshDrillResult = engine.Dispatch(drillCmd);
        Assert.True(freshDrillResult.IsSuccess);
    }

    [Fact]
    public void MonthlyRollover_SettlesLedgerAndCompletesEvaluatedTasks()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        // Set up task to have 35 Power Plants (Starts with 30)
        var task = new GameTask
        {
            Id = "TSK_POWER_EXPANSION",
            Title = "Power Grid Expansion",
            Category = TaskCategory.Economic,
            Status = GameTaskStatus.Active,
            SubTasks = new List<SubTaskState>
            {
                new("ST_POWER", "Reach 35 Power Plants", targetCount: 35, currentCount: 30)
            },
            Reward = new TaskReward(TreasuryBonus: 500_000_000m)
        };
        session.Tasks[task.Id] = task;

        // Construct 5 PowerPlants instantly to meet criteria
        country.Facilities["PowerPlants"] = 35;

        // Act: Advance 31 days to trigger Month Rollover (Jan 1 -> Feb 1)
        bool monthRolloverFired = false;
        DateOnly? rolloverDate = null;
        engine.OnMonthRollover += date =>
        {
            monthRolloverFired = true;
            rolloverDate = date;
        };

        engine.Tick(31.0); // 31 days

        // Assert
        Assert.Equal(new DateOnly(2026, 2, 1), session.Clock.CurrentDate);
        Assert.True(monthRolloverFired);
        Assert.Equal(new DateOnly(2026, 2, 1), rolloverDate);

        // Check Monthly Ledger Settled event
        Assert.Contains(session.Events, e => e.Title == "Monthly Ledger Settled");

        // Check Task evaluated and completed
        Assert.Equal(GameTaskStatus.Completed, task.Status);
        Assert.Contains(session.Events, e => e.Title == "Presidential Task Completed");
    }
}
