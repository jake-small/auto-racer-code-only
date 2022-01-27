using System;
using System.Collections.Generic;
using System.Linq;

public class ShopService
{
  private List<Card> _availableCards { get; set; }
  private static Random _rnd = new Random();


  public ShopService()
  {
    var cardLoader = new CardLoader(PrepSceneData.CardDataRelativePath);
    var cards = cardLoader.GetCards();

    _availableCards = new List<Card>();
    foreach (var card in cards)
    {
      _availableCards.Add(card);
    }
  }

  public List<Card> GetRandomCards(int amount, int maxTier)
  {
    var availableCardsForTier = _availableCards.Where(c => c.Tier <= maxTier).ToList();
    var cards = new List<Card>();
    for (int i = 0; i < amount; i++)
    {
      var r = _rnd.Next(availableCardsForTier.Count);
      var card = (Card)availableCardsForTier[r].Clone();
      cards.Add(card);
    }
    return cards;
  }
}
