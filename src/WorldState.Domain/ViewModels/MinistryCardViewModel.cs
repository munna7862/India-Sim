using WorldState.Domain.Models;

namespace WorldState.Domain.ViewModels;

public record MinistryCardViewModel(
    MinistryType Type,
    string DisplayName,
    string MinisterName,
    string CompetencePercent,
    string MonthlySalaryFormatted,
    string FundingLevelName,
    string OperationalCostFormatted
)
{
    public static MinistryCardViewModel FromState(MinistryState state)
    {
        string ministerName = state.ActiveMinister != null
            ? state.ActiveMinister.Name
            : "Position Vacant";

        string competence = state.ActiveMinister != null
            ? $"{state.ActiveMinister.Competence * 100:F0}%"
            : "N/A";

        string salary = state.ActiveMinister != null
            ? $"{HudViewModel.FormatCurrency(state.ActiveMinister.MonthlySalary)} / mo"
            : "$0 / mo";

        decimal operationalCost = (int)state.Funding * 1_000_000m;

        return new MinistryCardViewModel(
            Type: state.Type,
            DisplayName: FormatMinistryName(state.Type),
            MinisterName: ministerName,
            CompetencePercent: competence,
            MonthlySalaryFormatted: salary,
            FundingLevelName: state.Funding.ToString(),
            OperationalCostFormatted: $"{HudViewModel.FormatCurrency(operationalCost)} / mo"
        );
    }

    private static string FormatMinistryName(MinistryType type) => type switch
    {
        MinistryType.SecurityIntelligence => "Security & Intelligence",
        MinistryType.IndustryConstruction => "Industry & Construction",
        MinistryType.InternalAffairs => "Internal Affairs & Police",
        _ => $"Ministry of {type}"
    };
}
