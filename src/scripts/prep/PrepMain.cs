using Godot;
using System;
using System.Collections.Generic;

public class PrepMain : Node2D
{
  private readonly Vector2 ShopSlotOffset = new Vector2(6, 4);

  private List<string> _shopCards = new List<string>();
  private List<Sprite> _cardSlots = new List<Sprite>();
  private DroppedCard _droppedCard = null;

  public static Inventory _inventory = new Inventory();

  public override void _Ready()
  {
    FillCardShop();
    var cardSlotNodes = GetTree().GetNodesInGroup(PrepSceneData.GroupCardSlots);
    foreach (Sprite sprite in cardSlotNodes)
    {
      _cardSlots.Add(sprite);
    }
  }

  public override void _PhysicsProcess(float delta)
  {
    if (_droppedCard != null)
    {
      _droppedCard.CardNode.Position = _droppedCard.DroppedPosition;
      _droppedCard = null;
    }
  }

  public void _on_Card_droppedInSlot(Card card, KinematicBody2D cardNode, int slot, Vector2 droppedPosition, Vector2 originalPosition)
  {
    GD.Print($"Drop signal received for {card.Name} at slot {card.Slot} to {slot} at position {droppedPosition}");
    if (_inventory.IsCardInSlot(slot))
    {
      GD.Print($"Card already exists in slot {slot}");
      _droppedCard = new DroppedCard
      {
        Card = card,
        CardNode = cardNode,
        DroppedPosition = originalPosition,
        OriginalPosition = originalPosition
      };
      return;
    }
    else
    {
      if (card.Slot != -1)
      {
        var result = _inventory.MoveCard(card, slot);
        if (result)
        {
          GD.Print($"Moved card from slot {card.Slot} to {slot}");
          card.Slot = slot;
        }
        else
        {
          GD.Print($"Failed to move card from slot {card.Slot} to {slot}");
          _droppedCard = new DroppedCard
          {
            Card = card,
            CardNode = cardNode,
            DroppedPosition = originalPosition,
            OriginalPosition = originalPosition
          };
          return;
        }
      }
      else
      {
        var result = _inventory.AddCard(card, slot);
        if (result)
        {
          GD.Print($"Added card to slot {slot} from shop");
          card.Slot = slot;
        }
        else
        {
          GD.Print($"Failed to add card to slot {slot}");
          _droppedCard = new DroppedCard
          {
            Card = card,
            CardNode = cardNode,
            DroppedPosition = originalPosition,
            OriginalPosition = originalPosition
          };
          return;
        }
      }
      _droppedCard = new DroppedCard
      {
        Card = card,
        CardNode = cardNode,
        DroppedPosition = droppedPosition,
        OriginalPosition = originalPosition
      };
      // _dropped = true;
      //_bought = true;
      // _startingPosition = _droppedPosition;
      cardNode.Set("_dropped", true);
      cardNode.Set("_bought", true);
      cardNode.Set("_startingPosition", droppedPosition);
      cardNode.Set("_card", card);
    }
  }

  private void FillCardShop()
  {
    var shopService = new ShopService();
    var cards = shopService.GetRandomCards(PrepSceneData.ShopSize);

    var cardScene = ResourceLoader.Load(PrepSceneData.CardScenePath) as PackedScene;
    for (int i = 0; i < PrepSceneData.ShopSize; i++)
    {
      var card = cards[i];
      var cardInstance = (KinematicBody2D)cardScene.Instance();
      var containerPosition = GetNode<Sprite>($"shop_slot_{i}").Position;
      var position = containerPosition + ShopSlotOffset;
      cardInstance.Position = position;
      cardInstance.Set("_card", card);
      cardInstance.Connect(nameof(CardScript.droppedInSlot), this, nameof(_on_Card_droppedInSlot));
      AddChild(cardInstance);
    }
  }
}
