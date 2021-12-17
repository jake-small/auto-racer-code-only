public class BankAction
{
  public bool Success { get; }
  public int CoinTotal { get; }

  public BankAction(bool success, int coinTotal)
  {
    Success = success;
    CoinTotal = coinTotal;
  }

  public BankAction(bool success)
  {
    Success = success;
  }
}