using System.Collections.Generic;

public class Card
{
  public string Name { get; set; }
  public string Description { get; set; }
  public string BaseMove { get; set; }
  public Abilities Abilities { get; set; }
  public List<Tier> Tiers { get; set; }
}