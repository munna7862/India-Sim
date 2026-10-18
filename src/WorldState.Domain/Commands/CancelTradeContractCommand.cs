using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class CancelTradeContractCommand : ICommand
{
    public string CountryId { get; }
    public string ContractId { get; }

    public CancelTradeContractCommand(string countryId, string contractId)
    {
        CountryId = countryId;
        ContractId = contractId;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country '{CountryId}' not found.");
        }

        TradeContract? contractToCancel = null;
        string? partnerCountryId = null;

        foreach (var (partnerId, stance) in country.DiplomaticRelations)
        {
            var match = stance.ActiveTradeContracts.FirstOrDefault(c => c.Id == ContractId);
            if (match != null)
            {
                contractToCancel = match;
                partnerCountryId = partnerId;
                break;
            }
        }

        if (contractToCancel == null || partnerCountryId == null)
        {
            return CommandResult.Failure("ContractNotFound", $"Active trade contract '{ContractId}' not found.");
        }

        // Remove from initiating country
        var ownStance = country.GetRelationsWith(partnerCountryId);
        ownStance.ActiveTradeContracts.RemoveAll(c => c.Id == ContractId);

        // Remove from partner country if present
        if (session.Countries.TryGetValue(partnerCountryId, out var partner))
        {
            var partnerStance = partner.GetRelationsWith(CountryId);
            partnerStance.ActiveTradeContracts.RemoveAll(c => c.Id == ContractId);

            // Minor diplomatic fallout for unilateral termination
            ownStance.ModifyRelations(-5.0);
            partnerStance.ModifyRelations(-5.0);
        }

        session.AddEvent(new GameEvent(
            Id: Guid.NewGuid().ToString("N"),
            Date: session.Clock.CurrentDate,
            Title: "Trade Contract Terminated",
            Message: $"{country.Name} cancelled the commercial trade agreement for {contractToCancel.MonthlyQuantity:N0} {contractToCancel.ResourceType}/mo with {partnerCountryId}.",
            Category: EventCategory.Diplomacy,
            Severity: EventSeverity.Warning,
            ActorCountryId: CountryId
        ));

        return CommandResult.Success();
    }
}
