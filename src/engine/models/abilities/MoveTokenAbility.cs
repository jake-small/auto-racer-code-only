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
      Target = Target,
      Functions = Functions.Select(m => m.Clone()).Cast<Function>().ToList()
    };
  }
}