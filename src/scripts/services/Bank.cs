using Godot;

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

  public BankActionResult Buy(CardScript cardScript)
  {
    if (CoinTotal >= _buyCost)
    {
      GD.Print("Paid for card");
      CoinTotal = CoinTotal - _buyCost;
      GameManager.PrepEngine.CalculateOnBuyAbilities();
      GameManager.PrepEngine.CalculateOnBoughtAbilities(cardScript);
      return new BankActionResult(true, CoinTotal);
    }
    GD.Print("Can't afford to buy card");
    return new BankActionResult(false);
  }

  public BankActionResult Sell(CardScript cardScript)
  {
    GD.Print("Sold card");
    CoinTotal = CoinTotal + _sellValue;
    GameManager.PrepEngine.CalculateOnSellAbilities();
    GameManager.PrepEngine.CalculateOnSoldAbilities(cardScript);
    return new BankActionResult(true, CoinTotal);
  }

  public BankActionResult Reroll()
  {
    if (CoinTotal >= _rerollCost)
    {
      // GD.Print("Paid for reroll");
      CoinTotal = CoinTotal - _rerollCost;
      GameManager.PrepEngine.CalculateOnRerollAbilities();
      return new BankActionResult(true, CoinTotal);
    }
    GD.Print("Can't afford to reroll");
    return new BankActionResult(false);
  }

  public int AddCoins(int amount)
  {
    GD.Print($"{amount} coins added to bank. New total is {CoinTotal}");
    return CoinTotal += amount;
  }
}