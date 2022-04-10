using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class GameManager
{
  public static Player LocalPlayer { get; set; }
  public static Score Score { get; set; } = new Score();
  public static RaceHistory RaceHistory { get; set; } = new RaceHistory();
  public static int CurrentRace { get; set; } = 0;
  public static int TotalRaces { get; set; } = 10;
  public static int LifeTotal { get; set; } = 10;
  public static PrepEngine PrepEngine { get; set; } = new PrepEngine();
  public static List<string> CharacterSkins { get; set; } = new List<string>();
  public static string PlayerCharacterSkin { get; set; }
}