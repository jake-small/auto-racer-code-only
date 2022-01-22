using System.Collections.Generic;

public class Card
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


  public Card() { }
  public Card(Card anotherCard)
  {
    Name = anotherCard.Name;
    Icon = anotherCard.Icon;
    Description = anotherCard.Description;
    BaseMove = anotherCard.BaseMove;
    Tier = anotherCard.Tier;
    Abilities = anotherCard.Abilities;
    LevelValues = anotherCard.LevelValues;
    Level = anotherCard.Level;
    Exp = anotherCard.Exp;
  }

  public Card Clone() { return new Card(this); }

  public Card GetLeveledCard()
  {
    return _calcLayer.ApplyLevelValues(this);
  }

  public string GetName()
  {
    return _calcLayer.ApplyLevelValues(this, Name, Level);
  }

  public string GetRawName()
  {
    return Name;
  }
  public string GetDescription()
  {
    return _calcLayer.ApplyLevelValues(this, Description, Level);
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