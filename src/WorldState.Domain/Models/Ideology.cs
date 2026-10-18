namespace WorldState.Domain.Models;

public enum IdeologyType
{
    ConstitutionalDemocracy,
    DemocraticSocialism,
    AuthoritarianRepublic,
    NationalistFederation,
    Technocracy
}

public record IdeologyDefinition(
    IdeologyType Type,
    string DisplayName,
    string Description,
    double TaxEfficiencyMultiplier,
    double StabilityModifier,
    double ApprovalModifier,
    decimal TransitionCost
)
{
    public static readonly Dictionary<IdeologyType, IdeologyDefinition> Definitions = new()
    {
        [IdeologyType.ConstitutionalDemocracy] = new(
            Type: IdeologyType.ConstitutionalDemocracy,
            DisplayName: "Constitutional Democracy",
            Description: "Representative democracy governed by the rule of law. Boosts public approval and maintains balanced institutions.",
            TaxEfficiencyMultiplier: 1.0,
            StabilityModifier: 0.0,
            ApprovalModifier: 5.0,
            TransitionCost: 1_000_000_000m
        ),
        [IdeologyType.DemocraticSocialism] = new(
            Type: IdeologyType.DemocraticSocialism,
            DisplayName: "Democratic Socialism",
            Description: "Universal social welfare and democratic economic planning. Enhances living standards and equality.",
            TaxEfficiencyMultiplier: 0.95,
            StabilityModifier: 2.0,
            ApprovalModifier: 8.0,
            TransitionCost: 1_200_000_000m
        ),
        [IdeologyType.AuthoritarianRepublic] = new(
            Type: IdeologyType.AuthoritarianRepublic,
            DisplayName: "Authoritarian Republic",
            Description: "Centralized executive control with strict internal security. Significantly increases stability at the cost of civil approval.",
            TaxEfficiencyMultiplier: 1.05,
            StabilityModifier: 15.0,
            ApprovalModifier: -10.0,
            TransitionCost: 1_500_000_000m
        ),
        [IdeologyType.NationalistFederation] = new(
            Type: IdeologyType.NationalistFederation,
            DisplayName: "Nationalist Federation",
            Description: "Strong patriotic federal unity and prioritized defense capabilities. Increases stability and national solidarity.",
            TaxEfficiencyMultiplier: 1.0,
            StabilityModifier: 8.0,
            ApprovalModifier: 2.0,
            TransitionCost: 1_000_000_000m
        ),
        [IdeologyType.Technocracy] = new(
            Type: IdeologyType.Technocracy,
            DisplayName: "Technocracy",
            Description: "Data-driven governance by specialized scientific and industrial experts. Maximizes economic and industrial efficiency.",
            TaxEfficiencyMultiplier: 1.10,
            StabilityModifier: 5.0,
            ApprovalModifier: 0.0,
            TransitionCost: 1_800_000_000m
        )
    };

    public static IdeologyDefinition Get(IdeologyType type) =>
        Definitions.TryGetValue(type, out var def) ? def : Definitions[IdeologyType.ConstitutionalDemocracy];
}
