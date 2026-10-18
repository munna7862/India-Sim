namespace WorldState.Domain.Models;

public record ResourceStockpile(
    double Food,
    double Energy,
    double Oil,
    double Iron,
    double Uranium,
    double ConsumerGoods
)
{
    public static ResourceStockpile DefaultStartingStockpile() =>
        new(
            Food: 1_000_000.0,
            Energy: 500_000.0,
            Oil: 250_000.0,
            Iron: 200_000.0,
            Uranium: 50_000.0,
            ConsumerGoods: 100_000.0
        );
}
