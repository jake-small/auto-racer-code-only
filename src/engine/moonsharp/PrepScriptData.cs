
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class PrepScriptData : MoonSharpScriptData
{
  public IEnumerable<MoonSharpCard> PlayerInventory { get; set; }
  public IEnumerable<MoonSharpCard> ShopInventory { get; set; }
  public int CoinTotal { get; set; }
  public int LifeTotal { get; set; }
  public int RaceNumber { get; set; }

  public PrepScriptData(IEnumerable<CardScript> playerInventory, IEnumerable<CardScript> shopInventory, int coinTotal, int lifeTotal, int raceNumber)
  {
    PlayerInventory = playerInventory.Select(p => new MoonSharpCard(p));
    ShopInventory = shopInventory.Select(p => new MoonSharpCard(p));
    CoinTotal = coinTotal;
    LifeTotal = lifeTotal;
    RaceNumber = raceNumber;
  }
}

[MoonSharpUserData]
public class MoonSharpCard
{
  public string Name { get; set; }
  public int BaseMove { get; set; }
  public string Inventory { get; set; }
  public int Slot { get; set; }
  public int Tier { get; set; }
  public int Level { get; set; }
  public int Exp { get; set; }

  public MoonSharpCard(CardScript cardScript)
  {
    Name = cardScript.Card.GetName();
    BaseMove = cardScript.Card.BaseMove;
    Slot = cardScript.Slot;
    Inventory = cardScript.Inventory.ToString();
    Tier = cardScript.Card.Tier;
    Level = cardScript.Card.Level;
    Exp = cardScript.Card.Exp;
  }
}