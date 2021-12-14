using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PrepMain : Node2D
{
  private readonly Vector2 ShopSlotOffset = new Vector2(6, 4);

  private List<string> _shopCards = new List<string>();
  private List<Sprite> _cardSlots = new List<Sprite>();

  public static Inventory _inventory = new Inventory();

  private Button _freezeButton;
  private Label _debugInventoryLabel;
  private List<Card> _cachedDebugCards = new List<Card>();
  private Timer _dropCardTimer;
  private const float _dropCardTimerLength = 0.1f;
  private bool _canDropCard = true;

  public override void _Ready()
  {
    CardShopFill();
    var cardSlotNodes = GetTree().GetNodesInGroup(PrepSceneData.GroupCardSlots);
    foreach (Sprite sprite in cardSlotNodes)
    {
      _cardSlots.Add(sprite);
    }
    _debugInventoryLabel = GetNode<Label>(PrepSceneData.DebugInventoryLabel);

    _dropCardTimer = new Timer();
    _dropCardTimer.WaitTime = _dropCardTimerLength;
    _dropCardTimer.Connect("timeout", this, nameof(_on_dropCardTimer_timeout));
    AddChild(_dropCardTimer);

    var rerollButton = GetNode(PrepSceneData.ButtonRerollPath) as Button;
    rerollButton.Connect("pressed", this, nameof(Button_reroll_pressed));
    _freezeButton = GetNode(PrepSceneData.ButtonFreezePath) as Button;
    _freezeButton.Connect("pressed", this, nameof(Button_freeze_pressed));
    var goButton = GetNode(PrepSceneData.ButtonGoPath) as Button;
    goButton.Connect("pressed", this, nameof(Button_go_pressed));
  }

  public override void _Process(float delta)
  {
    var cards = _inventory.GetCards();
    if (cards.SequenceEqual(_cachedDebugCards))
    {
      return;
    }
    _cachedDebugCards = cards;
    var cardsText = "";
    foreach (var card in cards)
    {
      if (card == null)
      {
        cardsText += $"null\n";
        continue;
      }
      cardsText += $"{card.Name} in slot {card.Slot}. Body: {card.Body}\n";
    }
    _debugInventoryLabel.Text = cardsText;
  }

  public void _on_Card_selected(Card card)
  {
    _freezeButton.Disabled = false;
  }
  public void _on_Card_deselected(Card card)
  {
    _freezeButton.Disabled = true;
  }

  public void _on_Card_droppedInSlot(Card card, int slot, Vector2 droppedPosition, Vector2 originalPosition)
  {
    if (_canDropCard && _dropCardTimer.IsStopped())
    {
      GD.Print($"drop card timer started!!!");
      _dropCardTimer.Start();
      _canDropCard = false;
    }
    else if (!_canDropCard)
    {
      GD.Print($"Can't drop card. Too quick... Despite drop signal RECEIVED for {card.Name} at slot {card.Slot} to {slot} at position {droppedPosition}");
      DropCard(card, originalPosition);
      DeselectAllCards();
      return;
    }

    GD.Print($"Drop signal RECEIVED for {card.Name} at slot {card.Slot} to {slot} at position {droppedPosition}");
    if (_inventory.IsCardInSlot(slot) && card.Slot != -1) // Card in inventory but card exists in targetted slot
    {
      DeselectAllCards();
      var targetCard = _inventory.GetCardInSlot(slot);
      if (card.Name == targetCard.Name) // Combine cards of same type
      {
        targetCard.AddLevels(card.Level);
        _inventory.RemoveCard(card.Slot); // Remove dropped card
        return;
      }

      var result = _inventory.SwapCards(slot, card.Slot);
      if (result) // Swap cards in player inventory
      {
        DropCard(targetCard, originalPosition);
        DropCard(card, droppedPosition);
        return;
      }
    }
    else if (_inventory.IsCardInSlot(slot)) // Card in shop but card exists in targetted slot
    {
      DeselectAllCards();
      var targetCard = _inventory.GetCardInSlot(slot);
      if (card.Name == targetCard.Name) // Combine cards of same type
      {
        targetCard.AddLevels(card.Level);
        card.CardNode.QueueFree(); // Remove dropped card node
        return;
      }
    }
    else if (card.Slot != -1) // Card in inventory
    {
      var result = _inventory.MoveCard(card, slot);
      if (result)
      {
        DropCard(card, droppedPosition);
        return;
      }
    }
    else // Card in shop
    {
      var result = _inventory.AddCard(card, slot);
      if (result)
      {
        DropCard(card, droppedPosition);
        return;
      }
    }

    // Card couldn't be dropped in slot
    DropCard(card, originalPosition);
  }

  public void _on_dropCardTimer_timeout()
  {
    _canDropCard = true;
    _dropCardTimer.Stop();
  }

  private void Button_reroll_pressed()
  {
    Console.WriteLine("Reroll button pressed");
    CardShopClear();
    CardShopFill();
  }

  private void Button_freeze_pressed()
  {
    Console.WriteLine("Freeze button pressed");
  }

  private void Button_go_pressed()
  {
    Console.WriteLine("Go button pressed");
  }

  private void DropCard(Card card, Vector2 droppedPosition)
  {
    card.CardNode.Set("_selected", false);
    card.CardNode.Set("_dropped", true);
    card.CardNode.Set("_droppedPosition", droppedPosition);
    card.CardNode.Set("_startingPosition", droppedPosition);
    card.CardNode.Set("_currentCardSlot", card.Slot);
  }

  private void DeselectAllCards()
  {
    var cards = _inventory.GetCards();
    foreach (var card in cards)
    {
      if (card == null)
      {
        continue;
      }
      card.CardNode.Set("_selected", false);
    }
  }

  private void CardShopFill()
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
      cardInstance.Set("Card", card);
      cardInstance.Connect(nameof(CardScript.droppedInSlot), this, nameof(_on_Card_droppedInSlot));
      AddChild(cardInstance);
    }
  }

  private void CardShopClear()
  {
    var cardsInScene = GetTree().GetNodesInGroup(PrepSceneData.GroupCard);
    GD.Print($"cards in scene: {cardsInScene.Count}");
    foreach (var card in cardsInScene)
    {
      GD.Print($"card type: {card.GetType()}");
      if (card is CardScript cardScript)
      {
        if (cardScript.Card.Slot == -1)
        {
          cardScript.QueueFree();
        }

      }
    }
  }
}