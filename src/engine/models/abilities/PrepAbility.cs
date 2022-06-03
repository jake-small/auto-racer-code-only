using System;
using System.Collections.Generic;
using System.Linq;

public class PrepAbility : Ability, ICloneable
{
  public string Name { get; set; }
  /// <summary>
  ///  Triggers include: start turn, end turn, sell, sold, buy, bought, reroll, and freeze
  /// </summary>
  public string Trigger { private get; set; }
  public List<Function> Functions { get; set; }
  /// <summary>
  /// Effects include: BaseMove, Gold, and Exp
  /// </summary>
  public string Effect { private get; set; }
  public string Value { get; set; }
  public PrepTarget Target { get; set; }

  public Trigger GetTrigger()
  {
    var result = Enum.TryParse(Trigger, true, out Trigger type);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Trigger {Trigger} to enum Trigger");
      throw new Exception($"Error: unable to parse Trigger {Trigger} to enum Trigger");
    }
    return type;
  }

  public Effect GetEffect()
  {
    var result = Enum.TryParse(Effect, true, out Effect type);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Effect {Effect} to enum Effect");
      throw new Exception($"Error: unable to parse Effect {Effect} to enum Effect");
    }
    return type;
  }

  public object Clone()
  {
    return new PrepAbility
    {
      Name = Name,
      Trigger = Trigger,
      Functions = Functions?.Select(p => p.Clone()).Cast<Function>().ToList(),
      Effect = Effect,
      Value = Value,
      Target = Target
    };
  }
}

public enum Trigger
{
  Startturn,
  Endturn,
  Sell,
  Sold,
  Buy,
  Bought,
  Reroll,
  Freeze
}
public enum Effect
{
  Basemove,
  Gold,
  Exp,
  Reroll
}