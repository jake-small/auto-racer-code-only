using System.Collections.Generic;

public class PrepAbilityResult
{
  public Card Card { get; set; }
  public Effect Effect { get; set; }
  public int Value { get; set; }
  public IEnumerable<Card> Targets { get; set; }

  public PrepAbilityResult()
  {
    Targets = new List<Card>();
  }
  public PrepAbilityResult(Card card, Effect effect, int value = 0, IEnumerable<Card> targets = null)
  {
    Card = card;
    Effect = effect;
    Value = value;
    Targets = targets != null ? targets : new List<Card>();
  }
}