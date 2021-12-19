using System.Collections.Generic;

public class PlayerTurnResult
{
  public Player Player { get; set; }
  public Dictionary<int, int> TokensGiven { get; set; }
  public int Movement { get; set; }
  public bool DidWin { get; set; }
}