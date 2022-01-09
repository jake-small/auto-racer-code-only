using System;
using System.Collections.Generic;
using Godot;

public class MoveTokenAbility : TokenAbility
{
  public string Name { get; set; }
  public string Phase { get; set; }
  public List<Function> Functions { get; set; }
  public string Duration { get; set; }
  public Target Target { get; set; }
  public string Value { get; set; }
  public string Type { get; set; }
}