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

    [System.Text.Json.Serialization.JsonIgnore]
    public Economy.EconomicPolicyRegistry? PolicyRegistry { get; set; }

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

        var playerCountry = CountryState.CreateIndiaDefault();
        playerCountry.Id = protagonistCountryId;
        session.Countries[protagonistCountryId] = playerCountry;

        return session;
    }
}
