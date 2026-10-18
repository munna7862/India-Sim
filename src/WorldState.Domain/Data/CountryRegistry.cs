using System.Text.Json;
using System.Text.RegularExpressions;

namespace WorldState.Domain.Data;

public class CountryRegistry
{
    private static readonly Regex IsoAlpha3Regex = new(@"^[A-Z]{3}$", RegexOptions.Compiled);
    private readonly Dictionary<string, CountryDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public int Count => _definitions.Count;

    public void Register(CountryDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(definition.Id) || !IsoAlpha3Regex.IsMatch(definition.Id))
        {
            throw new ArgumentException(
                $"Invalid Country ID '{definition.Id}'. Must be a 3-letter uppercase ISO 3166-1 alpha-3 code.",
                nameof(definition)
            );
        }

        if (string.IsNullOrWhiteSpace(definition.Name))
        {
            throw new ArgumentException("Country Name cannot be null or whitespace.", nameof(definition));
        }

        if (definition.StartingTreasury < 0m)
        {
            throw new ArgumentException(
                $"Starting treasury cannot be negative. Received {definition.StartingTreasury:C0}.",
                nameof(definition)
            );
        }

        if (definition.StartingPopulation <= 0)
        {
            throw new ArgumentException(
                $"Starting population must be greater than zero. Received {definition.StartingPopulation}.",
                nameof(definition)
            );
        }

        if (definition.IncomeTaxRate < 0.0 || definition.IncomeTaxRate > 1.0)
        {
            throw new ArgumentException($"Invalid IncomeTaxRate {definition.IncomeTaxRate}.", nameof(definition));
        }

        if (_definitions.ContainsKey(definition.Id))
        {
            throw new InvalidOperationException($"Duplicate Country ID '{definition.Id}' already registered.");
        }

        _definitions[definition.Id] = definition;
    }

    public void RegisterFromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var definition = JsonSerializer.Deserialize<CountryDefinition>(json);
        if (definition == null)
        {
            throw new JsonException("Failed to deserialize CountryDefinition from JSON.");
        }

        Register(definition);
    }

    public CountryDefinition Get(string id)
    {
        if (!_definitions.TryGetValue(id, out var def))
        {
            throw new KeyNotFoundException($"Country with ID '{id}' was not found in registry.");
        }

        return def;
    }

    public bool TryGet(string id, out CountryDefinition? definition) =>
        _definitions.TryGetValue(id, out definition);

    public IReadOnlyList<CountryDefinition> GetAll() =>
        _definitions.Values.ToList();

    public IReadOnlyList<CountryDefinition> GetPlayable() =>
        _definitions.Values.Where(c => c.IsPlayable).ToList();
}
