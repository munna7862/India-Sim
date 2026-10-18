namespace WorldState.Domain.Models;

public class GameSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
    public int SchemaVersion { get; set; } = 1;
    public string PlayerCountryId { get; set; } = "IND";
    public SimulationClock Clock { get; set; } = new();
    public Dictionary<string, CountryState> Countries { get; set; } = new();

    public CountryState? GetPlayerCountry() =>
        Countries.TryGetValue(PlayerCountryId, out var state) ? state : null;

    public static GameSession CreateNewGame(string protagonistCountryId = "IND")
    {
        var session = new GameSession
        {
            PlayerCountryId = protagonistCountryId
        };

        var playerCountry = CountryState.CreateIndiaDefault();
        playerCountry.Id = protagonistCountryId;
        session.Countries[protagonistCountryId] = playerCountry;

        return session;
    }
}
