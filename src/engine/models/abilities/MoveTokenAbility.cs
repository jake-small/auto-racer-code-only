using System.Collections.Generic;

public class MoveTokenAbility
{
  public string Name { get; set; }
  public string Type { get; set; }
  public string Phase { get; set; }
  public string Value { get; set; }
  public string Duration { get; set; }
  public Target Target { get; set; }
  public List<object> BuiltInFunctions { get; set; }
  public List<object> Functions { get; set; }
}