using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

public class CardLoader : FileLoader
{
  private const string DefaultCardDataPath = @"configs/cardData.tres";
  private List<Card> _cards { get; set; }

  public CardLoader(string cardDataPath)
  {
    try
    {
      if (System.IO.File.Exists(cardDataPath))
      {
        GD.Print($"Loading card data '{cardDataPath}'");
        _cards = LoadJsonData<CardData>(cardDataPath).Cards;
      }
    }
    catch (System.Exception)
    {
      GD.Print($"Warning: Unable to access filesystem to access card config '{cardDataPath}', using built-in card data instead");
    }
    finally
    {
      if (_cards == null || _cards.Count == 0)
      {
        _cards = LoadResourceData<CardData>(DefaultCardDataPath).Cards;
      }
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
