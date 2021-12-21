public class BankActionResult
{
  public bool Success { get; }
  public int CoinTotal { get; }

  public BankActionResult(bool success, int coinTotal)
  {
    Success = success;
    CoinTotal = coinTotal;
  }

  public BankActionResult(bool success)
  {
    Success = success;
  }
}