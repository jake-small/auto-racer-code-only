using System;
using System.Collections.Generic;
using Godot;

public class ShopService
{
  private List<CardViewModel> _availableCards { get; set; }
  private static Random _rnd = new Random();


  public ShopService()
  {
    var cardLoader = new CardLoader(PrepSceneData.CardDataRelativePath);
    var cards = cardLoader.GetCards();

    _availableCards = new List<CardViewModel>();
    foreach (var card in cards)
    {
      _availableCards.Add(new CardViewModel
      {
        Card = card
      });
    }
  }

  public List<CardViewModel> GetRandomCards(int count)
  {
    var cards = new List<CardViewModel>();
    for (int i = 0; i < count; i++)
    {
      var r = _rnd.Next(_availableCards.Count);
      var card = _availableCards[r].Clone();
      cards.Add(card);
    }

    return cards;
  }
}
