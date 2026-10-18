using WorldState.Domain.Commands;
using WorldState.Domain.Military;
using WorldState.Domain.Models;
using WorldState.Domain.Services;
using WorldState.Domain.ViewModels;
using Xunit;

namespace WorldState.Domain.Tests;

public class DiplomacyMilitaryTests
{
    private GameSession CreateTestSession()
    {
        return GameSession.CreateNewGame("IND");
    }

    [Fact]
    public void SendDiplomaticGift_ValidSmallTier_TransfersFundsAndImprovesRelations()
    {
        var session = CreateTestSession();
        var india = session.GetPlayerCountry()!;
        var usa = session.Countries["USA"];

        decimal indiaStartingTreasury = india.Treasury;
        decimal usaStartingTreasury = usa.Treasury;
        double startingRelations = india.GetRelationsWith("USA").Relations;

        var cmd = new SendDiplomaticGiftCommand("IND", "USA", DiplomaticGiftTier.Small);
        var result = cmd.Execute(session);

        Assert.True(result.IsSuccess);
        Assert.Equal(indiaStartingTreasury - 250_000_000m, india.Treasury);
        Assert.Equal(usaStartingTreasury + 250_000_000m, usa.Treasury);
        Assert.Equal(startingRelations + 5.0, india.GetRelationsWith("USA").Relations);
        Assert.Equal(startingRelations + 5.0, usa.GetRelationsWith("IND").Relations);
        Assert.True(session.Cooldowns.IsOnCooldown("IND", "DiplomaticGift_USA", session.Clock.CurrentDate));
    }

    [Fact]
    public void SendDiplomaticGift_OnCooldown_RejectsCommand()
    {
        var session = CreateTestSession();
        var cmd = new SendDiplomaticGiftCommand("IND", "USA", DiplomaticGiftTier.Small);
        cmd.Execute(session);

        // Immediate subsequent gift should be on cooldown
        var result2 = cmd.Execute(session);
        Assert.False(result2.IsSuccess);
        Assert.Equal("ActionOnCooldown", result2.ErrorCode);
    }

    [Fact]
    public void SendDiplomaticGift_InvalidSelfOrAtWar_RejectsCommand()
    {
        var session = CreateTestSession();
        var selfGift = new SendDiplomaticGiftCommand("IND", "IND", DiplomaticGiftTier.Small);
        var selfResult = selfGift.Execute(session);
        Assert.False(selfResult.IsSuccess);
        Assert.Equal("InvalidTarget", selfResult.ErrorCode);

        // At war check
        session.Countries["IND"].GetRelationsWith("PAK").IsAtWar = true;
        var warGift = new SendDiplomaticGiftCommand("IND", "PAK", DiplomaticGiftTier.Small);
        var warResult = warGift.Execute(session);
        Assert.False(warResult.IsSuccess);
        Assert.Equal("AtWar", warResult.ErrorCode);
    }

    [Fact]
    public void SignNonAggressionPact_ValidConditions_Signs24MonthPactAndImprovesRelations()
    {
        var session = CreateTestSession();
        var india = session.GetPlayerCountry()!;
        var rus = session.Countries["RUS"];

        decimal startingTreasury = india.Treasury;
        double startingRel = india.GetRelationsWith("RUS").Relations;

        var cmd = new SignNonAggressionPactCommand("IND", "RUS", 24);
        var result = cmd.Execute(session);

        Assert.True(result.IsSuccess);
        Assert.Equal(startingTreasury - 50_000_000m, india.Treasury);
        Assert.Equal(startingRel + 10.0, india.GetRelationsWith("RUS").Relations);
        Assert.Equal(startingRel + 10.0, rus.GetRelationsWith("IND").Relations);

        Assert.True(india.GetRelationsWith("RUS").HasActiveNonAggressionPact(session.Clock.CurrentDate));
        Assert.True(rus.GetRelationsWith("IND").HasActiveNonAggressionPact(session.Clock.CurrentDate));
    }

    [Fact]
    public void SignNonAggressionPact_HostileNation_RejectsDueToLowRelations()
    {
        var session = CreateTestSession();
        // IND and PAK default relations are -60.0 (below -20.0 threshold)
        var cmd = new SignNonAggressionPactCommand("IND", "PAK", 24);
        var result = cmd.Execute(session);

        Assert.False(result.IsSuccess);
        Assert.Equal("RelationsTooLow", result.ErrorCode);
    }

    [Fact]
    public void SignNonAggressionPact_ActivePactExists_RejectsDuplicate()
    {
        var session = CreateTestSession();
        var cmd = new SignNonAggressionPactCommand("IND", "RUS", 24);
        cmd.Execute(session);

        var duplicateResult = cmd.Execute(session);
        Assert.False(duplicateResult.IsSuccess);
        Assert.Equal("ActivePactExists", duplicateResult.ErrorCode);
    }

    [Fact]
    public void SignTradeContract_ValidSurplus_EstablishesTradeAgreement()
    {
        var session = CreateTestSession();
        var india = session.GetPlayerCountry()!;
        var gbr = session.Countries["GBR"];

        double startingRel = india.GetRelationsWith("GBR").Relations;

        var cmd = new SignTradeContractCommand(
            exporterCountryId: "IND",
            importerCountryId: "GBR",
            resourceType: "Food",
            monthlyQuantity: 5_000.0,
            monthlyPayment: 200_000_000m,
            durationMonths: 24
        );

        var result = cmd.Execute(session);
        Assert.True(result.IsSuccess);

        var indStance = india.GetRelationsWith("GBR");
        var gbrStance = gbr.GetRelationsWith("IND");

        Assert.Single(indStance.ActiveTradeContracts);
        Assert.Single(gbrStance.ActiveTradeContracts);
        Assert.Equal(startingRel + 5.0, indStance.Relations);
    }

    [Fact]
    public void CancelTradeContract_RemovesContractFromBothNations_WithPenalty()
    {
        var session = CreateTestSession();
        var cmd = new SignTradeContractCommand("IND", "GBR", "Food", 5_000.0, 200_000_000m, 24);
        cmd.Execute(session);

        var contract = session.GetPlayerCountry()!.GetRelationsWith("GBR").ActiveTradeContracts.First();
        var cancelCmd = new CancelTradeContractCommand("IND", contract.Id);
        var cancelResult = cancelCmd.Execute(session);

        Assert.True(cancelResult.IsSuccess);
        Assert.Empty(session.GetPlayerCountry()!.GetRelationsWith("GBR").ActiveTradeContracts);
        Assert.Empty(session.Countries["GBR"].GetRelationsWith("IND").ActiveTradeContracts);
    }

    [Fact]
    public void ExecuteMonthlyLedger_ProcessesTradeRevenuesAndResourceExchanges()
    {
        var session = CreateTestSession();
        var india = session.GetPlayerCountry()!;

        // Sign export deal: IND exports 10,000 Food for $300M/month
        var cmd = new SignTradeContractCommand("IND", "GBR", "Food", 10_000.0, 300_000_000m, 24);
        cmd.Execute(session);

        var ledgerCmd = new ExecuteMonthlyLedgerCommand("IND");
        var ledgerResult = ledgerCmd.Execute(session);

        Assert.True(ledgerResult.IsSuccess);
        Assert.NotNull(india.LastBudgetReport);
        Assert.Equal(300_000_000m, india.LastBudgetReport.TradeExportRevenue);
    }

    [Fact]
    public void MilitaryUnitRegistry_Contains12DistinctUnitsAcrossBranches()
    {
        var registry = MilitaryUnitRegistry.CreateDefaultCatalog();
        Assert.Equal(12, registry.Units.Count);

        Assert.True(registry.TryGet("infantry_division", out var inf) && inf?.Branch == MilitaryBranch.Land);
        Assert.True(registry.TryGet("multirole_fighter", out var fighter) && fighter?.Branch == MilitaryBranch.Air);
        Assert.True(registry.TryGet("aircraft_carrier", out var carrier) && carrier?.Branch == MilitaryBranch.Naval);
    }

    [Fact]
    public void RecruitMilitaryUnits_ValidResourcesAndBarracks_AddsUnitsToInventory()
    {
        var session = CreateTestSession();
        var india = session.GetPlayerCountry()!;

        int initialTanks = india.MilitaryUnits.GetValueOrDefault("main_battle_tank", 0);
        decimal initialTreasury = india.Treasury;
        double initialIron = india.Stockpiles.Iron;
        double initialOil = india.Stockpiles.Oil;

        var cmd = new RecruitMilitaryUnitsCommand("IND", "main_battle_tank", 5);
        var result = cmd.Execute(session);

        Assert.True(result.IsSuccess);
        Assert.Equal(initialTanks + 5, india.MilitaryUnits["main_battle_tank"]);
        Assert.Equal(initialTreasury - (200_000_000m * 5), india.Treasury);
        Assert.Equal(initialIron - (800.0 * 5), india.Stockpiles.Iron);
        Assert.Equal(initialOil - (300.0 * 5), india.Stockpiles.Oil);
    }

    [Fact]
    public void RecruitMilitaryUnits_InsufficientBarracks_RejectsCommand()
    {
        var session = CreateTestSession();
        var india = session.GetPlayerCountry()!;
        india.Facilities["Barracks"] = 2; // Carrier requires 20

        var cmd = new RecruitMilitaryUnitsCommand("IND", "aircraft_carrier", 1);
        var result = cmd.Execute(session);

        Assert.False(result.IsSuccess);
        Assert.Equal("InsufficientBarracks", result.ErrorCode);
    }

    [Fact]
    public void DisbandMilitaryUnits_ValidQuantity_ReducesInventoryAndRecoversScrap()
    {
        var session = CreateTestSession();
        var india = session.GetPlayerCountry()!;

        int initialTanks = india.MilitaryUnits["main_battle_tank"];
        double initialIron = india.Stockpiles.Iron;

        var cmd = new DisbandMilitaryUnitsCommand("IND", "main_battle_tank", 2);
        var result = cmd.Execute(session);

        Assert.True(result.IsSuccess);
        Assert.Equal(initialTanks - 2, india.MilitaryUnits["main_battle_tank"]);
        // 20% scrap iron recovered: 800 * 2 * 0.20 = 320
        Assert.Equal(initialIron + 320.0, india.Stockpiles.Iron);
    }

    [Fact]
    public void DeclareWar_ValidHostilities_EstablishesWarAndTerminatesTrade()
    {
        var session = CreateTestSession();
        var india = session.GetPlayerCountry()!;
        var pak = session.Countries["PAK"];

        // Add trade contract first
        var trade = new TradeContract("TC_TEST", "IND", "PAK", "Food", 100, 10_000_000m, session.Clock.CurrentDate);
        india.GetRelationsWith("PAK").ActiveTradeContracts.Add(trade);
        pak.GetRelationsWith("IND").ActiveTradeContracts.Add(trade);

        var warCmd = new DeclareWarCommand("IND", "PAK");
        var warResult = warCmd.Execute(session);

        Assert.True(warResult.IsSuccess);
        Assert.True(india.GetRelationsWith("PAK").IsAtWar);
        Assert.True(pak.GetRelationsWith("IND").IsAtWar);
        Assert.Equal(-100.0, india.GetRelationsWith("PAK").Relations);
        Assert.Empty(india.GetRelationsWith("PAK").ActiveTradeContracts);
    }

    [Fact]
    public void DeclareWar_WhenActiveNAP_BlockedByTreaty()
    {
        var session = CreateTestSession();
        var india = session.GetPlayerCountry()!;
        india.GetRelationsWith("USA").NonAggressionPactUntil = session.Clock.CurrentDate.AddMonths(12);

        var warCmd = new DeclareWarCommand("IND", "USA");
        var warResult = warCmd.Execute(session);

        Assert.False(warResult.IsSuccess);
        Assert.Equal("TreatyViolationBlocked", warResult.ErrorCode);
        Assert.False(india.GetRelationsWith("USA").IsAtWar);
    }

    [Fact]
    public void ResolveCombat_AtWar_CalculatesCasualtiesAndOutcome()
    {
        var session = CreateTestSession();
        session.GetPlayerCountry()!.GetRelationsWith("PAK").IsAtWar = true;
        session.Countries["PAK"].GetRelationsWith("IND").IsAtWar = true;

        int indUnitsBefore = session.GetPlayerCountry()!.GetTotalMilitaryUnits();
        int pakUnitsBefore = session.Countries["PAK"].GetTotalMilitaryUnits();

        var combatCmd = new ResolveCombatCommand("IND", "PAK");
        var result = combatCmd.Execute(session);

        Assert.True(result.IsSuccess);
        Assert.NotNull(combatCmd.LastReport);

        int indUnitsAfter = session.GetPlayerCountry()!.GetTotalMilitaryUnits();
        int pakUnitsAfter = session.Countries["PAK"].GetTotalMilitaryUnits();

        Assert.True(indUnitsAfter < indUnitsBefore);
        Assert.True(pakUnitsAfter < pakUnitsBefore);
        Assert.True(session.Cooldowns.IsOnCooldown("IND", "Combat_PAK", session.Clock.CurrentDate));
    }

    [Fact]
    public void SignPeaceTreaty_WhenAtWar_EndsHostilitiesAndEstablishesNAP()
    {
        var session = CreateTestSession();
        var ind = session.GetPlayerCountry()!;
        var pak = session.Countries["PAK"];

        ind.GetRelationsWith("PAK").IsAtWar = true;
        pak.GetRelationsWith("IND").IsAtWar = true;

        var peaceCmd = new SignPeaceTreatyCommand("IND", "PAK");
        var peaceResult = peaceCmd.Execute(session);

        Assert.True(peaceResult.IsSuccess);
        Assert.False(ind.GetRelationsWith("PAK").IsAtWar);
        Assert.False(pak.GetRelationsWith("IND").IsAtWar);
        Assert.Equal(-25.0, ind.GetRelationsWith("PAK").Relations);
        Assert.True(ind.GetRelationsWith("PAK").HasActiveNonAggressionPact(session.Clock.CurrentDate));
    }

    [Fact]
    public void InternalSecurityAndEspionage_ActionsExecuteCleanly()
    {
        var session = CreateTestSession();
        var ind = session.GetPlayerCountry()!;

        // Presidential Protection
        var protCmd = new FundPresidentialProtectionCommand("IND", FundingLevel.Maximum);
        Assert.True(protCmd.Execute(session).IsSuccess);
        Assert.Equal(FundingLevel.Maximum, ind.PresidentialProtectionLevel);

        // Counter-Terrorism
        var ctCmd = new LaunchCounterTerrorismCommand("IND");
        Assert.True(ctCmd.Execute(session).IsSuccess);
        Assert.True(session.Cooldowns.IsOnCooldown("IND", "CounterTerrorism", session.Clock.CurrentDate));

        // Covert Operation (Intel Gathering)
        var spyCmd = new DispatchCovertOperationCommand("IND", "USA", CovertOperationType.GatherIntelligence)
        {
            ForceDetectionForTesting = false
        };
        var spyResult = spyCmd.Execute(session);
        Assert.True(spyResult.IsSuccess);

        // Covert Operation (Compromised agent detection)
        var detectedSpyCmd = new DispatchCovertOperationCommand("IND", "CHN", CovertOperationType.SabotageInfrastructure)
        {
            ForceDetectionForTesting = true
        };
        double chnRelBefore = ind.GetRelationsWith("CHN").Relations;
        var detectedResult = detectedSpyCmd.Execute(session);
        Assert.True(detectedResult.IsSuccess);
        Assert.Equal(chnRelBefore - 25.0, ind.GetRelationsWith("CHN").Relations);
    }

    [Fact]
    public void DiplomacyAndMilitaryViewModels_MapSessionStateAccurately()
    {
        var session = CreateTestSession();
        var dipVm = DiplomacyViewModel.FromSession(session);
        Assert.Equal(5, dipVm.TotalForeignNations);
        Assert.NotNull(dipVm.ForeignNations.FirstOrDefault(n => n.CountryId == "USA"));

        var milVm = MilitaryViewModel.FromSession(session);
        Assert.True(milVm.TotalUnits > 0);
        Assert.True(milVm.TotalCombatPower > 0);
        Assert.NotEmpty(milVm.Units);
    }

    [Fact]
    public void DiplomacyAndMilitaryState_SaveLoadRoundTrip_PreservesAllData()
    {
        var session = CreateTestSession();
        var ind = session.GetPlayerCountry()!;

        // Setup diplomacy & trade state
        ind.GetRelationsWith("RUS").NonAggressionPactUntil = session.Clock.CurrentDate.AddMonths(18);
        var trade = new TradeContract("TC_RUS_OIL", "RUS", "IND", "Oil", 2000, 150_000_000m, session.Clock.CurrentDate);
        ind.GetRelationsWith("RUS").ActiveTradeContracts.Add(trade);

        // Setup military inventory
        ind.MilitaryUnits["main_battle_tank"] = 123;
        ind.MilitaryUnits["multirole_fighter"] = 77;
        ind.PresidentialProtectionLevel = FundingLevel.High;

        var saveService = new SaveLoadService();
        string json = saveService.SerializeSession(session);
        var loadedSession = saveService.DeserializeSession(json);

        var loadedInd = loadedSession.GetPlayerCountry()!;
        Assert.Equal(FundingLevel.High, loadedInd.PresidentialProtectionLevel);
        Assert.Equal(123, loadedInd.MilitaryUnits["main_battle_tank"]);
        Assert.Equal(77, loadedInd.MilitaryUnits["multirole_fighter"]);

        var loadedRusStance = loadedInd.GetRelationsWith("RUS");
        Assert.True(loadedRusStance.HasActiveNonAggressionPact(session.Clock.CurrentDate));
        Assert.Single(loadedRusStance.ActiveTradeContracts);
        Assert.Equal("Oil", loadedRusStance.ActiveTradeContracts[0].ResourceType);
        Assert.Equal(2000, loadedRusStance.ActiveTradeContracts[0].MonthlyQuantity);
    }
}
