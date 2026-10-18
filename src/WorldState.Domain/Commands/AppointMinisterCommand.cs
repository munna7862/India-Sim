using WorldState.Domain.Models;

namespace WorldState.Domain.Commands;

public class AppointMinisterCommand : ICommand
{
    public string CountryId { get; }
    public MinistryType Ministry { get; }
    public Minister Candidate { get; }
    public decimal HiringFee { get; }

    public AppointMinisterCommand(
        string countryId,
        MinistryType ministry,
        Minister candidate,
        decimal hiringFee = 10_000_000m // $10M hiring fee
    )
    {
        CountryId = countryId;
        Ministry = ministry;
        Candidate = candidate;
        HiringFee = hiringFee;
    }

    public CommandResult Execute(GameSession session)
    {
        if (!session.Countries.TryGetValue(CountryId, out var country))
        {
            return CommandResult.Failure("CountryNotFound", $"Country {CountryId} not found in world state.");
        }

        if (country.Treasury < HiringFee)
        {
            return CommandResult.Failure(
                "InsufficientFunds",
                $"Country treasury ({country.Treasury:C0}) cannot cover hiring fee of {HiringFee:C0}."
            );
        }

        if (!country.Ministries.TryGetValue(Ministry, out var ministryState))
        {
            ministryState = new MinistryState(Ministry, null, FundingLevel.Medium);
        }

        // Apply state mutation
        country.Treasury -= HiringFee;
        country.Ministries[Ministry] = ministryState with { ActiveMinister = Candidate };

        return CommandResult.Success();
    }
}
