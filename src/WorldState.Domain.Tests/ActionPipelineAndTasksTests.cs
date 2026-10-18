using WorldState.Domain.Commands;
using WorldState.Domain.Models;
using Xunit;

namespace WorldState.Domain.Tests;

public class ActionPipelineAndTasksTests
{
    [Fact]
    public void SimulationEngine_DispatchesCommand_AndFiresEvent()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        bool eventFired = false;
        ICommand? firedCommand = null;
        CommandResult? firedResult = null;

        engine.OnCommandExecuted += (cmd, res) =>
        {
            eventFired = true;
            firedCommand = cmd;
            firedResult = res;
        };

        var command = new AdjustTaxRateCommand("IND", TaxType.CorporateTax, 0.30);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(eventFired);
        Assert.Same(command, firedCommand);
        Assert.Same(result, firedResult);
        Assert.Equal(0.30, session.GetPlayerCountry()!.CorporateTaxRate);
        Assert.NotEmpty(session.Events);
        Assert.Contains(session.Events, e => e.Category == EventCategory.Economy && e.Title == "Tax Policy Adjusted");
    }

    [Fact]
    public void AdjustMinistryFunding_UpdatesStateAndLogsWarningOnZero()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);

        var command = new AdjustMinistryFundingCommand("IND", MinistryType.Health, FundingLevel.Zero);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.True(result.IsSuccess);
        var healthMinistry = session.GetPlayerCountry()!.Ministries[MinistryType.Health];
        Assert.Equal(FundingLevel.Zero, healthMinistry.Funding);

        var loggedEvent = session.Events.LastOrDefault();
        Assert.NotNull(loggedEvent);
        Assert.Equal(EventSeverity.Warning, loggedEvent.Severity);
        Assert.Contains("Zero funding triggers sector unrest", loggedEvent.Message);
    }

    [Fact]
    public void ConstructFacility_WithSufficientResources_IncrementsCountAndDeductsCost()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        decimal startingTreasury = country.Treasury;
        double startingIron = country.Stockpiles.Iron;
        int startingFarms = country.Facilities.GetValueOrDefault("Farms", 0);

        var command = new ConstructFacilityCommand("IND", "Farms", 5);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(startingFarms + 5, country.Facilities["Farms"]);
        Assert.Equal(startingTreasury - 500_000_000m, country.Treasury);
        Assert.Equal(startingIron - 2_500.0, country.Stockpiles.Iron);
    }

    [Fact]
    public void ConstructFacility_WithInsufficientFunds_FailsWithoutMutatingState()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        country.Treasury = 10_000_000m; // Only $10M
        double startingIron = country.Stockpiles.Iron;
        int startingMines = country.Facilities.GetValueOrDefault("Mines", 0);

        var command = new ConstructFacilityCommand("IND", "Mines", 1); // Cost: $250M

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("InsufficientFunds", result.ErrorCode);
        Assert.Equal(10_000_000m, country.Treasury);
        Assert.Equal(startingIron, country.Stockpiles.Iron);
        Assert.Equal(startingMines, country.Facilities.GetValueOrDefault("Mines", 0));
    }

    [Fact]
    public void ConstructFacility_WithInsufficientIron_FailsWithoutMutatingTreasury()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        decimal startingTreasury = country.Treasury;
        country.Stockpiles = country.Stockpiles with { Iron = 50.0 }; // Only 50 iron

        var command = new ConstructFacilityCommand("IND", "PowerPlants", 1); // Cost: 2000 Iron

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("InsufficientResources", result.ErrorCode);
        Assert.Equal(startingTreasury, country.Treasury);
        Assert.Equal(50.0, country.Stockpiles.Iron);
    }

    [Fact]
    public void ConstructFacility_WithUnknownType_FailsValidation()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);

        var command = new ConstructFacilityCommand("IND", "MoonBase", 1);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UnknownFacilityType", result.ErrorCode);
    }

    [Fact]
    public void TakeLoan_IncreasesTreasuryAndDebt_WithinCeiling()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        decimal startingTreasury = country.Treasury;
        decimal loanAmount = 5_000_000_000m; // $5B

        var command = new TakeLoanCommand("IND", loanAmount);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(startingTreasury + loanAmount, country.Treasury);
        Assert.Equal(loanAmount, country.Debt);
    }

    [Fact]
    public void TakeLoan_ExceedingDebtCeiling_FailsWithoutMutatingState()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        decimal startingTreasury = country.Treasury;
        decimal excessiveLoan = 1_000_000_000_000_000m; // $1,000 Trillion

        var command = new TakeLoanCommand("IND", excessiveLoan);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("DebtCeilingExceeded", result.ErrorCode);
        Assert.Equal(startingTreasury, country.Treasury);
        Assert.Equal(0m, country.Debt);
    }

    [Fact]
    public void RepayLoan_ReducesDebtAndTreasury_Succeeds()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        country.Debt = 10_000_000_000m; // $10B existing debt
        decimal startingTreasury = country.Treasury;
        decimal repayment = 4_000_000_000m; // Repay $4B

        var command = new RepayLoanCommand("IND", repayment);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(startingTreasury - repayment, country.Treasury);
        Assert.Equal(6_000_000_000m, country.Debt);
    }

    [Fact]
    public void RepayLoan_WhenNoDebt_FailsSafely()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        country.Debt = 0m;

        var command = new RepayLoanCommand("IND", 1_000_000m);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("NoOutstandingDebt", result.ErrorCode);
    }

    [Fact]
    public void ClaimTaskReward_CompletedTask_GrantsRewardsOnceAndRejectsDoubleClaim()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        decimal startingTreasury = country.Treasury;
        double startingApproval = country.ApprovalRating;
        double startingStability = country.Stability;

        var task = new GameTask
        {
            Id = "TSK_INFRA_01",
            Title = "Modernize National Energy Grid",
            Category = TaskCategory.Economic,
            Status = GameTaskStatus.Active,
            SubTasks = new List<SubTaskState>
            {
                new("ST_POWER", "Construct 10 Power Plants", targetCount: 10, currentCount: 10)
            },
            Reward = new TaskReward(
                TreasuryBonus: 2_000_000_000m,
                ApprovalBonus: 5.0,
                StabilityBonus: 3.0
            )
        };

        session.Tasks[task.Id] = task;

        var claimCommand = new ClaimTaskRewardCommand("IND", task.Id);

        // Act 1: First claim
        var firstResult = engine.Dispatch(claimCommand);

        // Assert 1
        Assert.True(firstResult.IsSuccess);
        Assert.True(task.IsRewardClaimed);
        Assert.Contains(task.Id, session.ClaimedRewardIds);
        Assert.Equal(startingTreasury + 2_000_000_000m, country.Treasury);
        Assert.Equal(startingApproval + 5.0, country.ApprovalRating);
        Assert.Equal(startingStability + 3.0, country.Stability);

        // Act 2: Duplicate claim
        var secondResult = engine.Dispatch(claimCommand);

        // Assert 2
        Assert.False(secondResult.IsSuccess);
        Assert.Equal("RewardAlreadyClaimed", secondResult.ErrorCode);
        // Counters should NOT have increased again
        Assert.Equal(startingTreasury + 2_000_000_000m, country.Treasury);
        Assert.Equal(startingApproval + 5.0, country.ApprovalRating);
        Assert.Equal(startingStability + 3.0, country.Stability);
    }

    [Fact]
    public void ClaimTaskReward_IncompleteTask_FailsWithoutGrantingRewards()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        decimal startingTreasury = country.Treasury;

        var task = new GameTask
        {
            Id = "TSK_WELFARE_01",
            Title = "Expand Healthcare Network",
            Category = TaskCategory.Welfare,
            Status = GameTaskStatus.Active,
            SubTasks = new List<SubTaskState>
            {
                new("ST_HOSPITALS", "Construct 25 Hospitals", targetCount: 25, currentCount: 12)
            },
            Reward = new TaskReward(TreasuryBonus: 1_000_000_000m)
        };

        session.Tasks[task.Id] = task;

        var command = new ClaimTaskRewardCommand("IND", task.Id);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("TaskNotCompleted", result.ErrorCode);
        Assert.False(task.IsRewardClaimed);
        Assert.Equal(startingTreasury, country.Treasury);
    }
}
