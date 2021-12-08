using System;
using System.Collections.Generic;

public class CardLoader
{
  private List<Card> _cards { get; set; }

  public CardLoader(string file)
  {
    _cards = LoadJsonCards(file);
  }

  public List<Card> GetCards()
  {
    return _cards;
  }

  private List<Card> LoadJsonCards(string file)
  {
    var cards = new List<Card>();
    // TODO: load cards from json
    cards = LoadSampleCards();
    return cards;
  }

  private List<Card> LoadSampleCards()
  {
    var cards = new List<Card>();

    for (var i = 0; i < 12; i++)
    {
      var card = new Card
      {
        Name = $"card #{i}",
        Body = $"This is the sample card's body"
      };
      cards.Add(card);
    }

    return cards;
  }
}
