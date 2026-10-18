using WorldState.Domain.Commands;
using WorldState.Domain.Models;
using Xunit;

namespace WorldState.Domain.Tests;

public class CommandValidationTests
{
    [Fact]
    public void AppointMinister_WithSufficientFunds_SucceedsAndDeductsHiringFee()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var country = session.GetPlayerCountry()!;
        decimal startingTreasury = country.Treasury;
        decimal fee = 15_000_000m;
        var candidate = new Minister("M-01", "Dr. A. Sharma", 0.85, 250_000m);

        var command = new AppointMinisterCommand("IND", MinistryType.Health, candidate, fee);

        // Act
        var result = command.Execute(session);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorCode);
        Assert.Equal(startingTreasury - fee, country.Treasury);
        Assert.Equal(candidate, country.Ministries[MinistryType.Health].ActiveMinister);
    }

    [Fact]
    public void AppointMinister_WithInsufficientFunds_FailsWithoutMutatingState()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var country = session.GetPlayerCountry()!;
        country.Treasury = 5_000_000m; // Only $5M
        decimal fee = 20_000_000m; // Requires $20M
        var candidate = new Minister("M-02", "Gen. V. Singh", 0.90, 500_000m);

        var command = new AppointMinisterCommand("IND", MinistryType.Defense, candidate, fee);

        // Act
        var result = command.Execute(session);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("InsufficientFunds", result.ErrorCode);
        Assert.Equal(5_000_000m, country.Treasury); // Unchanged
        Assert.Null(country.Ministries[MinistryType.Defense].ActiveMinister); // Unchanged
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.05)]
    [InlineData(-1.0)]
    public void AdjustTaxRate_WithInvalidRate_FailsValidation(double invalidRate)
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var command = new AdjustTaxRateCommand("IND", TaxType.IncomeTax, invalidRate);

        // Act
        var result = command.Execute(session);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidTaxRate", result.ErrorCode);
    }

    [Fact]
    public void AdjustTaxRate_WithValidRate_SucceedsAndUpdatesState()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var country = session.GetPlayerCountry()!;
        var command = new AdjustTaxRateCommand("IND", TaxType.IncomeTax, 0.28);

        // Act
        var result = command.Execute(session);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0.28, country.IncomeTaxRate);
    }
}
