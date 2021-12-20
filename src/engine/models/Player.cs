using System.Collections.Generic;

public class Player
{
  public int Id { get; set; }
  public Dictionary<int, Card> Cards { get; set; }
  public int Position { get; set; } = 0;
}