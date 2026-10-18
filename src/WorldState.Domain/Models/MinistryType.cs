namespace WorldState.Domain.Models;

public enum MinistryType
{
    Health,
    Education,
    Infrastructure,
    Culture,
    Sports,
    Tourism,
    Justice,
    InternalAffairs,
    Defense,
    SecurityIntelligence,
    Finance,
    IndustryConstruction,
    Environment
}

public enum FundingLevel
{
    Zero = 0,
    Low = 25,
    Medium = 50,
    High = 75,
    Maximum = 100
}

public record Minister(
    string Id,
    string Name,
    double Competence, // 0.0 to 1.0 multiplier
    decimal MonthlySalary
);

public record MinistryState(
    MinistryType Type,
    Minister? ActiveMinister,
    FundingLevel Funding
)
{
    public double CalculateEfficiency()
    {
        double competence = ActiveMinister?.Competence ?? 0.70;
        double fundingMultiplier = Funding switch
        {
            FundingLevel.Zero => 0.0,
            FundingLevel.Low => 0.70,
            FundingLevel.Medium => 1.0,
            FundingLevel.High => 1.20,
            FundingLevel.Maximum => 1.35,
            _ => 1.0
        };
        return Math.Clamp(competence * fundingMultiplier, 0.0, 1.0);
    }
}

