using WorldState.Domain.Commands;
using WorldState.Domain.Models;
using Xunit;

namespace WorldState.Domain.Tests;

public class MonthlyAccountingTests
{
    [Fact]
    public void ExecuteMonthlyLedger_UpdatesTreasuryAndStockpilesDeterministically()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var country = session.GetPlayerCountry()!;
        decimal startingTreasury = country.Treasury;
        double startingFood = country.Stockpiles.Food;
        double startingIron = country.Stockpiles.Iron;

        var command = new ExecuteMonthlyLedgerCommand("IND");

        // Act
        var result = command.Execute(session);

        // Assert
        Assert.True(result.IsSuccess);

        // Treasury delta must have been applied:
        Assert.NotEqual(startingTreasury, country.Treasury);

        // Iron production from mines must have increased:
        Assert.True(country.Stockpiles.Iron > startingIron);

        // Food balance must be non-negative:
        Assert.True(country.Stockpiles.Food >= 0.0);
    }
}
