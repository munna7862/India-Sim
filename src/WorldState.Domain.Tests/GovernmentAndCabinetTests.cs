using WorldState.Domain.Commands;
using WorldState.Domain.Models;
using WorldState.Domain.Services;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class GovernmentAndCabinetTests
{
    [Fact]
    public void AppointMinister_WithSufficientFunds_SetsMinisterAndDeductsFee()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        decimal startingTreasury = country.Treasury;

        var candidate = new Minister("M_001", "Dr. Soumya Swaminathan", 0.92, 350_000m);
        var command = new AppointMinisterCommand("IND", MinistryType.Health, candidate, 20_000_000m);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(startingTreasury - 20_000_000m, country.Treasury);
        var health = country.Ministries[MinistryType.Health];
        Assert.NotNull(health.ActiveMinister);
        Assert.Equal("Dr. Soumya Swaminathan", health.ActiveMinister.Name);
        Assert.Equal(0.92, health.ActiveMinister.Competence);
        Assert.Contains(session.Events, e => e.Category == EventCategory.Government && e.Title == "Minister Appointed");
    }

    [Fact]
    public void AppointMinister_WithInsufficientFunds_FailsValidation()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        country.Treasury = 5_000_000m; // Only $5M

        var candidate = new Minister("M_001", "Candidate", 0.85, 300_000m);
        var command = new AppointMinisterCommand("IND", MinistryType.Health, candidate, 20_000_000m);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("InsufficientFunds", result.ErrorCode);
        Assert.Equal(5_000_000m, country.Treasury);
        Assert.Null(country.Ministries[MinistryType.Health].ActiveMinister);
    }

    [Fact]
    public void DismissMinister_WhenMinisterAppointed_ClearsMinisterAndLogsEvent()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        var candidate = new Minister("M_001", "Minister to Dismiss", 0.80, 250_000m);
        engine.Dispatch(new AppointMinisterCommand("IND", MinistryType.Finance, candidate, 10_000_000m));
        Assert.NotNull(country.Ministries[MinistryType.Finance].ActiveMinister);

        var dismissCommand = new DismissMinisterCommand("IND", MinistryType.Finance);

        // Act
        var result = engine.Dispatch(dismissCommand);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(country.Ministries[MinistryType.Finance].ActiveMinister);
        Assert.Contains(session.Events, e => e.Category == EventCategory.Government && e.Title == "Minister Dismissed");
    }

    [Fact]
    public void DismissMinister_WhenNoMinister_FailsSafely()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);

        var dismissCommand = new DismissMinisterCommand("IND", MinistryType.Defense);

        // Act
        var result = engine.Dispatch(dismissCommand);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("NoMinisterAppointed", result.ErrorCode);
    }

    [Fact]
    public void CalculateMinistryEfficiency_WithAndWithoutMinister_RespectsFundingLevel()
    {
        // Without minister (0.70 baseline) and Medium funding (1.0): 0.70 * 1.0 = 0.70 (70%)
        var vacantMedium = new MinistryState(MinistryType.Education, null, FundingLevel.Medium);
        Assert.Equal(0.70, vacantMedium.CalculateEfficiency(), 2);

        // Without minister and Zero funding: 0.70 * 0.0 = 0.0 (0%)
        var vacantZero = new MinistryState(MinistryType.Education, null, FundingLevel.Zero);
        Assert.Equal(0.0, vacantZero.CalculateEfficiency());

        // With expert minister (0.90) and Maximum funding (1.35): 0.90 * 1.35 = 1.0 (clamped 100%)
        var expert = new Minister("EXP", "Expert", 0.90, 400_000m);
        var fundedExpert = new MinistryState(MinistryType.Education, expert, FundingLevel.Maximum);
        Assert.Equal(1.0, fundedExpert.CalculateEfficiency(), 2);
    }

    [Fact]
    public void ChangeIdeology_WithSufficientFunds_UpdatesIdeologyAppliesModifiersAndSetsCooldown()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        decimal startingTreasury = country.Treasury;
        double startingStability = country.Stability;
        double startingApproval = country.ApprovalRating;

        var targetIdeology = IdeologyType.AuthoritarianRepublic;
        var def = IdeologyDefinition.Get(targetIdeology);
        var command = new ChangeIdeologyCommand("IND", targetIdeology);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(targetIdeology, country.Ideology);
        Assert.Equal(12, country.IdeologyCooldownMonths);
        Assert.Equal(startingTreasury - def.TransitionCost, country.Treasury);
        // Stability changed by -5.0 + 15.0 = +10.0
        Assert.Equal(Math.Clamp(startingStability + 10.0, 0.0, 100.0), country.Stability);
        // Approval changed by -5.0 - 10.0 = -15.0
        Assert.Equal(Math.Clamp(startingApproval - 15.0, 0.0, 100.0), country.ApprovalRating);
        Assert.Contains(session.Events, e => e.Category == EventCategory.Government && e.Title == "State Ideology Reformed");
    }

    [Fact]
    public void ChangeIdeology_WhenCooldownActive_FailsValidation()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        country.IdeologyCooldownMonths = 6;

        var command = new ChangeIdeologyCommand("IND", IdeologyType.Technocracy);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("IdeologyCooldownActive", result.ErrorCode);
        Assert.Equal(IdeologyType.ConstitutionalDemocracy, country.Ideology);
    }

    [Fact]
    public void ChangeIdeology_WhenSameIdeology_FailsValidation()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);

        var command = new ChangeIdeologyCommand("IND", IdeologyType.ConstitutionalDemocracy);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("AlreadyActiveIdeology", result.ErrorCode);
    }

    [Fact]
    public void ChangeIdeology_WithInsufficientFunds_FailsValidation()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        country.Treasury = 100_000_000m; // Only $100M, transition costs > $1B

        var command = new ChangeIdeologyCommand("IND", IdeologyType.DemocraticSocialism);

        // Act
        var result = engine.Dispatch(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("InsufficientFunds", result.ErrorCode);
        Assert.Equal(100_000_000m, country.Treasury);
    }

    [Fact]
    public void GovernmentViewModel_MapsCabinetAndComputesPayrollAndAveragesAccurately()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var country = session.GetPlayerCountry()!;

        country.Ministries[MinistryType.Health] = new MinistryState(
            MinistryType.Health,
            new Minister("M_HLTH", "Dr. Health", 0.90, 300_000m),
            FundingLevel.Maximum
        );

        // Act
        var vm = GovernmentViewModel.FromCountry(country);

        // Assert
        Assert.Equal("IND", vm.CountryId);
        Assert.Equal("Constitutional Democracy", vm.IdeologyDisplayName);
        Assert.True(vm.CanChangeIdeology);
        Assert.Equal(13, vm.Ministries.Count);
        Assert.Equal(300_000m, vm.TotalMonthlyPayroll);
        Assert.True(vm.TotalMonthlyOperationalCost > 0m);
        Assert.True(vm.AverageCabinetEfficiency > 0.0);
    }

    [Fact]
    public void MinisterRosterService_ReturnsCuratedCandidatesForEachMinistry()
    {
        foreach (MinistryType type in Enum.GetValues<MinistryType>())
        {
            var candidates = MinisterRosterService.GetCandidates(type);
            Assert.NotEmpty(candidates);
            Assert.All(candidates, c =>
            {
                Assert.Equal(type, c.Ministry);
                Assert.True(c.Competence >= 0.5 && c.Competence <= 1.0);
                Assert.True(c.HiringFee > 0m);
                Assert.True(c.MonthlySalary > 0m);
                Assert.False(string.IsNullOrWhiteSpace(c.Name));
            });
        }
    }
}
