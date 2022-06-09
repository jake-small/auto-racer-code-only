using System;
using System.Collections.Generic;
using System.Linq;

public class MoveTokenAbility : TokenAbility, ICloneable
{
  public string Name { get; set; }
  public string Phase { get; set; }
  public string Duration { get; set; }
  public string Value { get; set; }
  public string Type { get; set; }
  public Target Target { get; set; }
  public List<Function> Functions { get; set; }

  public object Clone()
  {
    return new MoveTokenAbility
    {
      Name = Name,
      Phase = Phase,
      Duration = Duration,
      Value = Value,
      Type = Type,
      Target = Target.Clone() as Target,
      Functions = Functions.Select(m => m.Clone()).Cast<Function>().ToList()
    };
  }

  public AbilityPhase GetAbilityPhase()
  {
    var result = Enum.TryParse(Phase, true, out AbilityPhase phase);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Phase '{Phase}' to enum AbilityPhase");
      throw new Exception($"Error: unable to parse Phase '{Phase}' to enum AbilityPhase");
    }
    return phase;
  }
}

public enum AbilityPhase
{
  Abilities1,
  Abilities2
}