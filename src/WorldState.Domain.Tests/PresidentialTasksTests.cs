using System.Text.Json;
using WorldState.Domain.Commands;
using WorldState.Domain.Models;
using WorldState.Domain.Tasks;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class PresidentialTasksTests
{
    [Fact]
    public void TaskRegistry_LoadFromJson_RegistersAllTasksAccurately()
    {
        // Arrange
        string tasksPath = Path.Combine(AppContext.BaseDirectory, "../../../../../content/tasks/tasks_modern.json");
        Assert.True(File.Exists(tasksPath), $"tasks_modern.json not found at {tasksPath}");
        string json = File.ReadAllText(tasksPath);

        var registry = new TaskRegistry();

        // Act
        registry.LoadFromJson(json);

        // Assert
        Assert.Equal(8, registry.Definitions.Count);
        Assert.True(registry.TryGet("TSK_INFRA_01", out var infraTask));
        Assert.NotNull(infraTask);
        Assert.True(infraTask.IsPrincipal);
        Assert.Single(infraTask.SubTasks);
        Assert.Equal("ST_POWER", infraTask.SubTasks[0].Id);
        Assert.Equal(40, infraTask.SubTasks[0].TargetCount);
    }

    [Fact]
    public void TaskRegistry_CreateDefaultModernTasks_ContainsPrincipalAndSecondaryTasks()
    {
        // Act
        var tasks = TaskRegistry.CreateDefaultModernTasks();

        // Assert
        Assert.NotEmpty(tasks);
        Assert.Contains(tasks.Values, t => t.IsPrincipal);
        Assert.Contains(tasks.Values, t => !t.IsPrincipal);
        Assert.Contains(tasks.Values, t => t.Status == GameTaskStatus.Inactive); // Dependent task locked initially
    }

    [Fact]
    public void SimulationEngine_EvaluateTasks_UpdatesFacilityCounters_AndCompletesTask()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        // India default starts with 30 PowerPlants. Requirement is 40.
        var task = session.Tasks["TSK_INFRA_01"];
        Assert.Equal(GameTaskStatus.Active, task.Status);
        Assert.False(task.SubTasks[0].IsCompleted);

        // Act 1: Initial evaluation
        engine.EvaluateTasks();
        Assert.Equal(30, task.SubTasks[0].CurrentCount);
        Assert.Equal(75.0, task.ProgressPercentage);
        Assert.Equal(GameTaskStatus.Active, task.Status);

        // Act 2: Construct 10 more power plants to reach 40
        country.Facilities["PowerPlants"] = 40;
        engine.EvaluateTasks();

        // Assert
        Assert.Equal(40, task.SubTasks[0].CurrentCount);
        Assert.Equal(100.0, task.ProgressPercentage);
        Assert.Equal(GameTaskStatus.Completed, task.Status);
        Assert.Contains(session.Events, e => e.Category == EventCategory.Tasks && e.Title == "Presidential Task Completed");
    }

    [Fact]
    public void SimulationEngine_EvaluateTasks_UnlocksDependentTasks_WhenPrerequisiteCompletes()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        var lockedTask = session.Tasks["TSK_IND_01"];
        Assert.Equal("TSK_INFRA_01", lockedTask.PrerequisiteTaskId);
        Assert.Equal(GameTaskStatus.Inactive, lockedTask.Status);

        // Act: Complete prerequisite task
        country.Facilities["PowerPlants"] = 40;
        engine.EvaluateTasks();

        // Assert
        Assert.Equal(GameTaskStatus.Completed, session.Tasks["TSK_INFRA_01"].Status);
        Assert.Equal(GameTaskStatus.Active, lockedTask.Status);
        Assert.Contains(session.Events, e => e.Category == EventCategory.Tasks && e.Title == "New Objective Available");
    }

    [Fact]
    public void SimulationEngine_EvaluateTasks_TracksApprovalAndStabilityMilestones()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        // India default: Approval 65.0, Stability 70.0. Target: Approval 70, Stability 75.
        var prestigeTask = session.Tasks["TSK_DIP_01"];
        Assert.Equal(GameTaskStatus.Active, prestigeTask.Status);

        engine.EvaluateTasks();
        Assert.False(prestigeTask.SubTasks.All(st => st.IsCompleted));

        // Act: Boost stats to threshold
        country.ApprovalRating = 72.0;
        country.Stability = 76.0;
        engine.EvaluateTasks();

        // Assert
        Assert.True(prestigeTask.SubTasks.All(st => st.IsCompleted));
        Assert.Equal(GameTaskStatus.Completed, prestigeTask.Status);
    }

    [Fact]
    public void TasksViewModel_FromSession_CategorizesPrincipalAndSecondaryTasksWithAccuratePercentages()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        engine.EvaluateTasks();

        // Act
        var vm = TasksViewModel.FromSession(session);

        // Assert
        Assert.NotEmpty(vm.PrincipalTasks);
        Assert.NotEmpty(vm.SecondaryTasks);
        Assert.Equal(session.Tasks.Count, vm.TotalCount);
        Assert.Contains(vm.PrincipalTasks, t => t.Id == "TSK_INFRA_01");
        var infraVm = vm.PrincipalTasks.First(t => t.Id == "TSK_INFRA_01");
        Assert.Equal(75.0, infraVm.ProgressPercentage);
        Assert.Equal("75%", infraVm.ProgressText);
        Assert.False(infraVm.CanClaimReward);
    }

    [Fact]
    public void SaveLoadRoundTrip_PreservesTaskStatesAndClaimedRewards()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        country.Facilities["PowerPlants"] = 40;
        engine.EvaluateTasks();

        var claimCmd = new ClaimTaskRewardCommand("IND", "TSK_INFRA_01");
        var claimResult = engine.Dispatch(claimCmd);
        Assert.True(claimResult.IsSuccess);

        // Act: Serialize and deserialize
        string json = JsonSerializer.Serialize(session);
        var restored = JsonSerializer.Deserialize<GameSession>(json);

        // Assert
        Assert.NotNull(restored);
        Assert.Contains("TSK_INFRA_01", restored.ClaimedRewardIds);
        Assert.True(restored.Tasks.TryGetValue("TSK_INFRA_01", out var restoredTask));
        Assert.NotNull(restoredTask);
        Assert.Equal(GameTaskStatus.Completed, restoredTask.Status);
        Assert.True(restoredTask.IsRewardClaimed);
        Assert.Equal(40, restoredTask.SubTasks[0].CurrentCount);
    }
}
