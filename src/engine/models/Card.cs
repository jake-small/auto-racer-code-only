using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Card
{
  public string Name { get; set; }
  public string Description { private get; set; }
  public string BaseMove { get; set; }
  public int Tier { get; set; }
  public Abilities Abilities { get; set; }
  public List<Level> Levels { get; set; }
  public int Level { get; set; } = 1;
  public int Exp { get; set; } = 0;

  public string GetDescription(int levelId)
  {
    return ApplyTierValues(Description, levelId);
  }

  private string ApplyTierValues(string text, int levelId)
  {
    if (Levels == null || !Levels.Any())
    {
      GD.Print($"No Tiers exist for card {Name}");
      return text;
    }
    var level = Levels?.FirstOrDefault(t => t.Id == levelId);
    if (level == null)
    {
      GD.Print($"Error: level does not exist: {levelId}");
      throw new Exception($"Error: level does not exist: {levelId}");
    }
    foreach (var param in level.Params)
    {
      var key = "{" + param.Key + "}";
      text = text.Replace(key, param.Value);
    }
    return text;
  }
}