using System;
using System.Collections.Generic;
using Godot;

public class PrepAbility : Ability
{
  public string Name { get; set; }
  /// <summary>
  ///  Phases include: start turn, end turn, sell, sold, buy, bought, reroll, and freeze
  /// </summary>
  public string Phase { get; set; }
  public List<Function> Functions { get; set; }
  /// <summary>
  /// Types include: BaseMove, Gold, and Exp
  /// </summary>
  public string Type { get; set; }
  public string Value { get; set; }
  /// <summary>
  ///  Targets include: self, an inventory position as an int, and shopCards
  /// </summary>
  public string Target { get; set; }
}