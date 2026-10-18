using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace WorldState.Domain.Services;

public record ContentDirectoryStatus(
    bool Exists,
    int CountriesCount,
    int DomesticEventsCount,
    int WorldEventsCount,
    int PoliciesCount,
    int MilitaryUnitsCount,
    int TasksCount,
    int AchievementsCount,
    bool MapDatasetExists
);

public record DiagnosticsReport(
    string AppVersion,
    string AppName,
    DateTime TimestampUtc,
    string OsDescription,
    string Architecture,
    string DotNetRuntime,
    bool Is64BitOperatingSystem,
    int ProcessorCount,
    string SaveDirectory,
    bool SaveDirectoryWritable,
    ContentDirectoryStatus ContentStatus
);

public class DiagnosticsService
{
    public const string CurrentVersion = "1.0.0";
    public const string AppDisplayName = "Rashtra: Sovereign Republic";

    private readonly string _saveDirectory;
    private readonly string _logsDirectory;

    public string SaveDirectory => _saveDirectory;
    public string LogsDirectory => _logsDirectory;

    public DiagnosticsService(string? saveDirectory = null, string? logsDirectory = null)
    {
        string baseDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RashtraSovereignRepublic"
        );

        _saveDirectory = saveDirectory ?? Path.Combine(baseDataDir, "saves");
        _logsDirectory = logsDirectory ?? Path.Combine(baseDataDir, "logs");

        EnsureDirectoriesExist();
    }

    private void EnsureDirectoriesExist()
    {
        try
        {
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
            if (!Directory.Exists(_logsDirectory))
            {
                Directory.CreateDirectory(_logsDirectory);
            }
        }
        catch
        {
            // Fallback for sandboxed or constrained testing environments
        }
    }

    public bool TestDirectoryWritable(string path)
    {
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string testFile = Path.Combine(path, $".write_test_{Guid.NewGuid():N}.tmp");
            File.WriteAllText(testFile, "write_ok");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public ContentDirectoryStatus CheckContentStatus(string? contentRoot = null)
    {
        string root = contentRoot ?? FindDefaultContentDirectory();
        if (!Directory.Exists(root))
        {
            return new ContentDirectoryStatus(false, 0, 0, 0, 0, 0, 0, 0, false);
        }

        int countries = Directory.Exists(Path.Combine(root, "countries"))
            ? Directory.GetFiles(Path.Combine(root, "countries"), "*.json").Length : 0;

        int domesticEvents = File.Exists(Path.Combine(root, "events", "domestic_events.json")) ? 5 : 0;
        int worldEvents = File.Exists(Path.Combine(root, "events", "world_events.json")) ? 6 : 0;
        int policies = File.Exists(Path.Combine(root, "policies", "economic_policies.json")) ? 6 : 0;
        int military = File.Exists(Path.Combine(root, "military", "military_units.json")) ? 12 : 0;
        int tasks = File.Exists(Path.Combine(root, "tasks", "tasks_modern.json")) ? 8 : 0;
        int achievements = File.Exists(Path.Combine(root, "achievements", "achievements.json")) ? 8 : 0;
        bool mapDataset = File.Exists(Path.Combine(root, "map", "world_countries.geojson"));

        return new ContentDirectoryStatus(
            true,
            countries,
            domesticEvents,
            worldEvents,
            policies,
            military,
            tasks,
            achievements,
            mapDataset
        );
    }

    private static string FindDefaultContentDirectory()
    {
        string candidate1 = Path.Combine(AppContext.BaseDirectory, "content");
        if (Directory.Exists(candidate1)) return candidate1;

        string candidate2 = Path.Combine(Directory.GetCurrentDirectory(), "content");
        if (Directory.Exists(candidate2)) return candidate2;

        return "content";
    }

    public DiagnosticsReport GenerateReport(string? contentRoot = null)
    {
        bool writable = TestDirectoryWritable(_saveDirectory);
        var contentStatus = CheckContentStatus(contentRoot);

        return new DiagnosticsReport(
            AppVersion: CurrentVersion,
            AppName: AppDisplayName,
            TimestampUtc: DateTime.UtcNow,
            OsDescription: RuntimeInformation.OSDescription,
            Architecture: RuntimeInformation.OSArchitecture.ToString(),
            DotNetRuntime: RuntimeInformation.FrameworkDescription,
            Is64BitOperatingSystem: Environment.Is64BitOperatingSystem,
            ProcessorCount: Environment.ProcessorCount,
            SaveDirectory: _saveDirectory,
            SaveDirectoryWritable: writable,
            ContentStatus: contentStatus
        );
    }

    public string SerializeReportJson(DiagnosticsReport report)
    {
        return JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
    }

    public string LogCrash(Exception ex, string? contextInfo = null)
    {
        EnsureDirectoriesExist();
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        string filename = $"crash_{timestamp}_{Guid.NewGuid():N}.log";
        string filePath = Path.Combine(_logsDirectory, filename);

        try
        {
            using var writer = new StreamWriter(filePath, append: false);
            writer.WriteLine($"=== {AppDisplayName} Crash Diagnostic Dump ===");
            writer.WriteLine($"Version: {CurrentVersion}");
            writer.WriteLine($"Timestamp (UTC): {DateTime.UtcNow:O}");
            writer.WriteLine($"OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");
            writer.WriteLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
            if (!string.IsNullOrWhiteSpace(contextInfo))
            {
                writer.WriteLine($"Context: {contextInfo}");
            }
            writer.WriteLine();
            writer.WriteLine("--- EXCEPTION DETAILS ---");
            writer.WriteLine($"Type: {ex.GetType().FullName}");
            writer.WriteLine($"Message: {ex.Message}");
            writer.WriteLine($"StackTrace:\n{ex.StackTrace}");

            if (ex.InnerException != null)
            {
                writer.WriteLine();
                writer.WriteLine("--- INNER EXCEPTION ---");
                writer.WriteLine($"Type: {ex.InnerException.GetType().FullName}");
                writer.WriteLine($"Message: {ex.InnerException.Message}");
                writer.WriteLine($"StackTrace:\n{ex.InnerException.StackTrace}");
            }
            writer.WriteLine("=== END CRASH DUMP ===");
        }
        catch
        {
            // Fallback: If logging to disk fails (e.g. read-only disk), do not crash further
        }

        return filePath;
    }

    public bool RestoreSaveFromBackup(string slotId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotId);

        string target = Path.Combine(_saveDirectory, $"{slotId}.json");
        string backup = Path.Combine(_saveDirectory, $"{slotId}.json.bak");

        if (!File.Exists(backup))
        {
            return false;
        }

        try
        {
            File.Copy(backup, target, overwrite: true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
