using System.Text.Json;
using System.Text.RegularExpressions;

Console.WriteLine("=== Rashtra Content Validator ===");

string contentDir = Path.Combine(Directory.GetCurrentDirectory(), "content");
if (!Directory.Exists(contentDir))
{
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
var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var isoRegex = new Regex("^[A-Z]{3}$", RegexOptions.Compiled);

foreach (string file in countryFiles)
{
    Console.WriteLine($"Validating: {Path.GetFileName(file)}...");
    try
    {
        string json = File.ReadAllText(file);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // 1. ID Check (ISO 3166-1 alpha-3)
        if (!root.TryGetProperty("id", out var idProp) || string.IsNullOrWhiteSpace(idProp.GetString()))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] Missing required non-empty string property 'id'.");
            Console.ResetColor();
            errorCount++;
            continue;
        }

        string id = idProp.GetString()!;
        if (!isoRegex.IsMatch(id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] ID '{id}' is not a valid 3-letter uppercase ISO 3166-1 alpha-3 code.");
            Console.ResetColor();
            errorCount++;
            continue;
        }

        if (seenIds.Contains(id))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] Duplicate country ID '{id}' detected in file: {Path.GetFileName(file)}");
            Console.ResetColor();
            errorCount++;
            continue;
        }
        seenIds.Add(id);

        // 2. Name & Capital Check
        if (!root.TryGetProperty("name", out var nameProp) || string.IsNullOrWhiteSpace(nameProp.GetString()))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] Missing required non-empty string property 'name'.");
            Console.ResetColor();
            errorCount++;
            continue;
        }

        if (!root.TryGetProperty("capital", out var capitalProp) || string.IsNullOrWhiteSpace(capitalProp.GetString()))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] Missing required non-empty string property 'capital'.");
            Console.ResetColor();
            errorCount++;
            continue;
        }

        // 3. Stats Invariant Checks
        if (!root.TryGetProperty("startingTreasury", out var treasuryProp) || treasuryProp.GetDecimal() < 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] Missing or invalid 'startingTreasury' (must be >= 0).");
            Console.ResetColor();
            errorCount++;
            continue;
        }

        if (!root.TryGetProperty("startingPopulation", out var popProp) || popProp.GetInt64() <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [FAIL] Missing or invalid 'startingPopulation' (must be > 0).");
            Console.ResetColor();
            errorCount++;
            continue;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  [OK] Valid country definition: {id} - {nameProp.GetString()} (Capital: {capitalProp.GetString()})");
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

Console.WriteLine($"\nValidation Complete: {validatedCount} valid countries verified, {errorCount} errors.");
return errorCount == 0 ? 0 : 1;
