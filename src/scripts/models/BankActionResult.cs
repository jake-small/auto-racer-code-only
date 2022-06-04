public class BankActionResult
{
  public bool Success { get; }
  public int CoinTotal { get; }
  public PrepAbilityResponse PrepAbilityResponse { get; }

  public BankActionResult(bool success, int coinTotal, PrepAbilityResponse prepAbilityResponse)
  {
    Success = success;
    CoinTotal = coinTotal;
    PrepAbilityResponse = prepAbilityResponse;
  }

  public BankActionResult(bool success, PrepAbilityResponse prepAbilityResponse)
  {
    Success = success;
    PrepAbilityResponse = prepAbilityResponse;
  }
}