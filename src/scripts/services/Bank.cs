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

  public BankActionResult Buy()
  {
    if (CoinTotal >= _buyCost)
    {
      GD.Print("Paid for card");
      CoinTotal = CoinTotal - _buyCost;
      return new BankActionResult(true, CoinTotal);
    }
    GD.Print("Can't afford to buy card");
    return new BankActionResult(false);
  }

  public BankActionResult Sell()
  {
    GD.Print("Sold card");
    CoinTotal = CoinTotal + _sellValue;
    return new BankActionResult(true, CoinTotal);
  }

  public BankActionResult Reroll()
  {
    if (CoinTotal >= _rerollCost)
    {
      GD.Print("Paid for reroll");
      CoinTotal = CoinTotal - _rerollCost;
      return new BankActionResult(true, CoinTotal);
    }
    GD.Print("Can't afford to reroll");
    return new BankActionResult(false);
  }
}