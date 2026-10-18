using WorldState.Domain.Models;

namespace WorldState.Domain.ViewModels;

public record GovernmentViewModel(
    string CountryId,
    string CountryName,
    IdeologyType ActiveIdeology,
    string IdeologyDisplayName,
    string IdeologyDescription,
    int IdeologyCooldownMonths,
    bool CanChangeIdeology,
    IReadOnlyList<IdeologyDefinition> AvailableIdeologies,
    IReadOnlyList<MinistryCardViewModel> Ministries,
    decimal TotalMonthlyPayroll,
    decimal TotalMonthlyOperationalCost,
    decimal TotalMonthlyGovernmentExpenses,
    double AverageCabinetEfficiency
)
{
    public static GovernmentViewModel FromCountry(CountryState country)
    {
        var ideologyDef = IdeologyDefinition.Get(country.Ideology);

        var ministryCards = new List<MinistryCardViewModel>();
        decimal totalPayroll = 0m;
        decimal totalOps = 0m;
        double sumEfficiency = 0.0;

        foreach (var (_, ministryState) in country.Ministries)
        {
            ministryCards.Add(MinistryCardViewModel.FromState(ministryState));
            if (ministryState.ActiveMinister != null)
            {
                totalPayroll += ministryState.ActiveMinister.MonthlySalary;
            }
            totalOps += (int)ministryState.Funding * 1_000_000m;
            sumEfficiency += ministryState.CalculateEfficiency();
        }

        double avgEfficiency = country.Ministries.Count > 0
            ? (sumEfficiency / country.Ministries.Count) * 100.0
            : 0.0;

        decimal totalExpenses = totalPayroll + totalOps + 500_000_000m; // Includes baseline admin & defense

        return new GovernmentViewModel(
            CountryId: country.Id,
            CountryName: country.Name,
            ActiveIdeology: country.Ideology,
            IdeologyDisplayName: ideologyDef.DisplayName,
            IdeologyDescription: ideologyDef.Description,
            IdeologyCooldownMonths: country.IdeologyCooldownMonths,
            CanChangeIdeology: country.IdeologyCooldownMonths <= 0,
            AvailableIdeologies: IdeologyDefinition.Definitions.Values.ToList(),
            Ministries: ministryCards,
            TotalMonthlyPayroll: totalPayroll,
            TotalMonthlyOperationalCost: totalOps,
            TotalMonthlyGovernmentExpenses: totalExpenses,
            AverageCabinetEfficiency: avgEfficiency
        );
    }
}
