export interface ResourceStockpile {
  food: number;
  energy: number;
  oil: number;
  iron: number;
  uranium: number;
  consumerGoods: number;
}

export const DEFAULT_STARTING_STOCKPILE: ResourceStockpile = {
  food: 1_000_000.0,
  energy: 500_000.0,
  oil: 250_000.0,
  iron: 200_000.0,
  uranium: 50_000.0,
  consumerGoods: 100_000.0
};
