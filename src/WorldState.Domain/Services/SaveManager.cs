using System.Text.Json;
using WorldState.Domain.Models;

namespace WorldState.Domain.Services;

public class SaveManager
{
    public const int CurrentSchemaVersion = 1;
    public const string QuickSaveSlot = "quicksave";
    public const string AutoSaveSlot = "autosave";

    private readonly string _saveDirectory;
    private readonly SaveLoadService _serializer;

    public string SaveDirectory => _saveDirectory;

    public SaveManager(string? saveDirectory = null, SaveLoadService? serializer = null)
    {
        _saveDirectory = saveDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RashtraSovereignRepublic",
            "saves"
        );
        _serializer = serializer ?? new SaveLoadService();

        if (!Directory.Exists(_saveDirectory))
        {
            Directory.CreateDirectory(_saveDirectory);
        }
    }

    public string GetSlotFilePath(string slotId) =>
        Path.Combine(_saveDirectory, $"{SanitizeSlotId(slotId)}.json");

    public string GetSlotBackupPath(string slotId) =>
        Path.Combine(_saveDirectory, $"{SanitizeSlotId(slotId)}.json.bak");

    public string GetSlotTempPath(string slotId) =>
        Path.Combine(_saveDirectory, $"{SanitizeSlotId(slotId)}.json.tmp");

    public SaveSlotResult Save(string slotId, GameSession session)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotId);
        ArgumentNullException.ThrowIfNull(session);

        string targetPath = GetSlotFilePath(slotId);
        string tempPath = GetSlotTempPath(slotId);
        string backupPath = GetSlotBackupPath(slotId);

        try
        {
            // 1. Serialize session to string
            string json = _serializer.SerializeSession(session);

            // 2. Atomic write: write to temp file first
            File.WriteAllText(tempPath, json);

            // 3. Backup existing save if present
            if (File.Exists(targetPath))
            {
                File.Copy(targetPath, backupPath, overwrite: true);
            }

            // 4. Atomically replace target with temp file
            File.Move(tempPath, targetPath, overwrite: true);

            var playerCountry = session.GetPlayerCountry();
            var metadata = new SaveSlotMetadata(
                SlotId: slotId,
                DisplayName: FormatSlotDisplayName(slotId),
                CountryId: session.PlayerCountryId,
                CountryName: playerCountry?.Name ?? session.PlayerCountryId,
                InGameDate: session.Clock.CurrentDate,
                RealTimeSavedUtc: DateTime.UtcNow,
                SchemaVersion: session.SchemaVersion,
                IsCorrupted: false,
                ErrorMessage: null,
                FileSizeBytes: new FileInfo(targetPath).Length
            );

            return SaveSlotResult.Success(slotId, metadata);
        }
        catch (Exception ex)
        {
            // Clean up temp file if failed
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { /* Ignore */ }
            }

            return SaveSlotResult.Failure(
                slotId,
                SaveErrorCode.IoError,
                $"Failed to write save slot '{slotId}': {ex.Message}"
            );
        }
    }

    public LoadSlotResult Load(string slotId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotId);

        string targetPath = GetSlotFilePath(slotId);
        if (!File.Exists(targetPath))
        {
            return LoadSlotResult.Failure(
                slotId,
                SaveErrorCode.FileNotFound,
                $"Save slot '{slotId}' does not exist."
            );
        }

        try
        {
            string json = File.ReadAllText(targetPath);
            var session = _serializer.DeserializeSession(json);

            if (session.SchemaVersion > CurrentSchemaVersion)
            {
                return LoadSlotResult.Failure(
                    slotId,
                    SaveErrorCode.SaveTooNew,
                    $"Save schema version {session.SchemaVersion} is newer than current supported version {CurrentSchemaVersion}."
                );
            }

            return LoadSlotResult.Success(slotId, session);
        }
        catch (JsonException ex)
        {
            return LoadSlotResult.Failure(
                slotId,
                SaveErrorCode.CorruptedSaveData,
                $"Corrupted save file in slot '{slotId}': {ex.Message}"
            );
        }
        catch (Exception ex)
        {
            return LoadSlotResult.Failure(
                slotId,
                SaveErrorCode.IoError,
                $"Failed to read save slot '{slotId}': {ex.Message}"
            );
        }
    }

    public SaveSlotResult QuickSave(GameSession session) => Save(QuickSaveSlot, session);

    public LoadSlotResult QuickLoad() => Load(QuickSaveSlot);

    public SaveSlotResult AutoSave(GameSession session) => Save(AutoSaveSlot, session);

    public bool DeleteSlot(string slotId)
    {
        string targetPath = GetSlotFilePath(slotId);
        string backupPath = GetSlotBackupPath(slotId);
        bool deleted = false;

        if (File.Exists(targetPath))
        {
            File.Delete(targetPath);
            deleted = true;
        }

        if (File.Exists(backupPath))
        {
            File.Delete(backupPath);
        }

        return deleted;
    }

    public SaveSlotMetadata? GetSlotMetadata(string slotId)
    {
        string targetPath = GetSlotFilePath(slotId);
        if (!File.Exists(targetPath)) return null;

        try
        {
            var fileInfo = new FileInfo(targetPath);
            string json = File.ReadAllText(targetPath);
            var session = _serializer.DeserializeSession(json);
            var country = session.GetPlayerCountry();

            return new SaveSlotMetadata(
                SlotId: slotId,
                DisplayName: FormatSlotDisplayName(slotId),
                CountryId: session.PlayerCountryId,
                CountryName: country?.Name ?? session.PlayerCountryId,
                InGameDate: session.Clock.CurrentDate,
                RealTimeSavedUtc: fileInfo.LastWriteTimeUtc,
                SchemaVersion: session.SchemaVersion,
                IsCorrupted: false,
                ErrorMessage: null,
                FileSizeBytes: fileInfo.Length
            );
        }
        catch (Exception ex)
        {
            var fileInfo = new FileInfo(targetPath);
            return new SaveSlotMetadata(
                SlotId: slotId,
                DisplayName: FormatSlotDisplayName(slotId),
                CountryId: "???",
                CountryName: "Corrupted Save",
                InGameDate: new DateOnly(2026, 1, 1),
                RealTimeSavedUtc: fileInfo.LastWriteTimeUtc,
                SchemaVersion: 0,
                IsCorrupted: true,
                ErrorMessage: ex.Message,
                FileSizeBytes: fileInfo.Length
            );
        }
    }

    public IReadOnlyList<SaveSlotMetadata> ListSlots()
    {
        var list = new List<SaveSlotMetadata>();
        if (!Directory.Exists(_saveDirectory)) return list;

        var files = Directory.GetFiles(_saveDirectory, "*.json");
        foreach (var file in files)
        {
            string slotId = Path.GetFileNameWithoutExtension(file);
            var meta = GetSlotMetadata(slotId);
            if (meta != null)
            {
                list.Add(meta);
            }
        }

        return list.OrderByDescending(m => m.RealTimeSavedUtc).ToList();
    }

    private static string SanitizeSlotId(string slotId)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(slotId.Where(c => !invalid.Contains(c)).ToArray()).ToLowerInvariant();
    }

    private static string FormatSlotDisplayName(string slotId) => slotId.ToLowerInvariant() switch
    {
        QuickSaveSlot => "Quick Save (F5)",
        AutoSaveSlot => "Auto Save",
        "slot_1" => "Save Slot 1",
        "slot_2" => "Save Slot 2",
        "slot_3" => "Save Slot 3",
        "slot_4" => "Save Slot 4",
        "slot_5" => "Save Slot 5",
        _ => $"Slot {slotId}"
    };
}
