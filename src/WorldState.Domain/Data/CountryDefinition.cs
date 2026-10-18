using System.Text.Json.Serialization;
using WorldState.Domain.Models;

namespace WorldState.Domain.Data;

public record CountryDefinition(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("capital")] string Capital,
    [property: JsonPropertyName("isPlayable")] bool IsPlayable,
    [property: JsonPropertyName("startingTreasury")] decimal StartingTreasury,
    [property: JsonPropertyName("startingPopulation")] long StartingPopulation,
    [property: JsonPropertyName("startingApproval")] double StartingApproval,
    [property: JsonPropertyName("startingQualityOfLiving")] double StartingQualityOfLiving,
    [property: JsonPropertyName("startingStability")] double StartingStability,
    [property: JsonPropertyName("incomeTaxRate")] double IncomeTaxRate,
    [property: JsonPropertyName("corporateTaxRate")] double CorporateTaxRate,
    [property: JsonPropertyName("vatRate")] double VatRate,
    [property: JsonPropertyName("facilities")] Dictionary<string, int> Facilities
)
{
    public CountryState ToCountryState()
    {
        var state = new CountryState
        {
            Id = Id,
            Name = Name,
            Treasury = StartingTreasury,
            Population = StartingPopulation,
            ApprovalRating = StartingApproval,
            QualityOfLiving = StartingQualityOfLiving,
            Stability = StartingStability,
            IncomeTaxRate = IncomeTaxRate,
            CorporateTaxRate = CorporateTaxRate,
            VatRate = VatRate,
            Facilities = new Dictionary<string, int>(Facilities)
        };

        foreach (MinistryType ministry in Enum.GetValues<MinistryType>())
        {
            state.Ministries[ministry] = new MinistryState(ministry, null, FundingLevel.Medium);
        }

        int barracks = Facilities.GetValueOrDefault("Barracks", 10);
        state.MilitaryUnits = new Dictionary<string, int>
        {
            ["infantry_division"] = barracks * 15,
            ["main_battle_tank"] = barracks * 3,
            ["artillery_corps"] = barracks * 2,
            ["mechanized_infantry"] = barracks * 2,
            ["multirole_fighter"] = barracks * 2,
            ["attack_helicopter"] = barracks * 1,
            ["air_defense_system"] = barracks * 1,
            ["guided_missile_destroyer"] = Math.Max(1, barracks / 2)
        };

        return state;
    }
}

