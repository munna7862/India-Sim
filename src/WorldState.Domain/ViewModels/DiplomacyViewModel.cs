using WorldState.Domain.Models;

namespace WorldState.Domain.ViewModels;

public record DiplomaticCountryCardViewModel(
    string CountryId,
    string CountryName,
    double RelationsScore,
    string RelationsText,
    string StatusText,
    bool IsAtWar,
    bool HasActiveNap,
    string NapExpiryText,
    int ActiveTradeContractCount,
    string TradeSummaryText,
    bool CanSignNap,
    bool CanSendAid,
    bool CanDeclareWar,
    List<TradeContract> TradeContracts
);

public record DiplomacyViewModel(
    string PlayerCountryId,
    string PlayerCountryName,
    int TotalForeignNations,
    int ActiveNapCount,
    int TotalActiveTradeContracts,
    int AtWarCount,
    IReadOnlyList<DiplomaticCountryCardViewModel> ForeignNations
)
{
    public static DiplomacyViewModel FromSession(GameSession session, string? playerCountryId = null)
    {
        ArgumentNullException.ThrowIfNull(session);

        string playerId = playerCountryId ?? session.PlayerCountryId;
        var playerCountry = session.Countries.TryGetValue(playerId, out var pc) ? pc : null;
        string playerName = playerCountry?.Name ?? playerId;

        var cards = new List<DiplomaticCountryCardViewModel>();
        int napCount = 0;
        int tradeCount = 0;
        int warCount = 0;

        if (playerCountry != null)
        {
            foreach (var (foreignId, foreignState) in session.Countries)
            {
                if (string.Equals(foreignId, playerId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var stance = playerCountry.GetRelationsWith(foreignId);
                bool hasNap = stance.HasActiveNonAggressionPact(session.Clock.CurrentDate);
                if (hasNap) napCount++;
                if (stance.IsAtWar) warCount++;
                tradeCount += stance.ActiveTradeContracts.Count;

                string napText = hasNap && stance.NonAggressionPactUntil.HasValue
                    ? $"NAP until {stance.NonAggressionPactUntil.Value:dd MMM yyyy}"
                    : "No Non-Aggression Pact";

                string tradeText = stance.ActiveTradeContracts.Count > 0
                    ? $"{stance.ActiveTradeContracts.Count} active contract(s)"
                    : "No active trade contracts";

                bool canSignNap = !stance.IsAtWar && !hasNap && stance.Relations >= -20.0 && playerCountry.Treasury >= 50_000_000m;
                bool canSendAid = !stance.IsAtWar && playerCountry.Treasury >= 250_000_000m && !session.Cooldowns.IsOnCooldown(playerId, $"DiplomaticGift_{foreignId}", session.Clock.CurrentDate);
                bool canDeclareWar = !stance.IsAtWar && !hasNap;

                cards.Add(new DiplomaticCountryCardViewModel(
                    CountryId: foreignId,
                    CountryName: foreignState.Name,
                    RelationsScore: stance.Relations,
                    RelationsText: $"{(stance.Relations >= 0 ? "+" : "")}{stance.Relations:F0}",
                    StatusText: stance.GetStatusText(),
                    IsAtWar: stance.IsAtWar,
                    HasActiveNap: hasNap,
                    NapExpiryText: napText,
                    ActiveTradeContractCount: stance.ActiveTradeContracts.Count,
                    TradeSummaryText: tradeText,
                    CanSignNap: canSignNap,
                    CanSendAid: canSendAid,
                    CanDeclareWar: canDeclareWar,
                    TradeContracts: new List<TradeContract>(stance.ActiveTradeContracts)
                ));
            }
        }

        return new DiplomacyViewModel(
            PlayerCountryId: playerId,
            PlayerCountryName: playerName,
            TotalForeignNations: cards.Count,
            ActiveNapCount: napCount,
            TotalActiveTradeContracts: tradeCount,
            AtWarCount: warCount,
            ForeignNations: cards.OrderByDescending(c => c.RelationsScore).ToList()
        );
    }
}
