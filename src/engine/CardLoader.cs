using System;
using System.Collections.Generic;
using System.Text.Json;

public class CardLoader
{
  private const string DefaultCardDataPath = @"configs/cardData.tres";
  private List<Card> _cards { get; set; }

  public CardLoader(string cardDataPath, DataLoader dataLoader)
  {
    try
    {
      if (System.IO.File.Exists(cardDataPath))
      {
        Console.WriteLine($"Loading card data '{cardDataPath}'");
        _cards = dataLoader.LoadJsonData<CardData>(cardDataPath).Cards;
      }
    }
    catch (System.Exception e)
    {
      Console.WriteLine($"Warning: Unable to access filesystem to access card config '{cardDataPath}', using built-in card data instead. Error: {e.Message}");
    }
    finally
    {
      if (_cards == null || _cards.Count == 0)
      {
        Console.WriteLine($"Warning: Card json file not found at '{cardDataPath}', using built-in card data instead");
        if (dataLoader.CanLoadResource())
        {
          _cards = dataLoader.LoadResourceData<CardData>(DefaultCardDataPath).Cards;
        }
        else
        {
          Console.WriteLine($"Warning: Unable to load card resource file");
        }
      }
      // TODO: add card data validation
    }
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
