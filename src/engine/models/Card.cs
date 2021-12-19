using System.Collections.Generic;

public class Card
{
  public string Name { get; set; }
  public string Description { get; set; }
  public string BaseMove { get; set; }
  public IEnumerable<Ability> Abilities { get; set; }
  public Dictionary<int, Dictionary<string, string>> Tier { get; set; }
}