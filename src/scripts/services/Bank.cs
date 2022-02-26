using System;

public class Bank
{
  private int _buyCost;
  private int _rerollCost;
  private int _sellValue;
  private int _sellLevelMultiplier;
  private int _sellLevelAdditive;
  private int _startingCoins;
  private bool _shouldLog;

  public int CoinTotal { get; private set; }

  public Bank(BankData bankData, bool shouldLog = true)
  {
    _buyCost = bankData.BuyCost;
    _rerollCost = bankData.RerollCost;
    _sellValue = bankData.SellValue;
    _sellLevelMultiplier = bankData.SellLevelMultiplier ?? 1;
    _sellLevelAdditive = bankData.SellLevelAdditive ?? 0;
    _startingCoins = bankData.StartingCoins;
    _shouldLog = shouldLog;
  }

  public int SetStartingCoins()
  {
    CoinTotal = _startingCoins;
    return CoinTotal;
  }

  public BankActionResult Buy(Card card)
  {
    if (CoinTotal >= _buyCost)
    {
      EngineTesting.Log("Paid for card", _shouldLog);
      CoinTotal = CoinTotal - _buyCost;
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
    if (CoinTotal >= _rerollCost)
    {
      EngineTesting.Log("Paid for reroll", _shouldLog);
      CoinTotal = CoinTotal - _rerollCost;
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
    var multiplier = _sellLevelMultiplier < 2 || levelUps is 0 ? 1 : _sellLevelMultiplier * levelUps;
    var additive = _sellLevelAdditive * levelUps;
    return (_sellValue * multiplier) + additive;
  }
}