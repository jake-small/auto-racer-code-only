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

//   TokenType GetTokenType()
//   {
//     var result = Enum.TryParse(Type.ToLowerInvariant().Capitalize(), out TokenType type);
//     if (!result)
//     {
//       GD.Print($"Error: unable to parse Type {Type} to enum {nameof(TokenType)}");
//       throw new Exception($"Error: unable to parse Type {Type} to enum {nameof(TokenType)}");
//     }
//     return type;
//   } 
// public enum TokenType
// {
//   Move,
//   Shield
// }