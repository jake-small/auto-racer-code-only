using System.Collections.Generic;

public interface Ability
{
  string Name { get; set; }
  // string Type { get; set; }
  string Phase { get; set; }
  List<object> BuiltInFunctions { get; set; }
  Functions Functions { get; set; }
}