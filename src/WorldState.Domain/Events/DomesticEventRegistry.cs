using System.Text.Json;
using WorldState.Domain.Models;

namespace WorldState.Domain.Events;

public class DomesticEventRegistry
{
    private readonly Dictionary<string, DomesticEventDefinition> _events = new();

    public IReadOnlyCollection<DomesticEventDefinition> Events => _events.Values;

    public void Register(DomesticEventDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            throw new ArgumentException("Event ID cannot be null or empty.");
        }
        _events[definition.Id] = definition;
    }

    public bool TryGet(string eventId, out DomesticEventDefinition? definition) =>
        _events.TryGetValue(eventId, out definition);

    public void LoadFromJson(string json)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var list = JsonSerializer.Deserialize<List<DomesticEventDefinition>>(json, options)
            ?? throw new JsonException("Failed to deserialize event definitions.");

        foreach (var def in list)
        {
            Register(def);
        }
    }

    public static DomesticEventRegistry CreateDefaultCatalog()
    {
        var registry = new DomesticEventRegistry();
        registry.Register(new DomesticEventDefinition
        {
            Id = "EVT_FLOOD_RELIEF",
            Title = "Monsoon Floods in Agricultural Belt",
            Description = "Unprecedented monsoon rains have flooded major river basins, disrupting farm harvests and displacing rural populations.",
            Category = "Emergency",
            RandomWeight = 0.35,
            CooldownMonths = 12,
            Choices = new()
            {
                new("CH_FLOOD_RELIEF_FULL", "Comprehensive Disaster Relief Package", "Disburse emergency compensation to farmers and repair destroyed irrigation embankments immediately.", 500_000_000m, 6.0, 3.0, 0.0),
                new("CH_FLOOD_RELIEF_PARTIAL", "Targeted Emergency Aid Only", "Deliver grain rations and basic shelter without long-term crop compensation.", 150_000_000m, 1.0, 0.0, 0.0),
                new("CH_FLOOD_RELIEF_IGNORE", "Rely on Local State Administration", "Avoid federal treasury expenditure and instruct regional authorities to manage with local disaster funds.", 0m, -7.0, -4.0, 0.0)
            }
        });

        registry.Register(new DomesticEventDefinition
        {
            Id = "EVT_DOCTORS_STRIKE",
            Title = "Medical Professionals Union Strike",
            Description = "Public hospital doctors and healthcare staff have organized strikes demanding modernized facilities and salary parity.",
            Category = "SocialUnrest",
            MaxApproval = 60.0,
            RandomWeight = 0.30,
            CooldownMonths = 12,
            Choices = new()
            {
                new("CH_DOC_ACCEDE", "Upgrade Medical Salaries and Clinic Equipment", "Fulfill core demands, renovate emergency rooms and expand nurse allowances.", 300_000_000m, 5.0, 4.0, 0.0),
                new("CH_DOC_ENFORCE", "Enact Essential Services Maintenance Act", "Order immediate return to work under penalty of legal suspension; refuse budget hikes.", 0m, -8.0, 2.0, 0.0)
            }
        });

        registry.Register(new DomesticEventDefinition
        {
            Id = "EVT_CORRUPTION_SCANDAL",
            Title = "Procurement Bribery Scandal Exposed",
            Description = "Investigative journalists have leaked financial records implicating senior procurement bureaucrats in kickback schemes.",
            Category = "Government",
            MinCorruption = 25.0,
            RandomWeight = 0.40,
            CooldownMonths = 10,
            Choices = new()
            {
                new("CH_CORR_PROBE", "Order Independent High-Court Judicial Inquiry", "Suspend implicated officials, seize illicit assets, and demonstrate absolute intolerance for graft.", 200_000_000m, 7.0, -2.0, -6.0),
                new("CH_CORR_BURDEN", "Internal Departmental Review", "Handle the matter quietly through administrative transfers without public trials.", 20_000_000m, -6.0, 1.0, 2.0)
            }
        });

        registry.Register(new DomesticEventDefinition
        {
            Id = "EVT_TECH_BOOM",
            Title = "Global Semiconductor Investment Consortium",
            Description = "An international alliance of electronics manufacturers offers to construct cutting-edge silicon fabrication clusters.",
            Category = "Economy",
            MaxStability = 55.0,
            RandomWeight = 0.30,
            CooldownMonths = 18,
            Choices = new()
            {
                new("CH_TECH_SUBSIDY", "Provide 50% Capital Subsidy & High-Tech Zone", "Guarantee uninterrupted high-voltage power, water rights, and substantial tax holidays.", 600_000_000m, 5.0, 4.0, 0.0),
                new("CH_TECH_PARTNER", "Joint Public-Private Venture Model", "Co-finance the facility in exchange for permanent 30% sovereign equity ownership.", 350_000_000m, 3.0, 2.0, 0.0)
            }
        });

        registry.Register(new DomesticEventDefinition
        {
            Id = "EVT_BORDER_TENSION",
            Title = "Border Infiltration Intercepted",
            Description = "Border security forces have neutralized heavily armed insurgent infiltrators in sensitive frontier valleys.",
            Category = "Emergency",
            RandomWeight = 0.35,
            CooldownMonths = 8,
            Choices = new()
            {
                new("CH_BORDER_SURGE", "Deploy Specialized Quick-Reaction Border Brigades", "Fortify surveillance outposts with thermal cameras and deploy mechanized response teams.", 250_000_000m, 5.0, 5.0, 0.0),
                new("CH_BORDER_DIPLOMATIC", "Lodge Formal Diplomatic Protest and Maintain Status Quo", "Summon the neighboring envoy to present evidence without escalating troop deployments.", 10_000_000m, -3.0, 1.0, 0.0)
            }
        });

        return registry;
    }
}
