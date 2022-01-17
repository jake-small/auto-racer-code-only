using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

public class CardLoader
{
  private List<Card> _cards { get; set; }

  public CardLoader(string cardDataPath)
  {
    _cards = LoadJsonCards(cardDataPath);
  }

  public List<Card> GetCards()
  {
    // var cards = _cards;
    // add some extra random cards
    // cards.AddRange(LoadSampleCards(10));
    return _cards;
  }

  private List<Card> LoadJsonCards(string cardDataFile)
  {
    if (!System.IO.File.Exists(cardDataFile))
    {
      GD.Print($"Error: provided cardDataFile '{cardDataFile}' does not exist");
      throw new Exception($"Error: provided cardDataFile '{cardDataFile}' does not exist");
    }
    var cardDataArr = System.IO.File.ReadAllLines(cardDataFile);
    var cardDataJson = String.Join("\n", cardDataArr);
    GD.Print($"Card Data:\n{cardDataJson}");
    // TODO: error handling
    var cardData = JsonSerializer.Deserialize<CardData>(cardDataJson);
    return cardData.Cards;
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
