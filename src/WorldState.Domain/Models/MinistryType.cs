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
);
