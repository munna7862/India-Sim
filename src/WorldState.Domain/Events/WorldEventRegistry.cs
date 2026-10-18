using System.Text.Json;
using WorldState.Domain.Models;

namespace WorldState.Domain.Events;

public class WorldEventRegistry
{
    private readonly Dictionary<string, WorldEventDefinition> _events = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<WorldEventDefinition> Events => _events.Values;

    public void Register(WorldEventDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new ArgumentException("WorldEvent Id cannot be null or whitespace.", nameof(definition));
        }

        _events[definition.Id] = definition;
    }

    public bool TryGet(string eventId, out WorldEventDefinition? definition) =>
        _events.TryGetValue(eventId, out definition);

    public void LoadFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (!root.TryGetProperty("events", out var listElement))
        {
            throw new InvalidOperationException("World Event JSON missing 'events' array.");
        }

        foreach (var item in listElement.EnumerateArray())
        {
            var def = JsonSerializer.Deserialize<WorldEventDefinition>(item.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (def != null && !string.IsNullOrWhiteSpace(def.Id))
            {
                Register(def);
            }
        }
    }

    public static WorldEventRegistry CreateDefaultCatalog()
    {
        var registry = new WorldEventRegistry();

        string[] candidatePaths =
        {
            Path.Combine(AppContext.BaseDirectory, "content", "events", "world_events.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "content", "events", "world_events.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "content", "events", "world_events.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "content", "events", "world_events.json")
        };

        foreach (var path in candidatePaths)
        {
            if (File.Exists(path))
            {
                registry.LoadFromJson(File.ReadAllText(path));
                return registry;
            }
        }

        var defaults = new[]
        {
            new WorldEventDefinition
            {
                Id = "WEV_GLOBAL_TRADE_BOOM",
                Title = "Global Trade Expansion",
                Description = "International shipping demand surges across major sea lanes, increasing global commercial revenues and providing a stimulus to export nations.",
                Category = "Economy",
                Severity = "Info",
                TriggerChance = 0.25,
                CooldownMonths = 12,
                TreasuryImpact = 1_500_000_000m,
                StabilityImpact = 2.0,
                ApprovalImpact = 3.0,
                RelationsImpact = 5.0,
                ActionRoute = "EconomyAndBudget"
            },
            new WorldEventDefinition
            {
                Id = "WEV_ENERGY_CRISIS",
                Title = "Global Energy Disruption",
                Description = "Volatility in international petroleum markets triggers price spikes, testing national reserve stability and industrial power supply.",
                Category = "Economy",
                Severity = "Warning",
                TriggerChance = 0.20,
                CooldownMonths = 18,
                TreasuryImpact = -1_000_000_000m,
                StabilityImpact = -3.0,
                ApprovalImpact = -2.0,
                RelationsImpact = 0.0,
                ActionRoute = "EconomyAndBudget"
            },
            new WorldEventDefinition
            {
                Id = "WEV_UN_PEACE_SUMMIT",
                Title = "Geneva World Peace Accord",
                Description = "Delegates at the United Nations pass a consensus resolution affirming sovereign non-aggression, fostering international goodwill and diplomatic thaw.",
                Category = "Diplomacy",
                Severity = "Info",
                TriggerChance = 0.20,
                CooldownMonths = 15,
                TreasuryImpact = 0m,
                StabilityImpact = 4.0,
                ApprovalImpact = 2.0,
                RelationsImpact = 8.0,
                ActionRoute = "DiplomacyAndTreaties"
            },
            new WorldEventDefinition
            {
                Id = "WEV_REGIONAL_TENSION",
                Title = "Cross-Border Military Drills",
                Description = "Heightened maneuvers along contested strategic frontiers cause neighboring armed forces to elevate readiness alerts.",
                Category = "Military",
                Severity = "Warning",
                TriggerChance = 0.25,
                CooldownMonths = 12,
                TreasuryImpact = -500_000_000m,
                StabilityImpact = -2.0,
                ApprovalImpact = 0.0,
                RelationsImpact = -6.0,
                ActionRoute = "MilitaryAndDefense"
            },
            new WorldEventDefinition
            {
                Id = "WEV_GLOBAL_PANDEMIC_ALERT",
                Title = "WHO Respiratory Virus Alert",
                Description = "An infectious respiratory cluster reported abroad triggers health monitoring protocols across all entry ports and international airports.",
                Category = "Emergency",
                Severity = "Critical",
                TriggerChance = 0.15,
                CooldownMonths = 24,
                TreasuryImpact = -2_000_000_000m,
                StabilityImpact = -4.0,
                ApprovalImpact = -3.0,
                RelationsImpact = -2.0,
                ActionRoute = "GovernmentCabinet"
            },
            new WorldEventDefinition
            {
                Id = "WEV_SCIENTIFIC_BREAKTHROUGH",
                Title = "Breakthrough in Clean Fusion Energy",
                Description = "An international consortium announces experimental net-positive fusion energy milestones, uplifting public confidence and scientific optimism.",
                Category = "Economy",
                Severity = "Info",
                TriggerChance = 0.15,
                CooldownMonths = 20,
                TreasuryImpact = 500_000_000m,
                StabilityImpact = 3.0,
                ApprovalImpact = 4.0,
                RelationsImpact = 3.0,
                ActionRoute = "EconomyAndBudget"
            }
        };

        foreach (var def in defaults)
        {
            registry.Register(def);
        }

        return registry;
    }
}
