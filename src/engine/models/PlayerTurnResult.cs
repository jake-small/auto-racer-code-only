using System.Collections.Generic;

public class PlayerTurnResult
{
  public Player Player { get; }
  public Dictionary<int, int> TokensGiven { get; set; } = new Dictionary<int, int>();
  public int Movement { get; set; } = 0;
  public bool DidWin { get; set; } = false;

  public PlayerTurnResult(Player player)
  {
    Player = player;
  }
}