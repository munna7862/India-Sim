using System.Text.Json;
using WorldState.Domain.Models;

namespace WorldState.Domain.Achievements;

public class AchievementRegistry
{
    private readonly Dictionary<string, AchievementDefinition> _achievements = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<AchievementDefinition> Achievements => _achievements.Values;

    public void Register(AchievementDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new ArgumentException("Achievement Id cannot be null or whitespace.", nameof(definition));
        }

        _achievements[definition.Id] = definition;
    }

    public bool TryGet(string achievementId, out AchievementDefinition? definition) =>
        _achievements.TryGetValue(achievementId, out definition);

    public void LoadFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (!root.TryGetProperty("achievements", out var listElement))
        {
            throw new InvalidOperationException("Achievement JSON missing 'achievements' array.");
        }

        foreach (var item in listElement.EnumerateArray())
        {
            var def = JsonSerializer.Deserialize<AchievementDefinition>(item.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (def != null && !string.IsNullOrWhiteSpace(def.Id))
            {
                Register(def);
            }
        }
    }

    public static AchievementRegistry CreateDefaultCatalog()
    {
        var registry = new AchievementRegistry();

        // Search candidate paths
        string[] candidatePaths =
        {
            Path.Combine(AppContext.BaseDirectory, "content", "achievements", "achievements.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "content", "achievements", "achievements.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "content", "achievements", "achievements.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "content", "achievements", "achievements.json")
        };

        foreach (var path in candidatePaths)
        {
            if (File.Exists(path))
            {
                registry.LoadFromJson(File.ReadAllText(path));
                return registry;
            }
        }

        // Built-in hardcoded fallback if file not reached at runtime
        var defaults = new[]
        {
            new AchievementDefinition
            {
                Id = "ACH_IND_REFORM",
                Title = "Economic Awakening",
                Description = "Enact at least 2 active national economic development policies.",
                Category = "Economy",
                ConditionType = "PolicyCount",
                RequiredThreshold = 2,
                RewardTreasury = 1_000_000_000m,
                RewardPrestigeScore = 100,
                IconKey = "trophy_economy"
            },
            new AchievementDefinition
            {
                Id = "ACH_TITAN_INDUSTRY",
                Title = "Industrial Powerhouse",
                Description = "Construct at least 40 Factories and 30 Power Plants to empower national production.",
                Category = "Economy",
                ConditionType = "FacilityCount",
                TargetKey = "Factories",
                RequiredThreshold = 40,
                SecondaryTargetKey = "PowerPlants",
                SecondaryThreshold = 30,
                RewardTreasury = 2_500_000_000m,
                RewardPrestigeScore = 250,
                IconKey = "trophy_industry"
            },
            new AchievementDefinition
            {
                Id = "ACH_PEACE_MAKER",
                Title = "Architect of Peace",
                Description = "Sign and maintain active Non-Aggression Pacts with at least 2 foreign powers.",
                Category = "Diplomacy",
                ConditionType = "PactCount",
                RequiredThreshold = 2,
                RewardTreasury = 1_500_000_000m,
                RewardPrestigeScore = 200,
                IconKey = "trophy_diplomacy"
            },
            new AchievementDefinition
            {
                Id = "ACH_SOVEREIGN_SOLVENCY",
                Title = "Fiscal Solvency",
                Description = "Eliminate all central bank debt ($0 debt) and maintain over $50 Billion in liquid Treasury reserves.",
                Category = "Economy",
                ConditionType = "DebtAndTreasury",
                RequiredThreshold = 50_000_000_000,
                RewardTreasury = 3_000_000_000m,
                RewardPrestigeScore = 300,
                IconKey = "trophy_treasury"
            },
            new AchievementDefinition
            {
                Id = "ACH_POPULAR_MANDATE",
                Title = "Beloved Republic",
                Description = "Attain an Approval Rating of at least 80% and Quality of Living rating of at least 70%.",
                Category = "Governance",
                ConditionType = "ApprovalAndLiving",
                RequiredThreshold = 80,
                SecondaryThreshold = 70,
                RewardTreasury = 2_000_000_000m,
                RewardPrestigeScore = 250,
                IconKey = "trophy_approval"
            },
            new AchievementDefinition
            {
                Id = "ACH_IRON_DETERRENCE",
                Title = "Invulnerable Bastion",
                Description = "Expand national armed forces to at least 100 units across divisions and conduct army drills.",
                Category = "Military",
                ConditionType = "MilitaryAndDrills",
                RequiredThreshold = 100,
                RewardTreasury = 2_000_000_000m,
                RewardPrestigeScore = 200,
                IconKey = "trophy_military"
            },
            new AchievementDefinition
            {
                Id = "ACH_CLEAN_GOVERNANCE",
                Title = "Integrity in Office",
                Description = "Drive systemic corruption down below 15% through robust justice oversight.",
                Category = "Security",
                ConditionType = "CorruptionThreshold",
                RequiredThreshold = 15,
                RewardTreasury = 1_500_000_000m,
                RewardPrestigeScore = 150,
                IconKey = "trophy_justice"
            },
            new AchievementDefinition
            {
                Id = "ACH_PRESIDENTIAL_MASTER",
                Title = "Visionary Leader",
                Description = "Complete and collect the presidential rewards for at least 3 major national objectives.",
                Category = "Governance",
                ConditionType = "CompletedTasksCount",
                RequiredThreshold = 3,
                RewardTreasury = 5_000_000_000m,
                RewardPrestigeScore = 500,
                IconKey = "trophy_star"
            }
        };

        foreach (var def in defaults)
        {
            registry.Register(def);
        }

        return registry;
    }
}
