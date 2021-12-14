using System;
using System.Collections.Generic;
using Godot;

public class Inventory
{
  private List<Sprite> _cardSlots = new List<Sprite>();
  private Dictionary<int, Card> _cards = new Dictionary<int, Card>();

  public Inventory() { }

  public bool IsCardInSlot(int slotNum)
  {
    return _cards.ContainsKey(slotNum);
  }

  public Card GetCardInSlot(int slotNum)
  {
    if (_cards.ContainsKey(slotNum))
    {
      return _cards[slotNum];
    }
    return null;
  }

  public List<Card> GetCards()
  {
    var cards = new List<Card>();
    for (int i = 0; i < PrepSceneData.InventorySize; i++)
    {
      var card = GetCardInSlot(i);
      cards.Add(card);
    }
    return cards;
  }

  public bool AddCard(Card card, int slot)
  {
    if (IsCardInSlot(slot))
    {
      GD.Print($"Can't ADD '{card.Name}' to {slot}. There's an card there already.");
      return false;
    }
    card.Slot = slot;
    _cards[card.Slot] = card;
    GD.Print($"ADDED '{card.Name}' to {slot}");
    return true;
  }

  public bool RemoveCard(int slot)
  {
    if (!IsCardInSlot(slot))
    {
      GD.Print($"Can't REMOVE card from {slot}. There's no card there.");
      return false;
    }
    var card = GetCardInSlot(slot);
    card.CardNode.QueueFree(); // Remove card node
    _cards.Remove(slot);
    GD.Print($"REMOVED card from {slot}");
    return true;
  }

  public bool MoveCard(Card card, int slot)
  {
    if (IsCardInSlot(slot))
    {
      GD.Print($"Can't MOVE '{card.Name}' from {card.Slot} to {slot}. There's already a card there.");
      return false;
    }
    var previousSlot = card.Slot;
    card.Slot = slot;
    _cards[slot] = card;
    _cards.Remove(previousSlot);
    GD.Print($"MOVED '{card.Name}' from {previousSlot} to {slot}");
    return true;
  }

  public bool SwapCards(int slot1, int slot2)
  {
    if (!IsCardInSlot(slot1) && !IsCardInSlot(slot2))
    {
      GD.Print($"Can't SWAP cards. No cards in slots {slot1} and {slot2}");
      return false;
    }
    if (!IsCardInSlot(slot1))
    {
      GD.Print($"Can't SWAP cards. No card in slot {slot1}");
      return false;
    }
    if (!IsCardInSlot(slot2))
    {
      GD.Print($"Can't SWAP cards. No card in slot {slot2}");
      return false;
    }

    var card1 = GetCardInSlot(slot1);
    var card2 = GetCardInSlot(slot2);
    card1.Slot = slot2;
    card2.Slot = slot1;

    // _cards.Remove(slot1);
    // _cards.Remove(slot2);
    _cards[slot1] = card2;
    _cards[slot2] = card1;
    GD.Print($"SWAPPED card '{card1.Name}' to {slot2} and '{card2.Name}' in {slot1}");
    return true;
  }

  public void PrintCards()
  {
    foreach (var card in _cards.Values)
    {
      GD.Print($"Slot: '{card.Slot}' Name: '{card.Name}'");
    }
  }
}
