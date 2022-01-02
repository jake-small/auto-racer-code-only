using System.Collections.Generic;

public interface Ability
{
  string Name { get; set; }
  // string Type { get; set; }
  string Phase { get; set; }
  List<Function> Functions { get; set; }
}