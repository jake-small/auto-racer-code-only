using System;
using System.Collections.Generic;

public class ShopInventory
{
  private Dictionary<int, Card> _cardDict { get; set; } = new Dictionary<int, Card>();

  public bool IsCardInSlot(int slotNum)
  {
    return _cardDict.ContainsKey(slotNum);
  }

  public Card GetCardInSlot(int slotNum)
  {
    if (_cardDict.ContainsKey(slotNum))
    {
      return _cardDict[slotNum];
    }
    return null;
  }

  public Dictionary<int, Card> GetCards()
  {
    return _cardDict;
  }

  public List<Card> GetCardsAsList()
  {
    var cards = new List<Card>();
    for (int i = 0; i < GameData.ShopInventorySize; i++)
    {
      var card = GetCardInSlot(i);
      if (card != null)
      {
        cards.Add(card);
      }
    }
    return cards;
  }

  public void Clear()
  {
    _cardDict.Clear();
  }

  public bool AddCard(Card card, int slot)
  {
    if (IsCardInSlot(slot))
    {
      Console.WriteLine($"Can't ADD '{card.GetName()}' to Shop inventory slot {slot}. There's already a card there.");
      return false;
    }
    _cardDict[slot] = card;
    Console.WriteLine($"ADDED '{card.GetName()}' to Shop inventory slot {slot}");
    return true;
  }

  public bool RemoveCard(int slot)
  {
    if (!IsCardInSlot(slot))
    {
      Console.WriteLine($"Can't REMOVE card from Shop inventory slot {slot}. There's no card there.");
      return false;
    }
    var card = GetCardInSlot(slot);
    _cardDict.Remove(slot);
    Console.WriteLine($"REMOVED card from Shop inventory slot {slot}");
    return true;
  }
}
