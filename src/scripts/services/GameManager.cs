using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class GameManager
{
  public static bool FFA { get; set; } = true;
  public static bool VsBots { get; set; } = false;
  public static bool ShowTutorial { get; set; } = true;
  public static Player LocalPlayer { get; set; }
  public static Score Score { get; set; } = new Score();
  public static RaceHistory RaceHistory { get; set; } = new RaceHistory();
  public static int CurrentRace { get; set; } = 0;
  public static int TotalRaces { get; set; } = 9;
  public static int LifeTotal { get; set; } = 10;
  public static CalculationLayer CalcLayer { get; set; } = new CalculationLayer();
  public static PrepEngine PrepEngine { get; set; } = new PrepEngine();
  public static NameGenerator NameGenerator { get; set; }
  public static List<string> CharacterSkins { get; set; } = new List<string>();
  public static int ShopSize { get; set; } = GameData.StartingShopInventorySize;
  public static IEnumerable<Player> Opponents { get; set; }

  public static void Reset()
  {
    LocalPlayer = null;
    Score = new Score();
    RaceHistory = new RaceHistory();
    CurrentRace = 0;
    TotalRaces = 9;
    LifeTotal = 10;
    CalcLayer = new CalculationLayer();
    PrepEngine = new PrepEngine();
    CharacterSkins = new List<string>();
    ShopSize = GameData.StartingShopInventorySize;
  }
}