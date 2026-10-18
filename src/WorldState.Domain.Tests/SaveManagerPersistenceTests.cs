using WorldState.Domain.Commands;
using WorldState.Domain.Models;
using WorldState.Domain.Services;
using Xunit;

namespace WorldState.Domain.Tests;

public class SaveManagerPersistenceTests : IDisposable
{
    private readonly string _tempSaveDir;
    private readonly SaveManager _saveManager;

    public SaveManagerPersistenceTests()
    {
        _tempSaveDir = Path.Combine(Path.GetTempPath(), "Rashtra_Tests_" + Guid.NewGuid().ToString("N"));
        _saveManager = new SaveManager(_tempSaveDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempSaveDir))
        {
            try { Directory.Delete(_tempSaveDir, recursive: true); } catch { /* Ignore */ }
        }
    }

    [Fact]
    public void SaveAndLoad_SlotRoundTrip_PreservesAllAuthoritativeState()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        var india = session.GetPlayerCountry()!;

        // Modify diverse state properties
        india.Treasury = 88_500_000_000m;
        india.Debt = 12_000_000_000m;
        india.CreditRating = "AA+";
        india.CorruptionIndex = 18.5;
        india.InflationRate = 0.042;
        india.IncomeTaxRate = 0.22;
        india.CorporateTaxRate = 0.28;
        india.VatRate = 0.14;
        india.Facilities["Hospitals"] = 45;
        india.Facilities["PowerPlants"] = 38;

        session.Clock.CurrentDate = new DateOnly(2027, 8, 20);
        session.Clock.Speed = ClockSpeed.Fast;
        session.Clock.AccumulatorSeconds = 0.25;

        session.AddEvent(new GameEvent(
            Id: "EVT-TEST-01",
            Date: new DateOnly(2027, 8, 15),
            Title: "Diplomatic Milestone",
            Message: "Trade delegation arrived.",
            Category: EventCategory.Diplomacy,
            Severity: EventSeverity.Info,
            ActorCountryId: "IND"
        ));

        var task = new GameTask
        {
            Id = "TSK_INFRA_SAVE_TEST",
            Title = "Save Test Task",
            Category = TaskCategory.Economic,
            Status = GameTaskStatus.Completed,
            IsRewardClaimed = true,
            SubTasks = new List<SubTaskState>
            {
                new("ST_POWER", "Build Power Plants", targetCount: 30, currentCount: 38)
            }
        };
        session.Tasks[task.Id] = task;
        session.ClaimedRewardIds.Add(task.Id);

        session.TimedOperations.Add(new TimedOperation
        {
            Id = "OP-TIMED-01",
            CountryId = "IND",
            Title = "Expanding Defense Grid",
            OperationType = "Construction",
            StartDate = new DateOnly(2027, 8, 1),
            CompletionDate = new DateOnly(2027, 9, 1),
            TotalDays = 31,
            Parameters = new Dictionary<string, string> { ["FacilityType"] = "Barracks", ["Quantity"] = "2" }
        });

        session.Cooldowns.SetCooldown("IND", "ArmyDrills", new DateOnly(2027, 12, 1));

        // Act: Save
        var saveResult = _saveManager.Save("slot_1", session);

        // Assert Save
        Assert.True(saveResult.IsSuccess);
        Assert.NotNull(saveResult.Metadata);
        Assert.Equal("slot_1", saveResult.Metadata.SlotId);
        Assert.Equal("Republic of India", saveResult.Metadata.CountryName);
        Assert.Equal(new DateOnly(2027, 8, 20), saveResult.Metadata.InGameDate);

        // Act: Load
        var loadResult = _saveManager.Load("slot_1");

        // Assert Load
        Assert.True(loadResult.IsSuccess);
        var loaded = loadResult.Session;
        Assert.NotNull(loaded);
        Assert.Equal(session.SessionId, loaded.SessionId);
        Assert.Equal(session.PlayerCountryId, loaded.PlayerCountryId);
        Assert.Equal(session.Clock.CurrentDate, loaded.Clock.CurrentDate);
        Assert.Equal(session.Clock.Speed, loaded.Clock.Speed);
        Assert.Equal(session.Clock.AccumulatorSeconds, loaded.Clock.AccumulatorSeconds);

        var loadedIndia = loaded.GetPlayerCountry();
        Assert.NotNull(loadedIndia);
        Assert.Equal(india.Treasury, loadedIndia.Treasury);
        Assert.Equal(india.Debt, loadedIndia.Debt);
        Assert.Equal(india.CreditRating, loadedIndia.CreditRating);
        Assert.Equal(india.CorruptionIndex, loadedIndia.CorruptionIndex);
        Assert.Equal(india.InflationRate, loadedIndia.InflationRate);
        Assert.Equal(india.IncomeTaxRate, loadedIndia.IncomeTaxRate);
        Assert.Equal(india.Facilities["Hospitals"], loadedIndia.Facilities["Hospitals"]);
        Assert.Equal(india.Facilities["PowerPlants"], loadedIndia.Facilities["PowerPlants"]);

        // Verify Events
        Assert.Single(loaded.Events);
        Assert.Equal("EVT-TEST-01", loaded.Events[0].Id);
        Assert.Equal("Diplomatic Milestone", loaded.Events[0].Title);

        // Verify Tasks
        Assert.True(loaded.Tasks.ContainsKey("TSK_INFRA_SAVE_TEST"));
        Assert.True(loaded.Tasks["TSK_INFRA_SAVE_TEST"].IsRewardClaimed);
        Assert.Contains("TSK_INFRA_SAVE_TEST", loaded.ClaimedRewardIds);

        // Verify Timed Operations
        Assert.Single(loaded.TimedOperations);
        Assert.Equal("OP-TIMED-01", loaded.TimedOperations[0].Id);

        // Verify Cooldowns
        Assert.True(loaded.Cooldowns.IsOnCooldown("IND", "ArmyDrills", new DateOnly(2027, 8, 20)));
    }

    [Fact]
    public void Save_AtomicWrite_CreatesBackupWhenOverwriting()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        session.Clock.CurrentDate = new DateOnly(2026, 1, 1);
        _saveManager.Save("slot_2", session);

        string savePath = _saveManager.GetSlotFilePath("slot_2");
        string backupPath = _saveManager.GetSlotBackupPath("slot_2");

        Assert.True(File.Exists(savePath));
        Assert.False(File.Exists(backupPath));

        // Act: Save again with updated date
        session.Clock.CurrentDate = new DateOnly(2026, 6, 15);
        _saveManager.Save("slot_2", session);

        // Assert: Backup file created with initial state
        Assert.True(File.Exists(savePath));
        Assert.True(File.Exists(backupPath));

        string backupJson = File.ReadAllText(backupPath);
        Assert.Contains("2026-01-01", backupJson);

        string currentJson = File.ReadAllText(savePath);
        Assert.Contains("2026-06-15", currentJson);
    }

    [Fact]
    public void Load_NonExistentSlot_ReturnsFileNotFound()
    {
        var result = _saveManager.Load("non_existent_slot");
        Assert.False(result.IsSuccess);
        Assert.Equal(SaveErrorCode.FileNotFound, result.ErrorCode);
    }

    [Fact]
    public void Load_CorruptedFile_ReturnsCorruptedSaveDataWithoutCrash()
    {
        // Arrange: Write malformed JSON
        string path = _saveManager.GetSlotFilePath("corrupt_slot");
        File.WriteAllText(path, "{ corrupted json: invalid syntax ");

        // Act
        var result = _saveManager.Load("corrupt_slot");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(SaveErrorCode.CorruptedSaveData, result.ErrorCode);
    }

    [Fact]
    public void Load_NewerSchemaVersion_ReturnsSaveTooNew()
    {
        // Arrange: Create save with future schema version 999
        var session = GameSession.CreateNewGame("IND");
        session.SchemaVersion = 999;
        _saveManager.Save("future_slot", session);

        // Act
        var result = _saveManager.Load("future_slot");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(SaveErrorCode.SaveTooNew, result.ErrorCode);
    }

    [Fact]
    public void QuickSave_And_QuickLoad_WorkSeamlessly()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        session.Clock.CurrentDate = new DateOnly(2026, 5, 10);

        // Act
        var saveResult = _saveManager.QuickSave(session);
        var loadResult = _saveManager.QuickLoad();

        // Assert
        Assert.True(saveResult.IsSuccess);
        Assert.True(loadResult.IsSuccess);
        Assert.NotNull(loadResult.Session);
        Assert.Equal(new DateOnly(2026, 5, 10), loadResult.Session.Clock.CurrentDate);
    }

    [Fact]
    public void ListSlots_ReturnsAllSlotsWithCorrectMetadata()
    {
        // Arrange
        var session1 = GameSession.CreateNewGame("IND");
        session1.Clock.CurrentDate = new DateOnly(2026, 2, 1);
        _saveManager.Save("slot_1", session1);

        var session2 = GameSession.CreateNewGame("IND");
        session2.Clock.CurrentDate = new DateOnly(2026, 3, 1);
        _saveManager.Save("slot_2", session2);

        // Act
        var slots = _saveManager.ListSlots();

        // Assert
        Assert.Equal(2, slots.Count);
        Assert.Contains(slots, s => s.SlotId == "slot_1");
        Assert.Contains(slots, s => s.SlotId == "slot_2");
    }

    [Fact]
    public void DeleteSlot_RemovesSaveAndBackupFiles()
    {
        // Arrange
        var session = GameSession.CreateNewGame("IND");
        _saveManager.Save("slot_3", session);
        _saveManager.Save("slot_3", session); // Trigger backup creation

        string savePath = _saveManager.GetSlotFilePath("slot_3");
        string backupPath = _saveManager.GetSlotBackupPath("slot_3");
        Assert.True(File.Exists(savePath));
        Assert.True(File.Exists(backupPath));

        // Act
        bool deleted = _saveManager.DeleteSlot("slot_3");

        // Assert
        Assert.True(deleted);
        Assert.False(File.Exists(savePath));
        Assert.False(File.Exists(backupPath));
    }

    [Fact]
    public void ContinuationAfterLoad_PermitsFurtherSimulationAndCommands()
    {
        // Arrange: Start, save, load
        var session = GameSession.CreateNewGame("IND");
        session.Clock.Speed = ClockSpeed.Normal;
        _saveManager.Save("slot_cont", session);

        var loadResult = _saveManager.Load("slot_cont");
        Assert.True(loadResult.IsSuccess);
        var restoredSession = loadResult.Session!;

        // Act: Initialize simulation engine on restored session and tick 30 days
        var engine = new SimulationEngine(restoredSession);
        var taxCommand = new AdjustTaxRateCommand("IND", TaxType.IncomeTax, 0.26);
        var cmdResult = engine.Dispatch(taxCommand);

        engine.Tick(30.0); // 30 days advance

        // Assert: Successfully simulated continuation
        Assert.True(cmdResult.IsSuccess);
        Assert.Equal(0.26, restoredSession.GetPlayerCountry()!.IncomeTaxRate);
        Assert.Equal(new DateOnly(2026, 1, 31), restoredSession.Clock.CurrentDate);
    }
}
