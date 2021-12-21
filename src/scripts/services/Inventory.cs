using System;
using System.Collections.Generic;
using Godot;

public static class Inventory
{
  private static Dictionary<int, CardViewModel> _cardVMs = new Dictionary<int, CardViewModel>();

  public static bool IsCardInSlot(int slotNum)
  {
    return _cardVMs.ContainsKey(slotNum);
  }

  public static CardViewModel GetCardInSlot(int slotNum)
  {
    if (_cardVMs.ContainsKey(slotNum))
    {
      return _cardVMs[slotNum];
    }
    return null;
  }

  public static Dictionary<int, CardViewModel> GetCardVMs() => _cardVMs;

  public static List<CardViewModel> GetCardVMsFlattened()
  {
    var cardVMs = new List<CardViewModel>();
    for (int i = 0; i < GameData.InventorySize; i++)
    {
      var cardVM = GetCardInSlot(i);
      if (cardVM != null)
      {
        cardVMs.Add(cardVM);
      }
    }
    return cardVMs;
  }

  public static Dictionary<int, Card> GetCards()
  {
    var cardDict = new Dictionary<int, Card>();
    for (int i = 0; i < GameData.InventorySize; i++)
    {
      var card = GetCardInSlot(i).Card;
      // if (card != null)
      // {
      cardDict[i] = card;
      // }
    }
    return cardDict;
  }

  public static bool AddCard(CardViewModel cardVM, int slot)
  {
    if (IsCardInSlot(slot))
    {
      GD.Print($"Can't ADD '{cardVM.Card.Name}' to {slot}. There's already a card there.");
      return false;
    }
    cardVM.CardNode.Frozen = false;
    cardVM.Slot = slot;
    _cardVMs[cardVM.Slot] = cardVM;
    GD.Print($"ADDED '{cardVM.Card.Name}' to {slot}");
    return true;
  }

  public static bool RemoveCard(int slot)
  {
    if (!IsCardInSlot(slot))
    {
      GD.Print($"Can't REMOVE card from {slot}. There's no card there.");
      return false;
    }
    var card = GetCardInSlot(slot);
    card.CardNode.QueueFree(); // Remove card node
    _cardVMs.Remove(slot);
    GD.Print($"REMOVED card from {slot}");
    return true;
  }

  public static bool MoveCard(CardViewModel cardVM, int slot)
  {
    if (IsCardInSlot(slot))
    {
      GD.Print($"Can't MOVE '{cardVM.Card.Name}' from {cardVM.Slot} to {slot}. There's already a card there.");
      return false;
    }
    var previousSlot = cardVM.Slot;
    cardVM.Slot = slot;
    _cardVMs[slot] = cardVM;
    _cardVMs.Remove(previousSlot);
    GD.Print($"MOVED '{cardVM.Card.Name}' from {previousSlot} to {slot}");
    return true;
  }

  public static bool SwapCards(int slot1, int slot2)
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

    var cardVM1 = GetCardInSlot(slot1);
    var cardVM2 = GetCardInSlot(slot2);
    cardVM1.Slot = slot2;
    cardVM2.Slot = slot1;
    _cardVMs[slot1] = cardVM2;
    _cardVMs[slot2] = cardVM1;
    GD.Print($"SWAPPED card '{cardVM1.Card.Name}' to {slot2} and '{cardVM2.Card.Name}' in {slot1}");
    return true;
  }

  public static void PrintCards()
  {
    foreach (var cardVM in _cardVMs.Values)
    {
      GD.Print($"Slot: '{cardVM.Slot}' Name: '{cardVM.Card.Name}'");
    }
  }
}
