using WorldState.Domain.Data;
using WorldState.Domain.Map;
using WorldState.Domain.Models;
using WorldState.Domain.Scenarios;

namespace WorldState.Domain.ViewModels;

public class NewGameSetupViewModel
{
    private readonly CountryRegistry _registry;
    private readonly ScenarioDefinition _scenario;

    public CountrySelectionViewModel CountrySelection { get; }

    public int Seed { get; set; } = 42;
    public string Difficulty { get; set; } = "Standard";
    public ScenarioDefinition Scenario => _scenario;

    public bool CanStart => CountrySelection.CanStartGame;

    public NewGameSetupViewModel(
        CountryRegistry registry,
        WorldMapData? mapData = null,
        ScenarioDefinition? scenario = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _scenario = scenario ?? ScenarioDefinition.CreateDefault();
        CountrySelection = new CountrySelectionViewModel(registry, mapData);

        // Preselect default country from scenario if valid
        if (!string.IsNullOrEmpty(_scenario.DefaultCountryId))
        {
            CountrySelection.SelectCountry(_scenario.DefaultCountryId);
        }
    }

    public GameSession StartGame()
    {
        if (!CanStart || CountrySelection.SelectedCountry == null)
        {
            throw new InvalidOperationException("Cannot start game without a selected playable country.");
        }

        var config = new NewGameConfig(
            ScenarioId: _scenario.Id,
            PlayerCountryId: CountrySelection.SelectedCountry.Id,
            Seed: Seed,
            Difficulty: Difficulty
        );

        return GameInitializer.InitializeNewGame(config, _registry, _scenario);
    }
}
