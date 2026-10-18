namespace WorldState.Domain.ViewModels;

public record ActionPromptViewModel(
    string ActionTitle,
    string Description,
    decimal OneTimeCost,
    string OneTimeCostFormatted,
    decimal CurrentTreasury,
    string CurrentTreasuryFormatted,
    decimal ProjectedTreasury,
    string ProjectedTreasuryFormatted,
    bool CanAfford,
    IReadOnlyList<string> Prerequisites,
    bool AllPrerequisitesMet
)
{
    public static ActionPromptViewModel Create(
        string title,
        string description,
        decimal oneTimeCost,
        decimal currentTreasury,
        IReadOnlyList<string> prerequisites
    )
    {
        decimal projected = currentTreasury - oneTimeCost;
        bool canAfford = currentTreasury >= oneTimeCost;

        return new ActionPromptViewModel(
            ActionTitle: title,
            Description: description,
            OneTimeCost: oneTimeCost,
            OneTimeCostFormatted: HudViewModel.FormatCurrency(oneTimeCost),
            CurrentTreasury: currentTreasury,
            CurrentTreasuryFormatted: HudViewModel.FormatCurrency(currentTreasury),
            ProjectedTreasury: projected,
            ProjectedTreasuryFormatted: HudViewModel.FormatCurrency(projected),
            CanAfford: canAfford,
            Prerequisites: prerequisites,
            AllPrerequisitesMet: canAfford
        );
    }
}
