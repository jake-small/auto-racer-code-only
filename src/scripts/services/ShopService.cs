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

  public ShopService(List<Card> cards)
  {
    _availableCards = cards;
  }

  public List<Card> GetAvailableCards()
  {
    return _availableCards;
  }

  public List<Card> GetRandomCards(int amount)
  {
    var maxTier = GetTier();
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

  private int GetTier()
  {
    /*
    option 1:
      race: 1 2 3 4 5 6 7 8 9 10 11 12
      tier: 1 1 1 2 2 2 3 3 3 4  4  4
    option 2:
      race: 1 2 3 4 5 6 7 8 9 10 11 12
      tier: 1 1 2 2 3 3 4 4 5 5  6  6
   */
    var raceNumber = GameManager.RaceNumber;

    // if (raceNumber > 9)
    // {
    //   return 4;
    // }
    // else if (raceNumber > 6)
    // {
    //   return 3;
    // }
    // else if (raceNumber > 3)
    // {
    //   return 2;
    // }
    // else if (raceNumber >= 0)
    // {
    //   return 1;
    // }

    if (raceNumber > 10)
    {
      return 6;
    }
    else if (raceNumber > 8)
    {
      return 5;
    }
    else if (raceNumber > 6)
    {
      return 4;
    }
    else if (raceNumber > 4)
    {
      return 3;
    }
    else if (raceNumber > 2)
    {
      return 2;
    }
    else if (raceNumber >= 0)
    {
      return 1;
    }
    return -1;
  }
}
