using System.Reflection;
using WorldState.Domain.Models;
using WorldState.Domain.Services;
using Xunit;

namespace WorldState.Domain.Tests;

/// <summary>
/// Phase 09 Sprint 09-02 Windows packaging, diagnostics, and clean-machine recovery tests.
/// </summary>
public class WindowsPackagingAndDiagnosticsTests
{
    [Fact]
    public void DiagnosticsService_GeneratesCompleteSystemReport()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "Rashtra_Diag_" + Guid.NewGuid().ToString("N"));
        try
        {
            var service = new DiagnosticsService(saveDirectory: tempDir, logsDirectory: tempDir);
            string contentRoot = Path.Combine(Directory.GetCurrentDirectory(), "content");
            if (!Directory.Exists(contentRoot))
            {
                // Fallback to repo root
                contentRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../content"));
            }

            var report = service.GenerateReport(contentRoot);

            Assert.Equal("1.0.0", report.AppVersion);
            Assert.Equal("Rashtra: Sovereign Republic", report.AppName);
            Assert.False(string.IsNullOrWhiteSpace(report.OsDescription));
            Assert.False(string.IsNullOrWhiteSpace(report.DotNetRuntime));
            Assert.True(report.ProcessorCount > 0);
            Assert.True(report.SaveDirectoryWritable);

            string json = service.SerializeReportJson(report);
            Assert.Contains("\"AppVersion\": \"1.0.0\"", json);
            Assert.Contains("\"SaveDirectoryWritable\": true", json);
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
    public void DiagnosticsService_LogCrash_CreatesSanitizedTimestampedFile()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "Rashtra_Crash_" + Guid.NewGuid().ToString("N"));
        try
        {
            var service = new DiagnosticsService(saveDirectory: tempDir, logsDirectory: tempDir);
            var exception = new InvalidOperationException("Simulation invariant triggered.", new ArgumentOutOfRangeException("TaxRate"));

            string logPath = service.LogCrash(exception, contextInfo: "Test execution of military battle resolution");

            Assert.True(File.Exists(logPath));
            string logContent = File.ReadAllText(logPath);

            Assert.Contains("=== Rashtra: Sovereign Republic Crash Diagnostic Dump ===", logContent);
            Assert.Contains("Version: 1.0.0", logContent);
            Assert.Contains("System.InvalidOperationException", logContent);
            Assert.Contains("Simulation invariant triggered.", logContent);
            Assert.Contains("ArgumentOutOfRangeException", logContent);
            Assert.Contains("Context: Test execution of military battle resolution", logContent);
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
    public void DiagnosticsService_RestoreSaveFromBackup_RecoversCorruptedFile()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "Rashtra_BackupRestore_" + Guid.NewGuid().ToString("N"));
        try
        {
            var service = new DiagnosticsService(saveDirectory: tempDir, logsDirectory: tempDir);
            string slotId = "slot_1";

            string primarySave = Path.Combine(tempDir, $"{slotId}.json");
            string backupSave = Path.Combine(tempDir, $"{slotId}.json.bak");

            File.WriteAllText(backupSave, "{ \"ValidBackupData\": true, \"Treasury\": 5000000000 }");
            File.WriteAllText(primarySave, "{ CORRUPTED_GARBAGE_PAYLOAD");

            // Execute restore
            bool restored = service.RestoreSaveFromBackup(slotId);
            Assert.True(restored);

            string recoveredText = File.ReadAllText(primarySave);
            Assert.Contains("\"ValidBackupData\": true", recoveredText);
            Assert.Contains("5000000000", recoveredText);

            // Attempting to restore non-existent backup fails safely
            bool nonExistentRestored = service.RestoreSaveFromBackup("non_existent_slot");
            Assert.False(nonExistentRestored);
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
    public void CleanMachine_TestDirectoryWritable_HandlesNonExistentAndReadOnlyPaths()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "Rashtra_CleanMachine_" + Guid.NewGuid().ToString("N"));
        try
        {
            var service = new DiagnosticsService();

            // 1. Non-existent path automatically created and verified writable
            string nestedNewDir = Path.Combine(tempDir, "Deep", "Nested", "Path");
            bool writable = service.TestDirectoryWritable(nestedNewDir);
            Assert.True(writable);
            Assert.True(Directory.Exists(nestedNewDir));

            // 2. Path blocked by existing file returns false cross-platform without throwing
            string blockingFile = Path.Combine(tempDir, "blocking_file.txt");
            File.WriteAllText(blockingFile, "block");
            string invalidChildPath = Path.Combine(blockingFile, "sub_dir");
            bool invalidWritable = service.TestDirectoryWritable(invalidChildPath);
            Assert.False(invalidWritable);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
