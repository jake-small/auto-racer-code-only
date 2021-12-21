using System;
using System.Collections.Generic;

public class CardLoader
{
  private List<CardViewModel> _cards { get; set; }

  public CardLoader(string file)
  {
    _cards = LoadJsonCards(file);
  }

  public List<CardViewModel> GetCards()
  {
    return _cards;
  }

  private List<CardViewModel> LoadJsonCards(string file)
  {
    var cards = new List<CardViewModel>();
    // TODO: load cards from json
    cards = LoadSampleCards();
    return cards;
  }

  private List<CardViewModel> LoadSampleCards()
  {
    var cardVMs = new List<CardViewModel>();
    for (var i = 0; i < 12; i++)
    {
      var card = EngineTesting.GetSampleCard();
      card.Name = $"card #{i}";
      card.Description = $"base move: {card.BaseMove}";
      var cardVM = new CardViewModel
      {
        Card = card
      };
      cardVMs.Add(cardVM);
    }
    return cardVMs;
  }
}
