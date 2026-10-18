namespace WorldState.Domain.Models;

public class GameSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public int SchemaVersion { get; set; } = 1;
    public string PlayerCountryId { get; set; } = "IND";
    public string ScenarioId { get; set; } = "modern_2026";
    public int Seed { get; set; } = 42;
    public string Difficulty { get; set; } = "Standard";
    public SimulationClock Clock { get; set; } = new();
    public Dictionary<string, CountryState> Countries { get; set; } = new();
    public List<GameEvent> Events { get; set; } = new();
    public Dictionary<string, GameTask> Tasks { get; set; } = new();
    public List<string> ClaimedRewardIds { get; set; } = new();
    public List<TimedOperation> TimedOperations { get; set; } = new();
    public CooldownTracker Cooldowns { get; set; } = new();
    public ActiveEventPrompt? PendingEvent { get; set; }
    public Dictionary<string, int> EventCooldowns { get; set; } = new();

    public Dictionary<string, AchievementProgress> Achievements { get; set; } = new();
    public List<string> ClaimedAchievementRewardIds { get; set; } = new();

    [System.Text.Json.Serialization.JsonIgnore]
    public Economy.EconomicPolicyRegistry? PolicyRegistry { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Military.MilitaryUnitRegistry? MilitaryRegistry { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Achievements.AchievementRegistry? AchievementRegistry { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Events.WorldEventRegistry? WorldEventRegistry { get; set; }


    public void AddEvent(GameEvent gameEvent)
    {
        ArgumentNullException.ThrowIfNull(gameEvent);
        Events.Add(gameEvent);
    }

    public CountryState? GetPlayerCountry() =>
        Countries.TryGetValue(PlayerCountryId, out var state) ? state : null;

    public static GameSession CreateNewGame(string protagonistCountryId = "IND")
    {
        var session = new GameSession
        {
            PlayerCountryId = protagonistCountryId,
            Tasks = WorldState.Domain.Tasks.TaskRegistry.CreateDefaultModernTasks()
        };

        var achievementCatalog = WorldState.Domain.Achievements.AchievementRegistry.CreateDefaultCatalog();
        session.AchievementRegistry = achievementCatalog;
        foreach (var ach in achievementCatalog.Achievements)
        {
            session.Achievements[ach.Id] = new AchievementProgress
            {
                AchievementId = ach.Id,
                IsUnlocked = false,
                ProgressPercent = 0.0
            };
        }

        var playerCountry = CountryState.CreateIndiaDefault();
        playerCountry.Id = protagonistCountryId;
        session.Countries[protagonistCountryId] = playerCountry;

        // Foreign countries for interactive diplomacy & sandbox
        var foreignNations = new[]
        {
            ("USA", "United States of America", 150_000_000_000m, 330_000_000L, 35.0, 30),
            ("RUS", "Russian Federation", 40_000_000_000m, 144_000_000L, 50.0, 25),
            ("CHN", "People's Republic of China", 90_000_000_000m, 1_410_000_000L, -20.0, 35),
            ("GBR", "United Kingdom", 45_000_000_000m, 67_000_000L, 40.0, 15),
            ("PAK", "Islamic Republic of Pakistan", 10_000_000_000m, 240_000_000L, -60.0, 18)
        };

        foreach (var (fId, fName, fTreasury, fPop, fRel, fBarracks) in foreignNations)
        {
            if (string.Equals(fId, protagonistCountryId, StringComparison.OrdinalIgnoreCase)) continue;

            var fState = new CountryState
            {
                Id = fId,
                Name = fName,
                Treasury = fTreasury,
                Population = fPop,
                ApprovalRating = 60.0,
                QualityOfLiving = 60.0,
                Stability = 65.0,
                Facilities = new Dictionary<string, int> { ["Barracks"] = fBarracks, ["Factories"] = 30 }
            };

            int barracks = fBarracks;
            fState.MilitaryUnits = new Dictionary<string, int>
            {
                ["infantry_division"] = barracks * 15,
                ["main_battle_tank"] = barracks * 3,
                ["artillery_corps"] = barracks * 2,
                ["multirole_fighter"] = barracks * 2,
                ["guided_missile_destroyer"] = Math.Max(1, barracks / 2)
            };

            session.Countries[fId] = fState;

            // Setup reciprocal relations
            playerCountry.GetRelationsWith(fId).Relations = fRel;
            fState.GetRelationsWith(protagonistCountryId).Relations = fRel;
        }

        return session;
    }
}

