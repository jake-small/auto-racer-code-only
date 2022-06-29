using System;
using System.Collections.Generic;
using System.Linq;

public class Card : ICloneable
{
  public string Guid { get; set; }
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
  public InventoryType InventoryType { get; set; }
  public bool Frozen = false;

  private const int MaxCardLevel = 3;
  private CalculationLayer _calcLayer;
  public Card()
  {
    try
    {
      _calcLayer = GameManager.CalcLayer;
    }
    catch (System.Exception)
    {
      Console.WriteLine("Warning: Unable to initialize GameManager, using a new CalculationLayer for each card. This is expensive- only use this for unit tests");
      _calcLayer = new CalculationLayer();
    }
  }

  public object Clone()
  {
    return new Card
    {
      Guid = Guid,
      Name = Name,
      Icon = Icon,
      Description = Description,
      BaseMove = BaseMove,
      Tier = Tier,
      Level = Level,
      Exp = Exp,
      ExpToLvl = ExpToLvl,
      Abilities = (Abilities)Abilities?.Clone(),
      LevelValues = LevelValues,
      InventoryType = InventoryType,
      Frozen = Frozen
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

  public string GetAbilityPhase()
  {
    if (Abilities == null || !Abilities.MoveTokenAbilities.Any())
    {
      return "";
    }
    var phases = new HashSet<string>();
    foreach (var ability in Abilities.MoveTokenAbilities)
    {
      phases.Add(ability.GetAbilityPhase().ToString().Last().ToString());
    }

    if (!phases.Any())
    {
      return "";
    }

    if (phases.Count() == 1)
    {
      return $"p{phases.First()}";
    }

    var phaseText = "p";
    foreach (var phase in phases)
    {
      phaseText = phaseText + $"{phase},";
    }
    phaseText.Remove(phaseText.Length - 2, 2); // remove last two characters from string: ", "
    return phaseText;
  }

  public bool IsMaxLevel()
  {
    return Level == MaxCardLevel;
  }

  public void AddExp(int exp)
  {
    if (Level >= MaxCardLevel)
    {
      return;
    }
    Exp += exp;
    if (Exp >= ExpToLvl)
    {
      Level = Level + 1;
      if (Level >= MaxCardLevel)
      {
        Exp = ExpToLvl;
        return;
      }
      var extraExp = Exp - ExpToLvl;
      ExpToLvl = ExpToLvl + 1;
      Exp = 1;
      AddExp(extraExp);
    }
  }

  public void CombineBaseMove(int otherBaseMove)
  {
    if (BaseMove > otherBaseMove)
    {
      BaseMove = BaseMove + 1;
      return;
    }
    BaseMove = otherBaseMove + 1;
  }
}

public enum InventoryType
{
  Player,
  Shop,
  Any
}