using System.Collections.Generic;
using Godot;

public class ShopInventory
{
  public Dictionary<int, CardScript> CardScriptDict { get; private set; } = new Dictionary<int, CardScript>();

  public bool IsCardInSlot(int slotNum)
  {
    return CardScriptDict.ContainsKey(slotNum);
  }

  public CardScript GetCardInSlot(int slotNum)
  {
    if (CardScriptDict.ContainsKey(slotNum))
    {
      return CardScriptDict[slotNum];
    }
    return null;
  }

  public List<CardScript> GetCardsAsList()
  {
    var cardScripts = new List<CardScript>();
    for (int i = 0; i < GameData.InventorySize; i++)
    {
      var card = GetCardInSlot(i);
      if (card != null)
      {
        cardScripts.Add(card);
      }
    }
    return cardScripts;
  }

  public void Clear()
  {
    CardScriptDict.Clear();
  }

  public bool AddCard(CardScript cardScript, int slot)
  {
    if (IsCardInSlot(slot))
    {
      GD.Print($"Can't ADD '{cardScript.Card.GetName()}' to Shop inventory slot {slot}. There's already a card there.");
      return false;
    }
    cardScript.Slot = slot;
    CardScriptDict[slot] = cardScript;
    GD.Print($"ADDED '{cardScript.Card.GetName()}' to Shop inventory slot {slot}");
    return true;
  }

  public bool RemoveCard(int slot)
  {
    if (!IsCardInSlot(slot))
    {
      GD.Print($"Can't REMOVE card from Shop inventory slot {slot}. There's no card there.");
      return false;
    }
    var card = GetCardInSlot(slot);
    card.Slot = -1;
    CardScriptDict.Remove(slot);
    GD.Print($"REMOVED card from Shop inventory slot {slot}");
    return true;
  }
}
