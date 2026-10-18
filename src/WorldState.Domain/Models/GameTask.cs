namespace WorldState.Domain.Models;

public enum TaskCategory
{
    Economic,
    Welfare,
    Defense,
    Diplomatic
}

public enum GameTaskStatus
{
    Inactive,
    Active,
    Completed,
    Failed
}

public record TaskReward(
    decimal TreasuryBonus = 0m,
    double ApprovalBonus = 0.0,
    double StabilityBonus = 0.0,
    string? AchievementId = null
);

public class SubTaskState
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CurrentCount { get; set; }
    public int TargetCount { get; set; }
    public bool IsCompleted => CurrentCount >= TargetCount;

    public SubTaskState() { }

    public SubTaskState(string id, string description, int targetCount, int currentCount = 0)
    {
        Id = id;
        Description = description;
        TargetCount = targetCount;
        CurrentCount = currentCount;
    }
}

public class GameTask
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskCategory Category { get; set; } = TaskCategory.Economic;
    public bool IsPrincipal { get; set; } = false;
    public string? PrerequisiteTaskId { get; set; }
    public DateOnly? Deadline { get; set; }
    public GameTaskStatus Status { get; set; } = GameTaskStatus.Active;
    public List<SubTaskState> SubTasks { get; set; } = new();
    public TaskReward Reward { get; set; } = new();
    public bool IsRewardClaimed { get; set; } = false;

    public double ProgressPercentage
    {
        get
        {
            if (SubTasks.Count == 0) return Status == GameTaskStatus.Completed ? 100.0 : 0.0;
            double sum = 0.0;
            foreach (var st in SubTasks)
            {
                if (st.TargetCount <= 0) sum += 100.0;
                else sum += Math.Min(100.0, (double)st.CurrentCount / st.TargetCount * 100.0);
            }
            return sum / SubTasks.Count;
        }
    }

    public bool CheckCompletion()
    {
        if (Status == GameTaskStatus.Completed)
        {
            return true;
        }

        if (SubTasks.Count > 0 && SubTasks.All(st => st.IsCompleted))
        {
            Status = GameTaskStatus.Completed;
            return true;
        }

        return false;
    }
}
