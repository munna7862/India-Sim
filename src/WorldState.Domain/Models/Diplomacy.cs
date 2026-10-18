namespace WorldState.Domain.Models;

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
}
