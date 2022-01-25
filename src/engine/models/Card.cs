using System;
using System.Collections.Generic;

public class Card : ICloneable
{
  public string Name { private get; set; }
  public string Icon { get; set; }
  public string Description { private get; set; }
  public int BaseMove { get; set; }
  public int Tier { get; set; }
  public int Level { get; set; } = 1;
  public int Exp { get; set; } = 1;
  public int ExpToLvl { get; private set; } = 3;
  public Abilities Abilities { get; set; }
  public List<LevelValue> LevelValues { get; set; }

  private CalculationLayer _calcLayer = new CalculationLayer();

  public object Clone()
  {
    return new Card
    {
      Name = Name,
      Icon = Icon,
      Description = Description,
      BaseMove = BaseMove,
      Tier = Tier,
      Level = Level,
      Exp = Exp,
      ExpToLvl = ExpToLvl,
      Abilities = (Abilities)Abilities?.Clone(),
      LevelValues = LevelValues
    };
  }

  public Card GetLeveledCard()
  {
    return _calcLayer.ApplyLevelValues((Card)this.Clone());
  }

  public Card ApplyPrepFunctionValues()
  {
    return _calcLayer.ApplyPrepFunctionValues((Card)this.Clone());
  }

  public Card ApplyTokenFunctionValues(Player player, IEnumerable<Player> players)
  {
    return _calcLayer.ApplyTokenFunctionValues((Card)this.Clone(), player, players);
  }

  public string GetName()
  {
    return _calcLayer.ApplyLevelValues((Card)this.Clone(), Name, Level);
  }

  public string GetRawName()
  {
    return Name;
  }
  public string GetDescription()
  {
    return _calcLayer.ApplyLevelValues((Card)this.Clone(), Description, Level);
  }

  public string GetRawDescription()
  {
    return Description;
  }

  public void AddExp(int exp)
  {
    Exp += exp;
    if (Exp >= ExpToLvl)
    {
      Level = Level + 1;
      ExpToLvl = ExpToLvl + 1;
      Exp = 1;
    }
  }
}