using System;
using System.Collections.Generic;
using Godot;

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

  public List<Card> GetRandomCards(int count)
  {
    var cards = new List<Card>();
    for (int i = 0; i < count; i++)
    {
      var r = _rnd.Next(_availableCards.Count);
      var card = _availableCards[r].Clone();
      cards.Add(card);
    }
    return cards;
  }
}
