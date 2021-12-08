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

  public bool AddCard(Card card, int slot)
  {
    if (IsCardInSlot(slot))
    {
      GD.Print($"Can't add card to {slot}. There's an card there already.");
      return false;
    }
    card.Slot = slot;
    _cards[card.Slot] = card;
    return true;
  }

  public bool MoveCard(Card card, int slot)
  {
    if (IsCardInSlot(slot))
    {
      GD.Print($"Can't move card from {card.Slot} to {slot}. There's an card there already.");
      return false;
    }
    var previousSlot = card.Slot;
    card.Slot = slot;
    _cards[slot] = card;
    _cards.Remove(previousSlot);
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
