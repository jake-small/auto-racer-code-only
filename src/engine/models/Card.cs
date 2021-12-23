using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Card
{
  public string Name { private get; set; }
  public string Description { private get; set; }
  public string BaseMove { private get; set; }
  public int Tier { get; set; }
  public Abilities Abilities { get; set; }
  public List<LevelValue> LevelValues { get; set; }
  public int Level { get; set; } = 1;
  public int Exp { get; set; } = 0;

  public Card GetLeveledCard()
  {
    return CalculationLayer.ApplyLevelValues(this);
  }

  public string GetName()
  {
    return CalculationLayer.ApplyLevelValues(this, Name, Level);
  }

  public string GetRawName()
  {
    return Name;
  }
  public string GetDescription()
  {
    return CalculationLayer.ApplyLevelValues(this, Description, Level);
  }

  public string GetRawDescription()
  {
    return Description;
  }

  public string GetBaseMove()
  {
    return CalculationLayer.ApplyLevelValues(this, BaseMove, Level);
  }

  public string GetRawBaseMove()
  {
    return BaseMove;
  }
}