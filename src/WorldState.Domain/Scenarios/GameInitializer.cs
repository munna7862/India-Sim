using WorldState.Domain.Data;
using WorldState.Domain.Models;

namespace WorldState.Domain.Scenarios;

public static class GameInitializer
{
    public static GameSession InitializeNewGame(
        NewGameConfig config,
        CountryRegistry registry,
        ScenarioDefinition? scenario = null)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(registry);

        scenario ??= ScenarioDefinition.CreateDefault();

        if (string.IsNullOrWhiteSpace(config.PlayerCountryId))
        {
            throw new ArgumentException("Player country ID cannot be null or empty.", nameof(config));
        }

        if (!registry.TryGet(config.PlayerCountryId, out var playerDef) || playerDef == null)
        {
            throw new InvalidOperationException($"Country '{config.PlayerCountryId}' is not found in the registry.");
        }

        if (!playerDef.IsPlayable)
        {
            throw new InvalidOperationException($"Country '{config.PlayerCountryId}' is marked as non-playable.");
        }

        // Deterministic session ID generated from seed and configuration parameters
        string sessionId = $"SES-{scenario.Id}-{config.PlayerCountryId}-{config.Seed:X8}";

        var session = new GameSession
        {
            SessionId = sessionId,
            SchemaVersion = 1,
            ScenarioId = scenario.Id,
            PlayerCountryId = config.PlayerCountryId,
            Seed = config.Seed,
            Difficulty = config.Difficulty,
            Clock = new SimulationClock(scenario.StartDate, ClockSpeed.Paused)
        };

        // Populate authoritative country states from definitions
        var countriesToInclude = new HashSet<string>(scenario.SupportedCountryIds, StringComparer.OrdinalIgnoreCase)
        {
            config.PlayerCountryId
        };

        foreach (var countryId in countriesToInclude)
        {
            if (registry.TryGet(countryId, out var def) && def != null)
            {
                var countryState = def.ToCountryState();
                session.Countries[def.Id] = countryState;
            }
        }

        return session;
    }
}
