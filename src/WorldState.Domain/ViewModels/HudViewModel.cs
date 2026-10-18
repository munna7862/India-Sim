using WorldState.Domain.Models;

namespace WorldState.Domain.ViewModels;

public record HudViewModel(
    string DateText,
    string SpeedText,
    bool IsPaused,
    string TreasuryText,
    bool IsDeficit,
    string PopulationText,
    string ApprovalText,
    string LivingStandardText,
    string StabilityText,
    int ActiveAlertCount
)
{
    public static HudViewModel FromSession(GameSession session)
    {
        var clock = session.Clock;
        var playerCountry = session.GetPlayerCountry();

        string dateText = clock.CurrentDate.ToString("dd MMM yyyy");
        string speedText = clock.Speed switch
        {
            ClockSpeed.Paused => "PAUSED",
            ClockSpeed.Normal => "1x",
            ClockSpeed.Fast => "2x",
            ClockSpeed.Ultra => "3x",
            _ => "1x"
        };

        if (playerCountry == null)
        {
            return new HudViewModel(
                dateText,
                speedText,
                clock.Speed == ClockSpeed.Paused,
                "$0",
                false,
                "0",
                "0%",
                "0%",
                "0%",
                0
            );
        }

        return new HudViewModel(
            DateText: dateText,
            SpeedText: speedText,
            IsPaused: clock.Speed == ClockSpeed.Paused,
            TreasuryText: FormatCurrency(playerCountry.Treasury),
            IsDeficit: playerCountry.Treasury < 0m,
            PopulationText: FormatNumber(playerCountry.Population),
            ApprovalText: $"{playerCountry.ApprovalRating:F1}%",
            LivingStandardText: $"{playerCountry.QualityOfLiving:F1}%",
            StabilityText: $"{playerCountry.Stability:F1}%",
            ActiveAlertCount: playerCountry.Treasury < 0m ? 1 : 0
        );
    }

    public static string FormatCurrency(decimal amount)
    {
        decimal abs = Math.Abs(amount);
        string sign = amount < 0 ? "-$" : "$";

        if (abs >= 1_000_000_000m)
        {
            return $"{sign}{abs / 1_000_000_000m:F1}B";
        }
        if (abs >= 1_000_000m)
        {
            return $"{sign}{abs / 1_000_000m:F1}M";
        }
        if (abs >= 1_000m)
        {
            return $"{sign}{abs / 1_000m:F0}K";
        }

        return $"{sign}{abs:F0}";
    }

    public static string FormatNumber(long number)
    {
        long abs = Math.Abs(number);
        if (abs >= 1_000_000_000L)
        {
            return $"{number / 1_000_000_000.0:F2}B";
        }
        if (abs >= 1_000_000L)
        {
            return $"{number / 1_000_000.0:F1}M";
        }
        if (abs >= 1_000L)
        {
            return $"{number / 1_000.0:F0}K";
        }

        return number.ToString();
    }
}
