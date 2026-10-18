using WorldState.Domain.Models;
using WorldState.Domain.Services;
using Xunit;

namespace WorldState.Domain.Tests;

public class SaveLoadRoundTripTests
{
    [Fact]
    public void SerializeAndDeserialize_ProducesEqualDomainState()
    {
        // Arrange
        var service = new SaveLoadService();
        var originalSession = GameSession.CreateNewGame("IND");
        var india = originalSession.GetPlayerCountry()!;

        // Modify some state
        india.Treasury = 75_420_000_000m;
        india.IncomeTaxRate = 0.22;
        india.Ministries[MinistryType.Defense] = new MinistryState(
            MinistryType.Defense,
            new Minister("DEF-01", "Marshal R. Sen", 0.95, 450_000m),
            FundingLevel.Maximum
        );
        originalSession.Clock.CurrentDate = new DateOnly(2027, 4, 15);
        originalSession.Clock.Speed = ClockSpeed.Fast;

        // Act
        string json = service.SerializeSession(originalSession);
        var restoredSession = service.DeserializeSession(json);

        // Assert
        Assert.NotNull(restoredSession);
        Assert.Equal(originalSession.SessionId, restoredSession.SessionId);
        Assert.Equal(originalSession.SchemaVersion, restoredSession.SchemaVersion);
        Assert.Equal(originalSession.PlayerCountryId, restoredSession.PlayerCountryId);
        Assert.Equal(originalSession.Clock.CurrentDate, restoredSession.Clock.CurrentDate);
        Assert.Equal(originalSession.Clock.Speed, restoredSession.Clock.Speed);

        var restoredIndia = restoredSession.GetPlayerCountry();
        Assert.NotNull(restoredIndia);
        Assert.Equal(india.Id, restoredIndia.Id);
        Assert.Equal(india.Treasury, restoredIndia.Treasury);
        Assert.Equal(india.IncomeTaxRate, restoredIndia.IncomeTaxRate);
        Assert.Equal(india.Population, restoredIndia.Population);
        Assert.Equal(
            india.Ministries[MinistryType.Defense].ActiveMinister?.Name,
            restoredIndia.Ministries[MinistryType.Defense].ActiveMinister?.Name
        );
        Assert.Equal(
            india.Ministries[MinistryType.Defense].Funding,
            restoredIndia.Ministries[MinistryType.Defense].Funding
        );
    }
}
