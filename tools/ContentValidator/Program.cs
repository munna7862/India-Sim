using System.Text.Json;
using System.Text.RegularExpressions;
using WorldState.Domain.Data;
using WorldState.Domain.Map;
using WorldState.Domain.Scenarios;

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
int errorCount = 0;
int validatedCountries = 0;
int validatedScenarios = 0;
int validatedMaps = 0;

var isoRegex = new Regex("^[A-Z]{3}$", RegexOptions.Compiled);
var countryRegistry = new CountryRegistry();

// 1. Validate Countries
string countriesDir = Path.Combine(contentDir, "countries");
if (Directory.Exists(countriesDir))
{
    Console.WriteLine("\n--- Validating Country Definitions ---");
    string[] countryFiles = Directory.GetFiles(countriesDir, "*.json");
    foreach (string file in countryFiles)
    {
        Console.WriteLine($"Validating country: {Path.GetFileName(file)}...");
        try
        {
            string json = File.ReadAllText(file);
            var def = JsonSerializer.Deserialize<CountryDefinition>(json);
            if (def == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [FAIL] Failed to deserialize CountryDefinition from {Path.GetFileName(file)}.");
                Console.ResetColor();
                errorCount++;
                continue;
            }

            countryRegistry.Register(def);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  [OK] Valid country definition: {def.Id} - {def.Name} (Capital: {def.Capital})");
            Console.ResetColor();
            validatedCountries++;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [ERROR] Validation failure: {ex.Message}");
            Console.ResetColor();
            errorCount++;
        }
    }
}

// 2. Validate Scenarios
string scenariosDir = Path.Combine(contentDir, "scenarios");
if (Directory.Exists(scenariosDir))
{
    Console.WriteLine("\n--- Validating Scenario Definitions ---");
    string[] scenarioFiles = Directory.GetFiles(scenariosDir, "*.json");
    foreach (string file in scenarioFiles)
    {
        Console.WriteLine($"Validating scenario: {Path.GetFileName(file)}...");
        try
        {
            string json = File.ReadAllText(file);
            var scenario = JsonSerializer.Deserialize<ScenarioDefinition>(json);
            if (scenario == null || string.IsNullOrWhiteSpace(scenario.Id))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [FAIL] Missing or invalid scenario structure in {Path.GetFileName(file)}.");
                Console.ResetColor();
                errorCount++;
                continue;
            }

            if (!countryRegistry.TryGet(scenario.DefaultCountryId, out _))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [FAIL] Scenario default country '{scenario.DefaultCountryId}' does not exist in country registry.");
                Console.ResetColor();
                errorCount++;
                continue;
            }

            foreach (var countryId in scenario.SupportedCountryIds)
            {
                if (!countryRegistry.TryGet(countryId, out _))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  [FAIL] Scenario supported country '{countryId}' does not exist in country registry.");
                    Console.ResetColor();
                    errorCount++;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  [OK] Valid scenario: {scenario.Id} - {scenario.Name} (Default: {scenario.DefaultCountryId}, {scenario.SupportedCountryIds.Count} countries)");
            Console.ResetColor();
            validatedScenarios++;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [ERROR] Scenario validation failure: {ex.Message}");
            Console.ResetColor();
            errorCount++;
        }
    }
}

// 3. Validate Map Geometries
string mapDir = Path.Combine(contentDir, "map");
if (Directory.Exists(mapDir))
{
    Console.WriteLine("\n--- Validating Map Datasets ---");
    string[] mapFiles = Directory.GetFiles(mapDir, "*.geojson");
    foreach (string file in mapFiles)
    {
        Console.WriteLine($"Validating map dataset: {Path.GetFileName(file)}...");
        try
        {
            string geoJson = File.ReadAllText(file);
            var mapData = GeoJsonMapLoader.LoadFromJson(geoJson);
            if (mapData.Countries.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [FAIL] No country geometries extracted from {Path.GetFileName(file)}.");
                Console.ResetColor();
                errorCount++;
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  [OK] Valid map dataset: {Path.GetFileName(file)} with {mapData.Countries.Count} country geometries.");
            Console.ResetColor();
            validatedMaps++;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [ERROR] Map validation failure: {ex.Message}");
            Console.ResetColor();
            errorCount++;
        }
    }
}

Console.WriteLine($"\nValidation Complete: {validatedCountries} countries, {validatedScenarios} scenarios, {validatedMaps} map datasets. Errors: {errorCount}.");
return errorCount == 0 ? 0 : 1;
