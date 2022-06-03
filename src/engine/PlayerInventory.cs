using System;
using System.Collections.Generic;
using System.Linq;

public class PlayerInventory
{
  private bool _shouldLog;
  private Dictionary<int, Card> _cardDict { get; set; } = new Dictionary<int, Card>();

  public PlayerInventory(bool shouldLog = true)
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
    var cardDict = new Dictionary<int, Card>();
    for (int i = 0; i < GameData.PlayerInventorySize; i++)
    {
      var card = GetCardInSlot(i);
      if (card is null)
      {
        card = new CardEmpty();
      }
      cardDict[i] = card;
    }
    return cardDict;
  }

  public List<Card> GetCardsAsList()
  {
    var cards = new List<Card>();
    for (int i = 0; i < GameData.PlayerInventorySize; i++)
    {
      var card = GetCardInSlot(i);
      if (card != null)
      {
        cards.Add(card);
      }
    }
    return cards;
  }

  public bool AddCard(Card card, int slot)
  {
    if (IsCardInSlot(slot))
    {
      EngineTesting.Log($"Can't ADD '{card.GetName()}' to {slot}. There's already a card there.", _shouldLog);
      return false;
    }
    card.InventoryType = InventoryType.Player;
    _cardDict[slot] = card;
    EngineTesting.Log($"ADDED '{card.GetName()}' to Player inventory slot {slot}", _shouldLog);
    return true;
  }

  public bool RemoveCard(int slot)
  {
    if (!IsCardInSlot(slot))
    {
      EngineTesting.Log($"Can't REMOVE card from Player inventory slot {slot}. There's no card there.", _shouldLog);
      return false;
    }
    var card = GetCardInSlot(slot);
    _cardDict.Remove(slot);
    EngineTesting.Log($"REMOVED card from Player inventory slot {slot}", _shouldLog);
    return true;
  }

  public bool MoveCard(Card card, int fromSlot, int toSlot)
  {
    if (IsCardInSlot(toSlot))
    {
      EngineTesting.Log($"Can't MOVE '{card.GetName()}' from {fromSlot} to {toSlot}. There's already a card there.", _shouldLog);
      return false;
    }
    _cardDict[toSlot] = card;
    _cardDict.Remove(fromSlot);
    EngineTesting.Log($"MOVED '{card.GetName()}' from {fromSlot} to {toSlot}", _shouldLog);
    return true;
  }

  public bool SwapCards(int slot1, int slot2)
  {
    if (!IsCardInSlot(slot1) && !IsCardInSlot(slot2))
    {
      EngineTesting.Log($"Can't SWAP cards. No cards in slots {slot1} and {slot2}", _shouldLog);
      return false;
    }
    if (!IsCardInSlot(slot1))
    {
      EngineTesting.Log($"Can't SWAP cards. No card in slot {slot1}", _shouldLog);
      return false;
    }
    if (!IsCardInSlot(slot2))
    {
      EngineTesting.Log($"Can't SWAP cards. No card in slot {slot2}", _shouldLog);
      return false;
    }

    var card1 = GetCardInSlot(slot1);
    var card2 = GetCardInSlot(slot2);
    _cardDict[slot1] = card2;
    _cardDict[slot2] = card1;
    EngineTesting.Log($"SWAPPED card '{card1.GetName()}' to {slot2} and '{card2.GetName()}' in {slot1}", _shouldLog);
    return true;
  }
}
