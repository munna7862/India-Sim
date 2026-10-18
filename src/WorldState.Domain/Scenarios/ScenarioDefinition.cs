using System.Text.Json.Serialization;

namespace WorldState.Domain.Scenarios;

public record ScenarioDefinition(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("startDate")] DateTime StartDate,
    [property: JsonPropertyName("defaultCountryId")] string DefaultCountryId,
    [property: JsonPropertyName("supportedCountryIds")] IReadOnlyList<string> SupportedCountryIds
)
{
    public static ScenarioDefinition CreateDefault() =>
        new(
            Id: "modern_2026",
            Name: "Modern Era 2026",
            Description: "Contemporary geopolitical stage at the start of 2026.",
            StartDate: new DateTime(2026, 1, 1),
            DefaultCountryId: "IND",
            SupportedCountryIds: new[] { "IND", "USA", "CHN", "RUS", "GBR", "PAK" }
        );
}
