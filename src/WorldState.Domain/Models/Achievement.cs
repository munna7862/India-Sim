namespace WorldState.Domain.Models;

public class AchievementDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public string ConditionType { get; set; } = string.Empty;
    public string? TargetKey { get; set; }
    public double RequiredThreshold { get; set; }
    public string? SecondaryTargetKey { get; set; }
    public double SecondaryThreshold { get; set; }
    public decimal RewardTreasury { get; set; }
    public int RewardPrestigeScore { get; set; }
    public string IconKey { get; set; } = "trophy";

    public (bool IsMet, double ProgressPercent) Evaluate(CountryState country, GameSession session)
    {
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(session);

        switch (ConditionType)
        {
            case "PolicyCount":
                int activePolicies = country.ActivePolicies.Count;
                double polProg = Math.Clamp(activePolicies / RequiredThreshold, 0.0, 1.0);
                return (activePolicies >= RequiredThreshold, polProg * 100.0);

            case "FacilityCount":
                int f1 = country.Facilities.GetValueOrDefault(TargetKey ?? "", 0);
                double p1 = Math.Clamp(f1 / RequiredThreshold, 0.0, 1.0);
                if (!string.IsNullOrEmpty(SecondaryTargetKey) && SecondaryThreshold > 0)
                {
                    int f2 = country.Facilities.GetValueOrDefault(SecondaryTargetKey, 0);
                    double p2 = Math.Clamp(f2 / SecondaryThreshold, 0.0, 1.0);
                    double combined = (p1 + p2) / 2.0;
                    return (f1 >= RequiredThreshold && f2 >= SecondaryThreshold, combined * 100.0);
                }
                return (f1 >= RequiredThreshold, p1 * 100.0);

            case "PactCount":
                int pacts = country.DiplomaticRelations.Values.Count(r => r.HasActiveNonAggressionPact(session.Clock.CurrentDate));
                double pactProg = Math.Clamp(pacts / RequiredThreshold, 0.0, 1.0);
                return (pacts >= RequiredThreshold, pactProg * 100.0);

            case "DebtAndTreasury":
                bool debtZero = country.Debt <= 0m;
                double treasProg = Math.Clamp((double)(country.Treasury / (decimal)RequiredThreshold), 0.0, 1.0);
                bool met = debtZero && country.Treasury >= (decimal)RequiredThreshold;
                double prog = debtZero ? (50.0 + treasProg * 50.0) : (treasProg * 50.0);
                return (met, Math.Clamp(prog, 0.0, 100.0));

            case "ApprovalAndLiving":
                double appProg = Math.Clamp(country.ApprovalRating / RequiredThreshold, 0.0, 1.0);
                double livProg = SecondaryThreshold > 0 ? Math.Clamp(country.QualityOfLiving / SecondaryThreshold, 0.0, 1.0) : 1.0;
                bool isAppMet = country.ApprovalRating >= RequiredThreshold && (SecondaryThreshold <= 0 || country.QualityOfLiving >= SecondaryThreshold);
                return (isAppMet, ((appProg + livProg) / 2.0) * 100.0);

            case "MilitaryAndDrills":
                int totalUnits = country.MilitaryUnits.Values.Sum();
                double unitProg = Math.Clamp(totalUnits / RequiredThreshold, 0.0, 1.0);
                bool drillsReady = session.Cooldowns.IsOnCooldown(country.Id, "ArmyDrills", session.Clock.CurrentDate);
                bool milMet = totalUnits >= RequiredThreshold && drillsReady;
                double milScore = (unitProg * 0.70 + (drillsReady ? 0.30 : 0.0)) * 100.0;
                return (milMet, Math.Clamp(milScore, 0.0, 100.0));

            case "CorruptionThreshold":
                // Target: corruption <= RequiredThreshold
                bool corruptMet = country.CorruptionIndex <= RequiredThreshold;
                double corruptProg = country.CorruptionIndex <= RequiredThreshold
                    ? 100.0
                    : Math.Clamp((100.0 - country.CorruptionIndex) / (100.0 - RequiredThreshold) * 100.0, 0.0, 100.0);
                return (corruptMet, corruptProg);

            case "CompletedTasksCount":
                int claimedTasks = session.ClaimedRewardIds.Count;
                double taskProg = Math.Clamp(claimedTasks / RequiredThreshold, 0.0, 1.0);
                return (claimedTasks >= RequiredThreshold, taskProg * 100.0);

            default:
                return (false, 0.0);
        }
    }
}

public class AchievementProgress
{
    public string AchievementId { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public DateOnly? UnlockedDate { get; set; }
    public bool IsRewardClaimed { get; set; }
    public DateOnly? ClaimedDate { get; set; }
    public double ProgressPercent { get; set; }
}
