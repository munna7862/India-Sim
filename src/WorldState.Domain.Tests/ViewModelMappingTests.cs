using WorldState.Domain.Models;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class ViewModelMappingTests
{
    [Theory]
    [InlineData(50_000_000_000, "$50.0B")]
    [InlineData(250_000_000, "$250.0M")]
    [InlineData(15_000, "$15K")]
    [InlineData(500, "$500")]
    [InlineData(-1_500_000, "-$1.5M")]
    public void FormatCurrency_FormatsCorrectly(decimal amount, string expected)
    {
        string formatted = HudViewModel.FormatCurrency(amount);
        Assert.Equal(expected, formatted);
    }

    [Theory]
    [InlineData(1_400_000_000, "1.40B")]
    [InlineData(50_000_000, "50.0M")]
    [InlineData(25_000, "25K")]
    public void FormatNumber_FormatsCorrectly(long number, string expected)
    {
        string formatted = HudViewModel.FormatNumber(number);
        Assert.Equal(expected, formatted);
    }

    [Fact]
    public void HudViewModel_FromSession_MapsAllMetricsAccurately()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        session.Clock.CurrentDate = new DateOnly(2026, 3, 15);
        session.Clock.Speed = ClockSpeed.Fast;

        // Act
        var vm = HudViewModel.FromSession(session);

        // Assert
        Assert.Equal("15 Mar 2026", vm.DateText);
        Assert.Equal("2x", vm.SpeedText);
        Assert.False(vm.IsPaused);
        Assert.Equal("$50.0B", vm.TreasuryText);
        Assert.False(vm.IsDeficit);
        Assert.Equal("1.40B", vm.PopulationText);
        Assert.Equal("65.0%", vm.ApprovalText);
        Assert.Equal("55.0%", vm.LivingStandardText);
    }

    [Fact]
    public void ActionPromptViewModel_CalculatesAffordabilityAndProjectedTreasury()
    {
        // Arrange & Act
        var prompt = ActionPromptViewModel.Create(
            title: "Appoint Defense Minister",
            description: "Hire Gen. Vikram Singh",
            oneTimeCost: 15_000_000m,
            currentTreasury: 50_000_000_000m,
            prerequisites: new[] { "Defense Ministry Vacant" }
        );

        // Assert
        Assert.True(prompt.CanAfford);
        Assert.True(prompt.AllPrerequisitesMet);
        Assert.Equal("$15.0M", prompt.OneTimeCostFormatted);
        Assert.Equal("$50.0B", prompt.CurrentTreasuryFormatted);
        Assert.Equal(49_985_000_000m, prompt.ProjectedTreasury);
    }

    [Fact]
    public void MinistryCardViewModel_WhenVacant_DisplaysVacantPosition()
    {
        // Arrange
        var state = new MinistryState(MinistryType.Health, null, FundingLevel.Medium);

        // Act
        var vm = MinistryCardViewModel.FromState(state);

        // Assert
        Assert.Equal("Ministry of Health", vm.DisplayName);
        Assert.Equal("Position Vacant", vm.MinisterName);
        Assert.Equal("N/A", vm.CompetencePercent);
        Assert.Equal("$0 / mo", vm.MonthlySalaryFormatted);
        Assert.Equal("$50.0M / mo", vm.OperationalCostFormatted);
    }

    [Fact]
    public void MinistryCardViewModel_WhenAppointed_DisplaysMinisterDetails()
    {
        // Arrange
        var minister = new Minister("M-01", "Dr. A. Sharma", 0.85, 250_000m);
        var state = new MinistryState(MinistryType.Health, minister, FundingLevel.High);

        // Act
        var vm = MinistryCardViewModel.FromState(state);

        // Assert
        Assert.Equal("Dr. A. Sharma", vm.MinisterName);
        Assert.Equal("85%", vm.CompetencePercent);
        Assert.Equal("$250K / mo", vm.MonthlySalaryFormatted);
        Assert.Equal("$75.0M / mo", vm.OperationalCostFormatted);
    }
}
