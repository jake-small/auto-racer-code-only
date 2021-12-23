using System;
using Godot;

public class Target
{
  public string Type { private get; set; }
  public string Amount { get; set; }
  public Range Range { get; set; }

  public TargetType GetTargetType()
  {
    var result = Enum.TryParse(Type, out TargetType type);
    if (!result)
    {
      GD.Print($"Error: unable to parse Type {Type} to enum TargetType");
      throw new Exception($"Error: unable to parse Type {Type} to enum TargetType");
    }
    return type;
  }
}

public enum TargetType
{
  Others,
  Self,
  All,
  Closest,
  Furthest
}