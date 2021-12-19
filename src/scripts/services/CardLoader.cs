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
    var cards = new List<CardViewModel>();

    for (var i = 0; i < 12; i++)
    {
      var card = new CardViewModel
      {
        Name = $"card #{i}",
        Body = $"This is the sample card's body"
      };
      cards.Add(card);
    }

    return cards;
  }
}
