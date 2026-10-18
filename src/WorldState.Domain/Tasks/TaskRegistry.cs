using System.Text.Json;
using WorldState.Domain.Models;

namespace WorldState.Domain.Tasks;

public class TaskDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "Economic";
    public bool IsPrincipal { get; set; } = false;
    public string? PrerequisiteTaskId { get; set; }
    public List<SubTaskDefinition> SubTasks { get; set; } = new();
    public TaskRewardDefinition Reward { get; set; } = new();

    public GameTask ToGameTask()
    {
        var cat = Enum.TryParse<TaskCategory>(Category, true, out var parsedCat)
            ? parsedCat
            : TaskCategory.Economic;

        var task = new GameTask
        {
            Id = Id,
            Title = Title,
            Description = Description,
            Category = cat,
            IsPrincipal = IsPrincipal,
            PrerequisiteTaskId = PrerequisiteTaskId,
            Status = string.IsNullOrEmpty(PrerequisiteTaskId) ? GameTaskStatus.Active : GameTaskStatus.Inactive,
            Reward = new TaskReward(
                TreasuryBonus: Reward.TreasuryBonus,
                ApprovalBonus: Reward.ApprovalBonus,
                StabilityBonus: Reward.StabilityBonus,
                AchievementId: Reward.AchievementId
            ),
            SubTasks = SubTasks.Select(st => new SubTaskState(st.Id, st.Description, st.TargetCount, 0)).ToList()
        };

        return task;
    }
}

public class SubTaskDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TargetCount { get; set; }
}

public class TaskRewardDefinition
{
    public decimal TreasuryBonus { get; set; }
    public double ApprovalBonus { get; set; }
    public double StabilityBonus { get; set; }
    public string? AchievementId { get; set; }
}

public class TaskRegistry
{
    private readonly Dictionary<string, TaskDefinition> _definitions = new();

    public IReadOnlyCollection<TaskDefinition> Definitions => _definitions.Values;

    public void Register(TaskDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new ArgumentException("Task ID cannot be null or empty.");
        }
        _definitions[definition.Id] = definition;
    }

    public bool TryGet(string taskId, out TaskDefinition? definition) =>
        _definitions.TryGetValue(taskId, out definition);

    public void LoadFromJson(string json)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var list = JsonSerializer.Deserialize<List<TaskDefinition>>(json, options)
            ?? throw new JsonException("Failed to deserialize task definitions.");

        foreach (var def in list)
        {
            Register(def);
        }
    }

    public static TaskRegistry LoadFromDirectory(string directoryPath)
    {
        var registry = new TaskRegistry();
        if (Directory.Exists(directoryPath))
        {
            foreach (var file in Directory.GetFiles(directoryPath, "*.json"))
            {
                string json = File.ReadAllText(file);
                registry.LoadFromJson(json);
            }
        }
        return registry;
    }

    public Dictionary<string, GameTask> CreateInitialTaskState()
    {
        var tasks = new Dictionary<string, GameTask>();
        foreach (var def in _definitions.Values)
        {
            tasks[def.Id] = def.ToGameTask();
        }
        return tasks;
    }

    public static Dictionary<string, GameTask> CreateDefaultModernTasks()
    {
        var registry = new TaskRegistry();
        registry.Register(new TaskDefinition
        {
            Id = "TSK_INFRA_01",
            Title = "Modernize National Energy Grid",
            Description = "Construct sufficient power generating stations to eliminate regional blackouts and fuel industrial growth.",
            Category = "Economic",
            IsPrincipal = true,
            SubTasks = new() { new() { Id = "ST_POWER", Description = "Construct 40 Power Plants across industrial corridors", TargetCount = 40 } },
            Reward = new() { TreasuryBonus = 2_000_000_000m, ApprovalBonus = 4.0, StabilityBonus = 3.0, AchievementId = "ACH_POWER_GRID" }
        });
        registry.Register(new TaskDefinition
        {
            Id = "TSK_AGRI_01",
            Title = "National Food Security Mission",
            Description = "Expand agricultural output and grain storage facilities to guarantee nutritional sovereignty.",
            Category = "Economic",
            IsPrincipal = true,
            SubTasks = new() { new() { Id = "ST_FARMS", Description = "Operate 60 Commercial Agricultural Farms", TargetCount = 60 } },
            Reward = new() { TreasuryBonus = 1_500_000_000m, ApprovalBonus = 5.0, StabilityBonus = 2.0, AchievementId = "ACH_BUMPER_HARVEST" }
        });
        registry.Register(new TaskDefinition
        {
            Id = "TSK_IND_01",
            Title = "Industrial Manufacturing Corridor",
            Description = "Commission state-of-the-art heavy fabrication and consumer production factories.",
            Category = "Economic",
            IsPrincipal = false,
            PrerequisiteTaskId = "TSK_INFRA_01",
            SubTasks = new() { new() { Id = "ST_FACTORIES", Description = "Establish 50 Manufacturing Factories", TargetCount = 50 } },
            Reward = new() { TreasuryBonus = 3_000_000_000m, ApprovalBonus = 3.0, StabilityBonus = 2.0, AchievementId = "ACH_INDUSTRIAL_GIANT" }
        });
        registry.Register(new TaskDefinition
        {
            Id = "TSK_MIN_01",
            Title = "Strategic Mineral Extraction",
            Description = "Expand domestic mining concessions to ensure raw iron and mineral independence.",
            Category = "Economic",
            IsPrincipal = false,
            SubTasks = new() { new() { Id = "ST_MINES", Description = "Develop 35 Iron & Mineral Mines", TargetCount = 35 } },
            Reward = new() { TreasuryBonus = 1_800_000_000m, ApprovalBonus = 2.0, StabilityBonus = 2.0, AchievementId = "ACH_MINERAL_WEALTH" }
        });
        registry.Register(new TaskDefinition
        {
            Id = "TSK_HLTH_01",
            Title = "Universal Public Healthcare Initiative",
            Description = "Commission district hospitals and boost healthcare allocations to elevate citizen life expectancy.",
            Category = "Welfare",
            IsPrincipal = true,
            SubTasks = new() { new() { Id = "ST_HOSPITALS", Description = "Construct 30 Regional Specialty Hospitals", TargetCount = 30 } },
            Reward = new() { TreasuryBonus = 1_200_000_000m, ApprovalBonus = 7.0, StabilityBonus = 4.0, AchievementId = "ACH_HEALTH_SHIELD" }
        });
        registry.Register(new TaskDefinition
        {
            Id = "TSK_EDU_01",
            Title = "Foundational Literacy & Technology Schools",
            Description = "Expand primary and vocational academies to build a skilled modern workforce.",
            Category = "Welfare",
            IsPrincipal = false,
            SubTasks = new() { new() { Id = "ST_SCHOOLS", Description = "Establish 40 Model Public Schools", TargetCount = 40 } },
            Reward = new() { TreasuryBonus = 1_500_000_000m, ApprovalBonus = 6.0, StabilityBonus = 3.0, AchievementId = "ACH_ENLIGHTENED_NATION" }
        });
        registry.Register(new TaskDefinition
        {
            Id = "TSK_DEF_01",
            Title = "Territorial Sovereign Defense Readiness",
            Description = "Fortify border garrisons and military installations to guarantee deterrence against adversaries.",
            Category = "Defense",
            IsPrincipal = false,
            SubTasks = new() { new() { Id = "ST_BARRACKS", Description = "Construct and garrison 25 Military Barracks", TargetCount = 25 } },
            Reward = new() { TreasuryBonus = 2_500_000_000m, ApprovalBonus = 4.0, StabilityBonus = 6.0, AchievementId = "ACH_IRON_FORTRESS" }
        });
        registry.Register(new TaskDefinition
        {
            Id = "TSK_DIP_01",
            Title = "National Prestige & Democratic Mandate",
            Description = "Attain decisive domestic public confidence and unwavering political stability.",
            Category = "Diplomatic",
            IsPrincipal = true,
            SubTasks = new()
            {
                new() { Id = "ST_APPROVAL_70", Description = "Achieve public approval rating of at least 70%", TargetCount = 70 },
                new() { Id = "ST_STABILITY_75", Description = "Achieve national stability score of at least 75%", TargetCount = 75 }
            },
            Reward = new() { TreasuryBonus = 5_000_000_000m, ApprovalBonus = 10.0, StabilityBonus = 10.0, AchievementId = "ACH_SOVEREIGN_REPUBLIC" }
        });

        return registry.CreateInitialTaskState();
    }
}
