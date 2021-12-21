using System.Collections.Generic;

public class CardEmpty : Card
{
  public CardEmpty()
  {
    Name = "Empty Slot";
    BaseMove = "1";
    Abilities = new List<Ability>();
    Tier = new Dictionary<int, Dictionary<string, string>>();
  }
}