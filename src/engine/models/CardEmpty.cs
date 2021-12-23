using System.Collections.Generic;

public class CardEmpty : Card
{
  public CardEmpty()
  {
    Name = "Empty Slot";
    Description = "";
    BaseMove = "1";
    Abilities = Abilities;
    LevelValues = new List<LevelValue>();
  }
}