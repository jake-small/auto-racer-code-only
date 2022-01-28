using System;

public class Bank
{
  private int _buyCost;
  private int _rerollCost;
  private int _sellValue;
  private int _startingCoins;

  public int CoinTotal { get; private set; }

  public Bank(BankData bankData)
  {
    _buyCost = bankData.BuyCost;
    _rerollCost = bankData.RerollCost;
    _sellValue = bankData.SellValue;
    _startingCoins = bankData.StartingCoins;
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
      Console.WriteLine("Paid for card");
      CoinTotal = CoinTotal - _buyCost;
      GameManager.PrepEngine.CalculateOnBuyAbilities();
      GameManager.PrepEngine.CalculateOnBoughtAbilities(card);
      return new BankActionResult(true, CoinTotal);
    }
    Console.WriteLine("Can't afford to buy card");
    return new BankActionResult(false);
  }

  public BankActionResult Sell(Card card)
  {
    Console.WriteLine("Sold card");
    CoinTotal = CoinTotal + _sellValue;
    GameManager.PrepEngine.CalculateOnSellAbilities();
    GameManager.PrepEngine.CalculateOnSoldAbilities(card);
    return new BankActionResult(true, CoinTotal);
  }

  public BankActionResult Reroll()
  {
    if (CoinTotal >= _rerollCost)
    {
      Console.WriteLine("Paid for reroll");
      CoinTotal = CoinTotal - _rerollCost;
      GameManager.PrepEngine.CalculateOnRerollAbilities();
      return new BankActionResult(true, CoinTotal);
    }
    Console.WriteLine("Can't afford to reroll");
    return new BankActionResult(false);
  }

  public int AddCoins(int amount)
  {
    Console.WriteLine($"{amount} coins added to bank. New total is {CoinTotal}");
    return CoinTotal += amount;
  }
}