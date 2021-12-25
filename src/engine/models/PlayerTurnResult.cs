using System.Collections.Generic;

public class PlayerTurnResult
{
  public Player Player { get; }
  public Dictionary<int, List<Token>> TokensGiven { get; set; } = new Dictionary<int, List<Token>>();
  public int Movement { get; set; } = 0;
  public bool DidWin { get; set; } = false;

  public PlayerTurnResult(Player player)
  {
    Player = player;
  }
}