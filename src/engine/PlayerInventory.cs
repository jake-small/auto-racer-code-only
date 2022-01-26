using System;
using System.Collections.Generic;
using Godot;

public class PlayerInventory
{
  private Dictionary<int, CardScript> _cardScripts { get; set; } = new Dictionary<int, CardScript>();

  public bool IsCardInSlot(int slotNum)
  {
    return _cardScripts.ContainsKey(slotNum);
  }

  public CardScript GetCardInSlot(int slotNum)
  {
    if (_cardScripts.ContainsKey(slotNum))
    {
      return _cardScripts[slotNum];
    }
    return null;
  }

  public Dictionary<int, Card> GetCards()
  {
    var cardDict = new Dictionary<int, Card>();
    for (int i = 0; i < GameData.InventorySize; i++)
    {
      var cardScript = GetCardInSlot(i);
      var card = (cardScript == null ? new CardEmpty() : cardScript.Card);
      cardDict[i] = card;
      GD.Print($"card: {card.GetRawName()} {card.BaseMove}");
    }
    return cardDict;
  }

  public List<CardScript> GetCardsAsList()
  {
    var cards = new List<CardScript>();
    for (int i = 0; i < GameData.InventorySize; i++)
    {
      var card = GetCardInSlot(i);
      if (card != null)
      {
        cards.Add(card);
      }
    }
    return cards;
  }

  public bool AddCard(CardScript cardScript, int slot, bool fromShopInventory)
  {
    if (IsCardInSlot(slot))
    {
      GD.Print($"Can't ADD '{cardScript.Card.GetName()}' to {slot}. There's already a card there.");
      return false;
    }
    if (fromShopInventory)
    {
      GameManager.PrepEngine.ShopInventory.RemoveCard(cardScript.Slot);
    }
    cardScript.Slot = slot;
    cardScript.Frozen = false;
    cardScript.Inventory = InventoryTarget.Player;
    _cardScripts[slot] = cardScript;
    GD.Print($"ADDED '{cardScript.Card.GetName()}' to Player inventory slot {slot}");
    return true;
  }

  public bool RemoveCard(int slot)
  {
    if (!IsCardInSlot(slot))
    {
      GD.Print($"Can't REMOVE card from Player inventory slot {slot}. There's no card there.");
      return false;
    }
    var card = GetCardInSlot(slot);
    card.Slot = -1;
    _cardScripts.Remove(slot);
    GD.Print($"REMOVED card from Player inventory slot {slot}");
    return true;
  }

  public bool MoveCard(CardScript cardScript, int toSlot)
  {
    var fromSlot = cardScript.Slot;
    if (IsCardInSlot(toSlot))
    {
      GD.Print($"Can't MOVE '{cardScript.Card.GetName()}' from {fromSlot} to {toSlot}. There's already a card there.");
      return false;
    }
    cardScript.Slot = toSlot;
    _cardScripts[toSlot] = cardScript;
    _cardScripts.Remove(fromSlot);
    GD.Print($"MOVED '{cardScript.Card.GetName()}' from {fromSlot} to {toSlot}");
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
    _cardScripts[slot1] = card2;
    _cardScripts[slot2] = card1;
    GD.Print($"SWAPPED card '{card1.Card.GetName()}' to {slot2} and '{card2.Card.GetName()}' in {slot1}");
    return true;
  }
}
