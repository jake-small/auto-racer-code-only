using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

public class CardLoader : FileLoader
{
  private List<Card> _cards { get; set; }

  public CardLoader(string cardDataPath)
  {
    _cards = LoadJsonData<CardData>(cardDataPath).Cards;
  }

  public List<Card> GetCards()
  {
    return _cards;
  }

  private List<Card> LoadSampleCards(int amount)
  {
    var cards = new List<Card>();
    for (var i = 0; i < amount; i++)
    {
      var card = EngineTesting.GetSampleCard();
      card.Name = $"card #{i}";
      card.Description = $"base move: {card.BaseMove}";
      cards.Add(card);
    }
    return cards;
  }
}
