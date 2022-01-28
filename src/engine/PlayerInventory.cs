using System;
using System.Collections.Generic;

public class PlayerInventory
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
      Console.WriteLine($"Can't ADD '{card.GetName()}' to {slot}. There's already a card there.");
      return false;
    }
    card.InventoryType = InventoryType.Player;
    _cardDict[slot] = card;
    Console.WriteLine($"ADDED '{card.GetName()}' to Player inventory slot {slot}");
    return true;
  }

  public bool RemoveCard(int slot)
  {
    if (!IsCardInSlot(slot))
    {
      Console.WriteLine($"Can't REMOVE card from Player inventory slot {slot}. There's no card there.");
      return false;
    }
    var card = GetCardInSlot(slot);
    // card.Slot = -1;
    _cardDict.Remove(slot);
    Console.WriteLine($"REMOVED card from Player inventory slot {slot}");
    return true;
  }

  public bool MoveCard(Card card, int fromSlot, int toSlot)
  {
    if (IsCardInSlot(toSlot))
    {
      Console.WriteLine($"Can't MOVE '{card.GetName()}' from {fromSlot} to {toSlot}. There's already a card there.");
      return false;
    }
    // card.Slot = toSlot;
    _cardDict[toSlot] = card;
    _cardDict.Remove(fromSlot);
    Console.WriteLine($"MOVED '{card.GetName()}' from {fromSlot} to {toSlot}");
    return true;
  }

  public bool SwapCards(int slot1, int slot2)
  {
    if (!IsCardInSlot(slot1) && !IsCardInSlot(slot2))
    {
      Console.WriteLine($"Can't SWAP cards. No cards in slots {slot1} and {slot2}");
      return false;
    }
    if (!IsCardInSlot(slot1))
    {
      Console.WriteLine($"Can't SWAP cards. No card in slot {slot1}");
      return false;
    }
    if (!IsCardInSlot(slot2))
    {
      Console.WriteLine($"Can't SWAP cards. No card in slot {slot2}");
      return false;
    }

    var card1 = GetCardInSlot(slot1);
    var card2 = GetCardInSlot(slot2);
    // card1.Slot = slot2;
    // card2.Slot = slot1;
    _cardDict[slot1] = card2;
    _cardDict[slot2] = card1;
    Console.WriteLine($"SWAPPED card '{card1.GetName()}' to {slot2} and '{card2.GetName()}' in {slot1}");
    return true;
  }
}
