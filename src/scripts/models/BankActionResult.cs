using System.Collections.Generic;

public class BankActionResult
{
  public bool Success { get; }
  public int CoinTotal { get; }
  public IEnumerable<PrepAbilityResult> PrepAbilityResults { get; }

  public BankActionResult(bool success, int coinTotal, IEnumerable<PrepAbilityResult> prepAbilityResults)
  {
    Success = success;
    CoinTotal = coinTotal;
    PrepAbilityResults = prepAbilityResults;
  }

  public BankActionResult(bool success, IEnumerable<PrepAbilityResult> prepAbilityResults)
  {
    Success = success;
    PrepAbilityResults = prepAbilityResults;
  }
}