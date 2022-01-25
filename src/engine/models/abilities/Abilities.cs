using System;
using System.Collections.Generic;
using System.Linq;

public class Abilities : ICloneable
{
  public List<MoveTokenAbility> MoveTokenAbilities { get; set; }
  public List<PrepAbility> PrepAbilities { get; set; }

  public object Clone()
  {
    var moveTokenAbilities = MoveTokenAbilities?.Select(m => m.Clone()).Cast<MoveTokenAbility>();
    var prepAbilities = PrepAbilities?.Select(p => p.Clone()).Cast<PrepAbility>();
    return new Abilities
    {
      MoveTokenAbilities = moveTokenAbilities?.ToList(),
      PrepAbilities = prepAbilities?.ToList()
    };
  }
}