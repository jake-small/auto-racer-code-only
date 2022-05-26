using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class GameManager
{
  public static bool ShowTutorial { get; set; } = true;
  public static Player LocalPlayer { get; set; }
  public static Score Score { get; set; } = new Score();
  // public static RaceHistory RaceHistory { get; set; } = new RaceHistory();
  public static int CurrentRace { get; set; } = 0;
  public static int TotalRaces { get; set; } = 10;
  public static int LifeTotal { get; set; } = 10;
  public static PrepEngine PrepEngine { get; set; } = new PrepEngine();
  public static NameGenerator NameGenerator { get; set; }
  public static List<string> CharacterSkins { get; set; } = new List<string>();

  public static void Reset()
  {
    LocalPlayer = null;
    Score = new Score();
    // RaceHistory = new RaceHistory();
    CurrentRace = 0;
    TotalRaces = 10;
    LifeTotal = 10;
    PrepEngine = new PrepEngine();
    CharacterSkins = new List<string>();
  }
}