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

/// <summary>
/// Ability Phases
/// 1) foreach n/a
/// 2) target: others, foreach: others
/// 3) target: others, foreach: self
/// 4) target: self, foreach: others
/// 5) target: self, foreach: self
/// </summary>
public enum AbilityPhase
{
  Abilities1,
  Abilities2,
  Abilities3,
  Abilities4,
  Abilities5
}

/*
Ability Phases

1) foreach n/a
2) target: others, foreach: others
3) target: others, foreach: self
4) target: self, foreach: others
5) target: self, foreach: self

Examples

1) curse: give other players -3
	target: others, foreach: n/a, sign -

2) ...
	target: others, foreach: others

3) anti wizard: other players -2 for each negative token on you
	target: others, foreach: self, sign -

4) dream siphon: gain 2 for each negative token on opponents
	target: self, foreach: others, sign +

5) focus ring: gain 2x +tokens and 2x -tokens
	target: self, foreach: self, sign + and -
5) stamina elixir: gain 1 for each -token
	target: self, foreach: self, sign +
*/