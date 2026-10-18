namespace WorldState.Domain.Scenarios;

public record NewGameConfig(
    string ScenarioId = "modern_2026",
    string PlayerCountryId = "IND",
    int Seed = 42,
    string Difficulty = "Standard"
);
