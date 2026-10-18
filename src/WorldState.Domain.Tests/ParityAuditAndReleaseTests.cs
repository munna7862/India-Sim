using System.Reflection;
using WorldState.Domain.Commands;
using WorldState.Domain.Data;
using WorldState.Domain.Economy;
using WorldState.Domain.Models;
using WorldState.Domain.Navigation;
using WorldState.Domain.Services;
using WorldState.Domain.Settings;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

/// <summary>
/// Phase 09 Sprint 09-01 Feature Parity Audit &amp; Bug Bash automated test suite.
/// Stress-tests complete game loops, extreme boundary conditions, multi-year progression,
/// sovereign debt distress/recovery, wartime combat, persistence fault tolerance, and offline air-gap invariants.
/// </summary>
public class ParityAuditAndReleaseTests
{
    [Fact]
    public void ParityAudit_MultiYearSimulation_MaintainsEconomicAndTaskInvariants()
    {
        // Setup a 3-year (36 monthly cycles) simulation run for India
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        long initialPop = player.Population;

        // Perform initial investments: expand farms and factories
        var constructFarmCmd = new ConstructFacilityCommand("IND", "Farms", 10);
        Assert.True(engine.Dispatch(constructFarmCmd).IsSuccess);

        var constructPowerCmd = new ConstructFacilityCommand("IND", "PowerPlants", 5);
        Assert.True(engine.Dispatch(constructPowerCmd).IsSuccess);

        // Adjust income tax to moderate 18%
        var taxCmd = new AdjustTaxRateCommand("IND", TaxType.IncomeTax, 0.18);
        Assert.True(engine.Dispatch(taxCmd).IsSuccess);

        // Run 36 monthly cycles
        for (int month = 1; month <= 36; month++)
        {
            // Advance clock by 30 days and tick engine
            session.Clock.AdvanceDays(30);
            engine.Tick(0.0);

            // Execute monthly ledger
            var ledgerResult = engine.Dispatch(new ExecuteMonthlyLedgerCommand("IND"));
            Assert.True(ledgerResult.IsSuccess, $"Month {month} ledger failed: {ledgerResult.ErrorCode}");

            // Periodic task & achievement evaluation
            engine.EvaluateTasks();
            engine.EvaluateAchievements();

            // Verify domain invariants
            Assert.True(player.Population >= initialPop, $"Population shrank unexpectedly at month {month}");
            Assert.True(player.Stability >= 0.0 && player.Stability <= 100.0, "Stability broke 0-100 bounds");
            Assert.True(session.Clock.CurrentDate.Year >= 2026);
        }

        // Ledger history must contain 24 records (rolling 24-month window)
        Assert.Equal(24, player.BudgetHistory.Count);

        // Verify tasks are present and status is valid
        var tasks = session.Tasks.Values.ToList();
        Assert.NotEmpty(tasks);
        foreach (var task in tasks)
        {
            Assert.True(Enum.IsDefined(typeof(GameTaskStatus), task.Status));
        }
    }

    [Fact]
    public void ParityAudit_SovereignDebtCrisisAndAusterityRecovery_RestoresSolvency()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        // 1. Initial rating is AAA
        Assert.Equal("AAA", player.CreditRating);

        // 2. Borrow up to standard loan
        var loanCmd = new TakeLoanCommand("IND", 50_000_000_000m);
        var loanRes = engine.Dispatch(loanCmd);
        Assert.True(loanRes.IsSuccess);
        Assert.Equal(50_000_000_000m, player.Debt);

        // Attempting to borrow above debt ceiling must be rejected
        decimal excessiveLoan = 500_000_000_000m;
        var excessiveCmd = new TakeLoanCommand("IND", excessiveLoan);
        var excessiveRes = engine.Dispatch(excessiveCmd);
        Assert.False(excessiveRes.IsSuccess);
        Assert.Equal("DebtCeilingExceeded", excessiveRes.ErrorCode);

        // 3. Emergency austerity measures: enact austerity policy and raise taxes
        var austerityCmd = new EnactEconomicPolicyCommand("IND", "fiscal_austerity_program");
        var austerityRes = engine.Dispatch(austerityCmd);
        Assert.True(austerityRes.IsSuccess);

        var taxHike = new AdjustTaxRateCommand("IND", TaxType.CorporateTax, 0.35);
        Assert.True(engine.Dispatch(taxHike).IsSuccess);

        // 4. Pay off debt in tranches
        var repayCmd = new RepayLoanCommand("IND", 50_000_000_000m);
        var repayRes = engine.Dispatch(repayCmd);
        Assert.True(repayRes.IsSuccess);
        Assert.Equal(0m, player.Debt);

        // Recalculate monthly ledger to update rating
        var ledgerCmd = new ExecuteMonthlyLedgerCommand("IND");
        ledgerCmd.Execute(session);
        Assert.Equal("AAA", player.CreditRating);
    }

    [Fact]
    public void ParityAudit_TotalWarAndDiplomaticResolution_ExecutesFullConflictLifecycle()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;
        var pak = session.Countries["PAK"];

        // 1. Establish peaceful diplomatic baseline: sign trade contract
        var trade = new TradeContract("TC_AUDIT", "IND", "PAK", "Food", 100, 10_000_000m, session.Clock.CurrentDate);
        player.GetRelationsWith("PAK").ActiveTradeContracts.Add(trade);
        pak.GetRelationsWith("IND").ActiveTradeContracts.Add(trade);
        Assert.NotEmpty(player.GetRelationsWith("PAK").ActiveTradeContracts);

        // Set up active Non-Aggression Pact
        player.GetRelationsWith("PAK").NonAggressionPactUntil = session.Clock.CurrentDate.AddMonths(24);
        pak.GetRelationsWith("IND").NonAggressionPactUntil = session.Clock.CurrentDate.AddMonths(24);

        // 2. War declaration blocked while NAP is active
        var illegalWar = new DeclareWarCommand("IND", "PAK");
        var illegalRes = engine.Dispatch(illegalWar);
        Assert.False(illegalRes.IsSuccess);
        Assert.Equal("TreatyViolationBlocked", illegalRes.ErrorCode);

        // Clear NAP
        player.GetRelationsWith("PAK").NonAggressionPactUntil = null;
        pak.GetRelationsWith("IND").NonAggressionPactUntil = null;

        // 3. Declare war -> should terminate active trade contracts automatically
        var warCmd = new DeclareWarCommand("IND", "PAK");
        var warRes = engine.Dispatch(warCmd);
        Assert.True(warRes.IsSuccess);

        Assert.True(player.GetRelationsWith("PAK").IsAtWar);
        Assert.Empty(player.GetRelationsWith("PAK").ActiveTradeContracts);

        // 4. Prepare armed forces: recruit tanks and conduct army drills
        var recruitCmd = new RecruitMilitaryUnitsCommand("IND", "main_battle_tank", 5);
        var recruitRes = engine.Dispatch(recruitCmd);
        Assert.True(recruitRes.IsSuccess);

        var drillCmd = new ConductArmyDrillsCommand("IND");
        var drillRes = engine.Dispatch(drillCmd);
        Assert.True(drillRes.IsSuccess);
        Assert.True(session.Cooldowns.IsOnCooldown("IND", "ArmyDrills", session.Clock.CurrentDate));

        // 5. Combat resolution
        int indTotalBefore = player.GetTotalMilitaryUnits();
        int pakTotalBefore = pak.GetTotalMilitaryUnits();

        var combatCmd = new ResolveCombatCommand("IND", "PAK");
        var combatRes = engine.Dispatch(combatCmd);
        Assert.True(combatRes.IsSuccess);
        Assert.NotNull(combatCmd.LastReport);

        Assert.True(player.GetTotalMilitaryUnits() < indTotalBefore);
        Assert.True(pak.GetTotalMilitaryUnits() < pakTotalBefore);

        // 6. Sign peace treaty -> ends war state
        var peaceCmd = new SignPeaceTreatyCommand("IND", "PAK");
        var peaceRes = engine.Dispatch(peaceCmd);
        Assert.True(peaceRes.IsSuccess);
        Assert.False(player.GetRelationsWith("PAK").IsAtWar);
        Assert.False(pak.GetRelationsWith("IND").IsAtWar);
    }

    [Fact]
    public void ParityAudit_SaveLoadResilience_CorruptFilesAndRecovery()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "Rashtra_Audit_" + Guid.NewGuid().ToString("N"));
        try
        {
            var saveManager = new SaveManager(tempDir);
            var serializer = new SaveLoadService();
            var session = GameSession.CreateNewGame("IND");

            // 1. Normal save to slot 1
            var saveRes = saveManager.Save("slot1", session);
            Assert.True(saveRes.IsSuccess);
            Assert.True(File.Exists(saveManager.GetSlotFilePath("slot1")));

            // 2. Overwrite creates .bak backup
            session.GetPlayerCountry()!.Treasury += 100_000_000m;
            var overwriteRes = saveManager.Save("slot1", session);
            Assert.True(overwriteRes.IsSuccess);
            Assert.True(File.Exists(saveManager.GetSlotBackupPath("slot1")));

            // 3. Corrupt target file with invalid JSON
            File.WriteAllText(saveManager.GetSlotFilePath("slot1"), "{ INVALID_JSON_DATA_!!!");
            var corruptLoadRes = saveManager.Load("slot1");
            Assert.False(corruptLoadRes.IsSuccess);
            Assert.Equal(SaveErrorCode.CorruptedSaveData, corruptLoadRes.ErrorCode);

            // 4. Missing file returns FileNotFound
            var missingRes = saveManager.Load("non_existent_slot");
            Assert.False(missingRes.IsSuccess);
            Assert.Equal(SaveErrorCode.FileNotFound, missingRes.ErrorCode);

            // 5. Future schema version test
            string futureJson = serializer.SerializeSession(session)
                .Replace("\"SchemaVersion\": 1", "\"SchemaVersion\": 999")
                .Replace("\"schemaVersion\": 1", "\"schemaVersion\": 999");
            File.WriteAllText(saveManager.GetSlotFilePath("slot_future"), futureJson);

            var futureRes = saveManager.Load("slot_future");
            Assert.False(futureRes.IsSuccess);
            Assert.Equal(SaveErrorCode.SaveTooNew, futureRes.ErrorCode);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void ParityAudit_SecurityAndDomesticAffairs_ClampingAndBoundaries()
    {
        var session = GameSession.CreateNewGame("IND");
        var engine = new SimulationEngine(session);
        var player = session.GetPlayerCountry()!;

        // 1. Presidential protection level
        var validProtCmd = new FundPresidentialProtectionCommand("IND", FundingLevel.High);
        Assert.True(engine.Dispatch(validProtCmd).IsSuccess);
        Assert.Equal(FundingLevel.High, player.PresidentialProtectionLevel);

        var maxProtCmd = new FundPresidentialProtectionCommand("IND", FundingLevel.Maximum);
        Assert.True(engine.Dispatch(maxProtCmd).IsSuccess);
        Assert.Equal(FundingLevel.Maximum, player.PresidentialProtectionLevel);

        var lowProtCmd = new FundPresidentialProtectionCommand("IND", FundingLevel.Low);
        Assert.True(engine.Dispatch(lowProtCmd).IsSuccess);
        Assert.Equal(FundingLevel.Low, player.PresidentialProtectionLevel);

        // 2. Anti-Corruption Campaign
        player.CorruptionIndex = 40.0;
        var antiCorrCmd = new LaunchAntiCorruptionCampaignCommand("IND");
        var antiCorrRes = engine.Dispatch(antiCorrCmd);
        Assert.True(antiCorrRes.IsSuccess);
        Assert.True(player.CorruptionIndex < 40.0);

        // Immediate duplicate launch blocked by cooldown
        var dupAntiCorr = engine.Dispatch(new LaunchAntiCorruptionCampaignCommand("IND"));
        Assert.False(dupAntiCorr.IsSuccess);

        // 3. Counter-Terrorism operation
        double stabilityBefore = player.Stability;
        var counterTerrorCmd = new LaunchCounterTerrorismCommand("IND");
        var counterTerrorRes = engine.Dispatch(counterTerrorCmd);
        Assert.True(counterTerrorRes.IsSuccess);
        Assert.True(player.Stability >= stabilityBefore);

        // 4. Minister cabinet management: appoint with sufficient funds, reject duplicates
        var candidates = MinisterRosterService.GetCandidates(MinistryType.Health);
        Assert.NotEmpty(candidates);

        var candidate = candidates[0];
        var minister = new Minister(candidate.Id, candidate.Name, candidate.Competence, candidate.MonthlySalary);
        var appointCmd = new AppointMinisterCommand("IND", MinistryType.Health, minister, candidate.HiringFee);
        Assert.True(engine.Dispatch(appointCmd).IsSuccess);

        // Dismiss minister frees the role
        var dismissCmd = new DismissMinisterCommand("IND", MinistryType.Health);
        Assert.True(engine.Dispatch(dismissCmd).IsSuccess);
        Assert.Null(player.Ministries[MinistryType.Health].ActiveMinister);
    }

    [Fact]
    public void ParityAudit_AirGappedOfflineIntegrity_GuaranteedZeroNetworkCalls()
    {
        // Audit the domain assembly to guarantee zero references to network or telemetry libraries
        var domainAssembly = typeof(GameSession).Assembly;
        var referencedAssemblies = domainAssembly.GetReferencedAssemblies();

        var forbiddenNames = new[]
        {
            "System.Net.Http",
            "System.Net.Sockets",
            "System.Net.WebSockets",
            "System.Net.Requests"
        };

        foreach (var refAsm in referencedAssemblies)
        {
            foreach (var forbidden in forbiddenNames)
            {
                Assert.NotEqual(forbidden, refAsm.Name);
            }
        }

        // Verify no methods in domain expose network sockets or HTTP clients
        var allTypes = domainAssembly.GetTypes();
        foreach (var type in allTypes)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var field in fields)
            {
                Assert.DoesNotContain("HttpClient", field.FieldType.Name);
                Assert.DoesNotContain("Socket", field.FieldType.Name);
            }
        }
    }
}
