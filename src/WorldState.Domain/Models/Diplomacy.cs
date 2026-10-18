namespace WorldState.Domain.Models;

public enum DiplomaticGiftTier
{
    Small,
    Medium,
    Large
}

public record TradeContract(
    string Id,
    string ExporterCountryId,
    string ImporterCountryId,
    string ResourceType,
    double MonthlyQuantity,
    decimal MonthlyPayment,
    DateOnly StartDate,
    DateOnly? ExpiryDate = null
);

public class DiplomaticStance
{
    public double Relations { get; set; } = 0.0; // -100.0 (Hostile/War) to +100.0 (Allied)
    public bool IsAtWar { get; set; } = false;
    public DateOnly? NonAggressionPactUntil { get; set; } = null;
    public bool HasAlliance { get; set; } = false;
    public List<TradeContract> ActiveTradeContracts { get; set; } = new();

    public bool HasActiveNonAggressionPact(DateOnly currentDate) =>
        NonAggressionPactUntil.HasValue && NonAggressionPactUntil.Value >= currentDate;

    public void ModifyRelations(double delta)
    {
        Relations = Math.Clamp(Relations + delta, -100.0, 100.0);
    }

    public string GetStatusText()
    {
        if (IsAtWar) return "At War";
        if (HasAlliance) return "Allied";
        if (Relations >= 60.0) return "Strategic Partner";
        if (Relations >= 20.0) return "Friendly";
        if (Relations >= 0.0) return "Neutral";
        if (Relations >= -50.0) return "Cold";
        return "Hostile";
    }
}
