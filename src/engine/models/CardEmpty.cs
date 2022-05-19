using System.Collections.Generic;

public class CardEmpty : Card
{
  public CardEmpty(int basemove = -5)
  {
    Name = "Empty Slot";
    Description = "";
    BaseMove = basemove;
    Abilities = Abilities;
    LevelValues = new List<LevelValue>();
  }
}