public class BankData
{
  public int StartingCoins { get; set; }
  public int BuyCost { get; set; }
  public int RerollCost { get; set; }
  public int SellValue { get; set; }
  public int? SellLevelMultiplier { get; set; }
  public int? SellLevelAdditive { get; set; }

  public bool IsEmpty()
  {
    return StartingCoins == default(int)
        && BuyCost == default(int)
        && RerollCost == default(int)
        && SellValue == default(int)
        && SellLevelMultiplier == null
        && SellLevelAdditive == null;
  }
}