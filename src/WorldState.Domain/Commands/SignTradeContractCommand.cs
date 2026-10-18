using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class SignTradeContractCommand : ICommand
{
    public static readonly HashSet<string> ValidResourceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Food",
        "Energy",
        "Oil",
        "Iron",
        "Uranium",
        "ConsumerGoods"
    };

    public string ExporterCountryId { get; }
    public string ImporterCountryId { get; }
    public string ResourceType { get; }
    public double MonthlyQuantity { get; }
    public decimal MonthlyPayment { get; }
    public int DurationMonths { get; }

    public SignTradeContractCommand(
        string exporterCountryId,
        string importerCountryId,
        string resourceType,
        double monthlyQuantity,
        decimal monthlyPayment,
        int durationMonths = 24)
    {
        ExporterCountryId = exporterCountryId;
        ImporterCountryId = importerCountryId;
        ResourceType = resourceType;
        MonthlyQuantity = monthlyQuantity;
        MonthlyPayment = monthlyPayment;
        DurationMonths = durationMonths > 0 ? durationMonths : 24;
    }

    public CommandResult Execute(GameSession session)
    {
        if (string.Equals(ExporterCountryId, ImporterCountryId, StringComparison.OrdinalIgnoreCase))
        {
            return CommandResult.Failure("InvalidContractParties", "A nation cannot establish an export-import trade contract with itself.");
        }

        if (!session.Countries.TryGetValue(ExporterCountryId, out var exporter))
        {
            return CommandResult.Failure("CountryNotFound", $"Exporter nation '{ExporterCountryId}' not found.");
        }

        if (!session.Countries.TryGetValue(ImporterCountryId, out var importer))
        {
            return CommandResult.Failure("CountryNotFound", $"Importer nation '{ImporterCountryId}' not found.");
        }

        if (!ValidResourceTypes.Contains(ResourceType))
        {
            return CommandResult.Failure("UnknownResourceType", $"Resource '{ResourceType}' is not a valid trade commodity.");
        }

        if (MonthlyQuantity <= 0.0 || MonthlyPayment <= 0m)
        {
            return CommandResult.Failure("InvalidQuantities", "Monthly trade quantity and payment must both be strictly positive.");
        }

        var exporterStance = exporter.GetRelationsWith(ImporterCountryId);
        var importerStance = importer.GetRelationsWith(ExporterCountryId);

        if (exporterStance.IsAtWar)
        {
            return CommandResult.Failure("AtWar", $"Cannot sign commercial trade agreements with '{importer.Name}' during active hostilities.");
        }

        if (exporterStance.Relations < -30.0)
        {
            return CommandResult.Failure(
                "RelationsTooLow",
                $"Relations with {importer.Name} ({exporterStance.Relations:F0}) are too poor to agree on commercial trade terms. Minimum required: -30."
            );
        }

        // Check if identical active contract exists
        bool contractExists = exporterStance.ActiveTradeContracts.Any(c =>
            string.Equals(c.ImporterCountryId, ImporterCountryId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.ResourceType, ResourceType, StringComparison.OrdinalIgnoreCase));

        if (contractExists)
        {
            return CommandResult.Failure(
                "DuplicateContract",
                $"An active trade contract for {ResourceType} already exists between {exporter.Name} and {importer.Name}."
            );
        }

        // Verify exporter has sufficient stockpile for initial delivery
        double currentStock = GetStockpileAmount(exporter.Stockpiles, ResourceType);
        if (currentStock < MonthlyQuantity)
        {
            return CommandResult.Failure(
                "InsufficientStockpile",
                $"Exporter {exporter.Name} lacks adequate {ResourceType} stockpile ({currentStock:N0}) to initiate monthly export of {MonthlyQuantity:N0}."
            );
        }

        var contract = new TradeContract(
            Id: $"TC_{ExporterCountryId}_{ImporterCountryId}_{ResourceType}_{Guid.NewGuid():N}",
            ExporterCountryId: ExporterCountryId,
            ImporterCountryId: ImporterCountryId,
            ResourceType: ResourceType,
            MonthlyQuantity: MonthlyQuantity,
            MonthlyPayment: MonthlyPayment,
            StartDate: session.Clock.CurrentDate,
            ExpiryDate: session.Clock.CurrentDate.AddMonths(DurationMonths)
        );

        exporterStance.ActiveTradeContracts.Add(contract);
        importerStance.ActiveTradeContracts.Add(contract);

        exporterStance.ModifyRelations(5.0);
        importerStance.ModifyRelations(5.0);

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Commercial Trade Agreement Ratified",
            Message: $"{exporter.Name} will export {MonthlyQuantity:N0} {ResourceType}/mo to {importer.Name} in exchange for ${MonthlyPayment / 1_000_000m:F1}M/mo until {contract.ExpiryDate:dd MMM yyyy}.",
            Category: EventCategory.Diplomacy,
            Severity: EventSeverity.Info,
            ActorCountryId: ExporterCountryId
        ));

        return CommandResult.Success();
    }

    private static double GetStockpileAmount(ResourceStockpile stockpile, string resourceType) => resourceType.ToLowerInvariant() switch
    {
        "food" => stockpile.Food,
        "energy" => stockpile.Energy,
        "oil" => stockpile.Oil,
        "iron" => stockpile.Iron,
        "uranium" => stockpile.Uranium,
        "consumergoods" => stockpile.ConsumerGoods,
        _ => 0.0
    };
}
