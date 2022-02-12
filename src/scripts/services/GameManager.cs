using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class GameManager
{
  public static Player LocalPlayer { get; set; }
  public static Score Score { get; set; } = new Score();
  public static RaceHistory RaceHistory { get; set; } = new RaceHistory();
  public static int RaceNumber { get; set; } = 0;
  public static int LifeTotal { get; set; } = 10;
  public static PrepEngine PrepEngine { get; set; } = new PrepEngine();
}