using WorldState.Domain.Commands;
using WorldState.Domain.Economy;
using WorldState.Domain.Models;
using WorldState.Domain.Services;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class EconomyBudgetTests
{
    [Fact]
    public void AdjustTaxRate_ValidRates_UpdatesCountryStateAndCalculatesProjections()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        var result = engine.Dispatch(new AdjustTaxRateCommand("IND", TaxType.IncomeTax, 0.28));
        Assert.True(result.IsSuccess);
        Assert.Equal(0.28, country.IncomeTaxRate, 4);

        result = engine.Dispatch(new AdjustTaxRateCommand("IND", TaxType.CorporateTax, 0.32));
        Assert.True(result.IsSuccess);
        Assert.Equal(0.32, country.CorporateTaxRate, 4);

        result = engine.Dispatch(new AdjustTaxRateCommand("IND", TaxType.Vat, 0.18));
        Assert.True(result.IsSuccess);
        Assert.Equal(0.18, country.VatRate, 4);

        var vm = EconomyViewModel.FromCountry(country, engine.PolicyRegistry);
        Assert.Equal(0.28, vm.IncomeTaxRate, 4);
        Assert.Equal(0.32, vm.CorporateTaxRate, 4);
        Assert.Equal(0.18, vm.VatRate, 4);
        Assert.True(vm.ProjectedGrossRevenue > 0);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-0.5)]
    [InlineData(1.05)]
    [InlineData(2.0)]
    public void AdjustTaxRate_InvalidRates_FailsValidationWithoutMutatingState(double invalidRate)
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        double initialRate = country.IncomeTaxRate;

        var result = engine.Dispatch(new AdjustTaxRateCommand("IND", TaxType.IncomeTax, invalidRate));
        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidTaxRate", result.ErrorCode);
        Assert.Equal(initialRate, country.IncomeTaxRate);
    }

    [Fact]
    public void TakeLoanAndRepayLoan_EnforcesCeilingsAndDeductions()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        decimal startingTreasury = country.Treasury;
        decimal startingDebt = country.Debt;
        decimal loanAmount = 10_000_000_000m; // $10B

        // 1. Take valid loan
        var takeResult = engine.Dispatch(new TakeLoanCommand("IND", loanAmount));
        Assert.True(takeResult.IsSuccess);
        Assert.Equal(startingTreasury + loanAmount, country.Treasury);
        Assert.Equal(startingDebt + loanAmount, country.Debt);

        // 2. Exceed national debt ceiling
        decimal excessiveLoan = 500_000_000_000m; // $500B
        var exceedResult = engine.Dispatch(new TakeLoanCommand("IND", excessiveLoan));
        Assert.False(exceedResult.IsSuccess);
        Assert.Equal("DebtCeilingExceeded", exceedResult.ErrorCode);

        // 3. Repay loan partially
        decimal repayAmount = 4_000_000_000m; // $4B
        var repayResult = engine.Dispatch(new RepayLoanCommand("IND", repayAmount));
        Assert.True(repayResult.IsSuccess);
        Assert.Equal(startingTreasury + loanAmount - repayAmount, country.Treasury);
        Assert.Equal(startingDebt + loanAmount - repayAmount, country.Debt);

        // 4. Repay loan with amount greater than outstanding debt clamps safely
        decimal overpayment = 20_000_000_000m;
        var fullRepayResult = engine.Dispatch(new RepayLoanCommand("IND", overpayment));
        Assert.True(fullRepayResult.IsSuccess);
        Assert.Equal(0m, country.Debt);

        // 5. Repay when debt is zero fails gracefully
        var zeroDebtRepay = engine.Dispatch(new RepayLoanCommand("IND", 1_000_000m));
        Assert.False(zeroDebtRepay.IsSuccess);
        Assert.Equal("NoOutstandingDebt", zeroDebtRepay.ErrorCode);
    }

    [Fact]
    public void ConstructFacility_NewFacilityTypes_ConsumesResourcesAndIncrementsCounts()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        int initialOilWells = country.Facilities.GetValueOrDefault("OilWells", 0);
        int initialUraniumMines = country.Facilities.GetValueOrDefault("UraniumMines", 0);
        decimal initialTreasury = country.Treasury;
        double initialIron = country.Stockpiles.Iron;

        // 1. Construct OilWells
        var oilResult = engine.Dispatch(new ConstructFacilityCommand("IND", "OilWells", 2));
        Assert.True(oilResult.IsSuccess);
        Assert.Equal(initialOilWells + 2, country.Facilities["OilWells"]);
        Assert.Equal(initialTreasury - 700_000_000m, country.Treasury);
        Assert.Equal(initialIron - 3_000.0, country.Stockpiles.Iron);

        // 2. Construct UraniumMines
        var uraniumResult = engine.Dispatch(new ConstructFacilityCommand("IND", "UraniumMines", 1));
        Assert.True(uraniumResult.IsSuccess);
        Assert.Equal(initialUraniumMines + 1, country.Facilities["UraniumMines"]);

        // 3. Construct with insufficient iron fails without mutating state
        country.Stockpiles = country.Stockpiles with { Iron = 10.0 };
        var failResult = engine.Dispatch(new ConstructFacilityCommand("IND", "Mines", 5));
        Assert.False(failResult.IsSuccess);
        Assert.Equal("InsufficientResources", failResult.ErrorCode);
    }

    [Fact]
    public void EconomicPolicies_EnactAndRevoke_AppliesModifiersAndUpkeep()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        decimal initialTreasury = country.Treasury;
        double initialApproval = country.ApprovalRating;

        // 1. Enact Agricultural Subsidies
        var enactResult = engine.Dispatch(new EnactEconomicPolicyCommand("IND", "agricultural_subsidies"));
        Assert.True(enactResult.IsSuccess);
        Assert.Contains("agricultural_subsidies", country.ActivePolicies);
        Assert.Equal(initialTreasury - 500_000_000m, country.Treasury);
        Assert.Equal(initialApproval + 3.0, country.ApprovalRating);

        // 2. Cannot enact duplicate policy
        var dupResult = engine.Dispatch(new EnactEconomicPolicyCommand("IND", "agricultural_subsidies"));
        Assert.False(dupResult.IsSuccess);
        Assert.Equal("PolicyAlreadyActive", dupResult.ErrorCode);

        // 3. Enact unknown policy fails safely
        var unknownResult = engine.Dispatch(new EnactEconomicPolicyCommand("IND", "non_existent_policy"));
        Assert.False(unknownResult.IsSuccess);
        Assert.Equal("PolicyNotFound", unknownResult.ErrorCode);

        // 4. Revoke active policy
        var revokeResult = engine.Dispatch(new RevokeEconomicPolicyCommand("IND", "agricultural_subsidies"));
        Assert.True(revokeResult.IsSuccess);
        Assert.DoesNotContain("agricultural_subsidies", country.ActivePolicies);

        // 5. Revoking inactive policy fails safely
        var revokeInactive = engine.Dispatch(new RevokeEconomicPolicyCommand("IND", "agricultural_subsidies"));
        Assert.False(revokeInactive.IsSuccess);
        Assert.Equal("PolicyNotActive", revokeInactive.ErrorCode);
    }

    [Fact]
    public void MonthlyLedger_GeneratesItemizedReport_AndUpdatesCreditRating()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        // Trigger monthly ledger execution
        var ledgerCmd = new ExecuteMonthlyLedgerCommand("IND");
        var result = ledgerCmd.Execute(session);

        Assert.True(result.IsSuccess);
        Assert.NotNull(country.LastBudgetReport);
        Assert.Single(country.BudgetHistory);

        var report = country.LastBudgetReport;
        Assert.Equal("IND", report.CountryId);
        Assert.True(report.GrossRevenue > 0);
        Assert.True(report.IncomeTaxRevenue > 0);
        Assert.True(report.CorporateTaxRevenue > 0);
        Assert.True(report.VatRevenue > 0);
        Assert.True(report.GrossExpenses > 0);
        Assert.True(report.MinistryOperationalExpenses > 0);
        Assert.True(report.CivilAdministrationExpenses > 0);
        Assert.Equal(report.GrossRevenue - report.GrossExpenses, report.NetBalance);
        Assert.Equal("AAA", report.CreditRating);
        Assert.Equal("AAA", country.CreditRating);
    }

    [Fact]
    public void SovereignDebtDistress_LowTreasuryAndHighDebt_DowngradesRatingAndEmitsAlert()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        // Place country in severe fiscal crisis: $3 Trillion debt vs $1.68T GDP
        country.Treasury = -15_000_000_000m; // -$15B
        country.Debt = 3_000_000_000_000m;    // $3,000B ($3T)

        var ledgerCmd = new ExecuteMonthlyLedgerCommand("IND");
        ledgerCmd.Execute(session);

        Assert.Equal("CCC", country.CreditRating);
        Assert.Contains(session.Events, e => e.Title.Contains("Sovereign Debt Distress"));
    }

    [Fact]
    public void LongRunEconomicSimulation_MultiMonthProgression_RemainsStableAndDeterministic()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        // Enact industrial modernization
        engine.Dispatch(new EnactEconomicPolicyCommand("IND", "industrial_modernization"));

        // Advance 12 months (each month is ~30 days)
        for (int month = 0; month < 12; month++)
        {
            session.Clock.AdvanceDays(30);
            engine.Tick(0.0);
        }

        Assert.True(country.BudgetHistory.Count >= 10);
        Assert.True(country.Treasury > 0);
        Assert.True(country.Stockpiles.Food > 0);
        Assert.True(country.Stockpiles.Energy > 0);
        Assert.True(country.Stockpiles.Iron > 0);
        Assert.True(country.Stockpiles.Oil > 0);
        Assert.True(country.Stockpiles.ConsumerGoods > 0);
        Assert.InRange(country.ApprovalRating, 0.0, 100.0);
        Assert.InRange(country.QualityOfLiving, 0.0, 100.0);
        Assert.InRange(country.Stability, 0.0, 100.0);
    }

    [Fact]
    public void SaveLoadRoundTrip_PreservesAllEconomicStateAndHistory()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        // Modify economic parameters
        engine.Dispatch(new AdjustTaxRateCommand("IND", TaxType.IncomeTax, 0.22));
        engine.Dispatch(new TakeLoanCommand("IND", 5_000_000_000m));
        engine.Dispatch(new ConstructFacilityCommand("IND", "OilWells", 3));
        engine.Dispatch(new EnactEconomicPolicyCommand("IND", "green_energy_transition"));

        // Run monthly rollover
        session.Clock.AdvanceDays(31);
        engine.Tick(0.0);

        // Serialize and deserialize
        var saveService = new SaveLoadService();
        string json = saveService.SerializeSession(session);
        var restored = saveService.DeserializeSession(json);

        Assert.NotNull(restored);
        var restoredCountry = restored.GetPlayerCountry();
        Assert.NotNull(restoredCountry);

        Assert.Equal(country.IncomeTaxRate, restoredCountry.IncomeTaxRate, 4);
        Assert.Equal(country.Treasury, restoredCountry.Treasury);
        Assert.Equal(country.Debt, restoredCountry.Debt);
        Assert.Equal(country.CreditRating, restoredCountry.CreditRating);
        Assert.Equal(country.Facilities["OilWells"], restoredCountry.Facilities["OilWells"]);
        Assert.Contains("green_energy_transition", restoredCountry.ActivePolicies);
        Assert.NotEmpty(restoredCountry.BudgetHistory);
        Assert.NotNull(restoredCountry.LastBudgetReport);
        Assert.Equal(country.LastBudgetReport!.NetBalance, restoredCountry.LastBudgetReport!.NetBalance);
    }

    [Theory]
    [InlineData("IND")]
    [InlineData("USA")]
    [InlineData("CHN")]
    [InlineData("RUS")]
    [InlineData("GBR")]
    [InlineData("PAK")]
    public void MultiCountry_EconomicInitialization_CalculatesValidGdpAndLedger(string countryId)
    {
        var session = GameSession.CreateNewGame(countryId);
        var engine = new SimulationEngine(session);
        var country = session.Countries[countryId];

        decimal monthlyGdp = country.CalculateMonthlyGdp();
        Assert.True(monthlyGdp > 0);
        Assert.Equal("AAA", country.CalculateCreditRating());

        var ledgerCmd = new ExecuteMonthlyLedgerCommand(countryId);
        var result = ledgerCmd.Execute(session);
        Assert.True(result.IsSuccess);
        Assert.NotNull(country.LastBudgetReport);
        Assert.True(country.LastBudgetReport.GrossRevenue > 0);
        Assert.True(country.LastBudgetReport.GrossExpenses > 0);
    }

    [Fact]
    public void BankruptcyAndCreditRating_DefaultsToRatingD_WhenTreasuryCollapses()
    {
        var session = GameSession.CreateNewGame("IND");
        var country = session.GetPlayerCountry()!;

        country.Treasury = -60_000_000_000m; // -$60B
        string rating = country.CalculateCreditRating();
        country.CreditRating = rating;
        Assert.Equal("D", rating);
        Assert.Equal(0.0300m, country.GetMonthlyInterestRate());
    }

    [Fact]
    public void FiscalAusterity_ReducesNetMonthlyExpenditure()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;

        // First baseline ledger
        new ExecuteMonthlyLedgerCommand("IND").Execute(session);
        decimal baselineExpenses = country.LastBudgetReport!.GrossExpenses;

        // Enact austerity (monthly upkeep is -$300M, reducing total expenses)
        engine.Dispatch(new EnactEconomicPolicyCommand("IND", "fiscal_austerity_program"));
        new ExecuteMonthlyLedgerCommand("IND").Execute(session);
        decimal austerityExpenses = country.LastBudgetReport!.GrossExpenses;

        Assert.True(austerityExpenses < baselineExpenses);
    }

    [Fact]
    public void StartTimedConstruction_NewFacility_EnqueuesOperationAndCompletes()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var country = session.GetPlayerCountry()!;
        int initialOilWells = country.Facilities.GetValueOrDefault("OilWells", 0);

        var startCmd = new StartTimedConstructionCommand("IND", "OilWells", 2, 30);
        var result = engine.Dispatch(startCmd);
        Assert.True(result.IsSuccess);
        Assert.Single(session.TimedOperations);

        // Advance 30 days
        session.Clock.AdvanceDays(30);
        engine.Tick(0.0);

        Assert.Empty(session.TimedOperations);
        Assert.Equal(initialOilWells + 2, country.Facilities["OilWells"]);
    }
}
