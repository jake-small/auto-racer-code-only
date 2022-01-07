using System;
using System.Collections.Generic;
using Godot;

public interface TokenAbility : Ability
{
  string Phase { get; set; }
  string Duration { get; set; }
  Target Target { get; set; }

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
}

// public enum TokenType
// {
//   Move,
//   Shield
// }