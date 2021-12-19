using System;
using System.Collections.Generic;

public class ShopService
{
  private List<CardViewModel> _availableCards { get; set; }
  private static Random _rnd = new Random();


  public ShopService()
  {
    var cardLoader = new CardLoader("todo/json/file/path");
    _availableCards = cardLoader.GetCards();
  }

  public List<CardViewModel> GetRandomCards(int count)
  {
    var cards = new List<CardViewModel>();
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
