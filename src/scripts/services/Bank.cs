using System;

public class Bank : FileLoader
{
  public BankData BankData { get; private set; }
  public int BuyCost { get; private set; }
  public int RerollCost { get; private set; }
  public int SellValue { get; private set; }
  public int SellLevelMultiplier { get; private set; }
  public int SellLevelAdditive { get; private set; }
  public int StartingCoins { get; private set; }
  private bool _shouldLog;

  public int CoinTotal { get; private set; }

  public Bank(string bankDataFile, bool shouldLog = true)
  {
    BankData = LoadJsonData<BankData>(bankDataFile);
    BuyCost = BankData.BuyCost;
    RerollCost = BankData.RerollCost;
    SellValue = BankData.SellValue;
    SellLevelMultiplier = BankData.SellLevelMultiplier ?? 1;
    SellLevelAdditive = BankData.SellLevelAdditive ?? 0;
    StartingCoins = BankData.StartingCoins;
    _shouldLog = shouldLog;
  }
  public Bank(BankData bankData, bool shouldLog = true)
  {
    BuyCost = bankData.BuyCost;
    RerollCost = bankData.RerollCost;
    SellValue = bankData.SellValue;
    SellLevelMultiplier = bankData.SellLevelMultiplier ?? 1;
    SellLevelAdditive = bankData.SellLevelAdditive ?? 0;
    StartingCoins = bankData.StartingCoins;
    _shouldLog = shouldLog;
  }

  public int SetStartingCoins()
  {
    CoinTotal = StartingCoins;
    return CoinTotal;
  }

  public BankActionResult Buy(Card card)
  {
    if (CoinTotal >= BuyCost)
    {
      EngineTesting.Log("Paid for card", _shouldLog);
      CoinTotal = CoinTotal - BuyCost;
      GameManager.PrepEngine.CalculateOnBuyAbilities();
      GameManager.PrepEngine.CalculateOnBoughtAbilities(card);
      return new BankActionResult(true, CoinTotal);
    }
    EngineTesting.Log("Can't afford to buy card", _shouldLog);
    return new BankActionResult(false);
  }

  public BankActionResult Sell(Card card)
  {
    EngineTesting.Log("Sold card", _shouldLog);
    CoinTotal = CoinTotal + GetSellValue(card);
    GameManager.PrepEngine.CalculateOnSellAbilities();
    GameManager.PrepEngine.CalculateOnSoldAbilities(card);
    return new BankActionResult(true, CoinTotal);
  }

  public BankActionResult Reroll()
  {
    if (CoinTotal >= RerollCost)
    {
      EngineTesting.Log("Paid for reroll", _shouldLog);
      CoinTotal = CoinTotal - RerollCost;
      GameManager.PrepEngine.CalculateOnRerollAbilities();
      return new BankActionResult(true, CoinTotal);
    }
    EngineTesting.Log("Can't afford to reroll", _shouldLog);
    return new BankActionResult(false);
  }

  public int AddCoins(int amount)
  {
    EngineTesting.Log($"{amount} coins added to bank. New total is {CoinTotal}", _shouldLog);
    return CoinTotal += amount;
  }

  public int GetSellValue(Card card)
  {
    var levelUps = card.Level - 1;
    var multiplier = SellLevelMultiplier < 2 || levelUps is 0 ? 1 : SellLevelMultiplier * levelUps;
    var additive = SellLevelAdditive * levelUps;
    return (SellValue * multiplier) + additive;
  }
}