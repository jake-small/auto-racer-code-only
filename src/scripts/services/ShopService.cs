using System;
using System.Collections.Generic;

public class ShopService
{
  private List<Card> _availableCards { get; set; }
  private static Random _rnd = new Random();


  public ShopService()
  {
    var cardLoader = new CardLoader("todo/json/file/path");
    _availableCards = cardLoader.GetCards();
  }

  public List<Card> GetRandomCards(int count)
  {
    var cards = new List<Card>();
    for (int i = 0; i < count; i++)
    {
      var r = _rnd.Next(_availableCards.Count);
      Console.WriteLine($"random card #{r}");
      var card = _availableCards[r].Clone();
      card.Body = Guid.NewGuid().ToString();
      cards.Add(card);
    }

    return cards;
  }
}
