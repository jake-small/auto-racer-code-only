using Godot;

public class Bank
{
  private int _buyCost = 3;
  private int _rerollCost = 1;
  private int _sellValue = 1;
  private int _startingCoins = 10;
  private int _coinTotal;

  public int SetStartingCoins()
  {
    _coinTotal = _startingCoins;
    return _coinTotal;
  }

  public BankAction Buy()
  {
    if (_coinTotal >= _buyCost)
    {
      GD.Print("Paid for card");
      _coinTotal = _coinTotal - _buyCost;
      return new BankAction(true, _coinTotal);
    }
    GD.Print("Can't afford to buy card");
    return new BankAction(false);
  }

  public BankAction Sell()
  {
    GD.Print("Sold card");
    _coinTotal = _coinTotal + _sellValue;
    return new BankAction(true, _coinTotal);
  }

  public BankAction Reroll()
  {
    if (_coinTotal >= _rerollCost)
    {
      GD.Print("Paid for reroll");
      _coinTotal = _coinTotal - _rerollCost;
      return new BankAction(true, _coinTotal);
    }
    GD.Print("Can't afford to reroll");
    return new BankAction(false);
  }
}