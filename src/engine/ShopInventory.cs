using System;
using System.Collections.Generic;
using System.Linq;

public class ShopInventory
{
  private bool _shouldLog;
  private Dictionary<int, Card> _cardDict { get; set; } = new Dictionary<int, Card>();

  public ShopInventory(bool shouldLog = true)
  {
    _shouldLog = shouldLog;
  }

  public bool IsCardInSlot(int slotNum)
  {
    return _cardDict.ContainsKey(slotNum);
  }

  public int GetSlotOfCard(Card card)
  {
    var slotCardPair = _cardDict.FirstOrDefault(x => x.Value == card);
    if (slotCardPair.Value != null)
    {
      return slotCardPair.Key;
    }
    return -1;
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
    for (int i = 0; i < GameManager.ShopSize; i++)
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
      EngineTesting.Log($"Can't ADD '{card.GetName()}' to Shop inventory slot {slot}. There's already a card there.", _shouldLog);
      return false;
    }
    _cardDict[slot] = card;
    EngineTesting.Log($"ADDED '{card.GetName()}' to Shop inventory slot {slot}", _shouldLog);
    return true;
  }

  public bool RemoveCard(int slot)
  {
    if (!IsCardInSlot(slot))
    {
      EngineTesting.Log($"Can't REMOVE card from Shop inventory slot {slot}. There's no card there.", _shouldLog);
      return false;
    }
    var card = GetCardInSlot(slot);
    _cardDict.Remove(slot);
    EngineTesting.Log($"REMOVED card from Shop inventory slot {slot}", _shouldLog);
    return true;
  }
}
