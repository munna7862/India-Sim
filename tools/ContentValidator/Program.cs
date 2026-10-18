using System.Text.Json;

Console.WriteLine("=== Rashtra Content Validator ===");

string contentDir = Path.Combine(Directory.GetCurrentDirectory(), "content");
if (!Directory.Exists(contentDir))
{
    // Try looking upward if running from tools directory
    string altDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "content");
    if (Directory.Exists(altDir))
    {
        contentDir = Path.GetFullPath(altDir);
    }
}

if (!Directory.Exists(contentDir))
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[WARN] Content directory '{contentDir}' not found. Nothing to validate.");
    Console.ResetColor();
    return 0;
}

Console.WriteLine($"Scanning content in: {contentDir}");
string[] countryFiles = Directory.GetFiles(contentDir, "*.json", SearchOption.AllDirectories);
int validatedCount = 0;
int errorCount = 0;

foreach (string file in countryFiles)
{
    Console.WriteLine($"Validating: {Path.GetFileName(file)}...");
    try
    {
        string json = File.ReadAllText(file);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (!root.TryGetProperty("id", out var idProp) || string.IsNullOrWhiteSpace(idProp.GetString()))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] Missing required non-empty string property 'id'.");
            Console.ResetColor();
            errorCount++;
            continue;
        }

        if (!root.TryGetProperty("name", out var nameProp) || string.IsNullOrWhiteSpace(nameProp.GetString()))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] Missing required non-empty string property 'name'.");
            Console.ResetColor();
            errorCount++;
            continue;
        }

        if (!root.TryGetProperty("startingTreasury", out var treasuryProp) || treasuryProp.GetDecimal() < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] Missing or invalid 'startingTreasury' (must be >= 0).");
            Console.ResetColor();
            errorCount++;
            continue;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  [OK] Valid country definition: {idProp.GetString()} ({nameProp.GetString()})");
        Console.ResetColor();
        validatedCount++;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  [ERROR] Parsing failure: {ex.Message}");
        Console.ResetColor();
        errorCount++;
    }
}

Console.WriteLine($"\nValidation Complete: {validatedCount} valid, {errorCount} errors.");
return errorCount == 0 ? 0 : 1;
