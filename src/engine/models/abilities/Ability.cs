using System.Collections.Generic;

public interface Ability // TODO make ability and create a calcualation (similar to how level data is handled for description)
{
  string Name { get; set; }
  string Type { get; set; }
  string Phase { get; set; }
  // public string Value { get; set; }
  // public string Duration { get; set; }
  // public Target Target { get; set; }
  List<object> BuiltInFunctions { get; set; }
  List<object> Functions { get; set; }
}