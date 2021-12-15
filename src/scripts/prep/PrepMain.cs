using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PrepMain : Node2D
{
  private readonly Vector2 ShopSlotOffset = new Vector2(6, 4);

  private static Inventory _inventory = new Inventory();
  private List<string> _shopCards = new List<string>();
  private List<Sprite> _cardSlots = new List<Sprite>();
  private Card _selectedCard = null;

  private Button _freezeButton;
  private Button _sellButton;
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
    _sellButton = GetNode(PrepSceneData.ButtonSellPath) as Button;
    _sellButton.Connect("pressed", this, nameof(Button_sell_pressed));
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
    for (int i = 0; i < PrepSceneData.InventorySize; i++)
    {
      var card = _inventory.GetCardInSlot(i);
      if (card == null)
      {
        cardsText += $"empty slot\n";
        continue;
      }
      cardsText += $"{card.Name} in slot {card.Slot} at level {card.Level}. Body: {card.Body}\n";
    }
    _debugInventoryLabel.Text = cardsText;
  }

  public void _on_Card_selected(Card card)
  {
    _selectedCard = card;
    EnableCardActionButtons(card.Slot == -1);
  }

  public void _on_Card_deselected(Card card)
  {
    _selectedCard = null;
    DisableCardActionButtons();
  }

  public void _on_Card_droppedOnSellButton(Card card)
  {
    SellCard();
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

  public void _on_Button_Freeze_mouse_entered()
  {
    GD.Print($"Mouse entered freeze button");
    var shopCardNodes = GetCardNodesInShop();
    foreach (var shopCardNode in shopCardNodes)
    {
      shopCardNode.Set("_mouseInCardActionButton", true);
    }
  }

  public void _on_Button_Freeze_mouse_exited()
  {
    GD.Print($"Mouse exited freeze button");
    var shopCardNodes = GetCardNodesInShop();
    foreach (var shopCardNode in shopCardNodes)
    {
      shopCardNode.Set("_mouseInCardActionButton", false);
    }
  }

  public void _on_Button_Sell_mouse_entered()
  {
    GD.Print($"Mouse entered sell button");
    var cards = _inventory.GetCards();
    foreach (var card in cards)
    {
      card.CardNode.Set("_mouseInCardActionButton", true);
    }
  }

  public void _on_Button_Sell_mouse_exited()
  {
    GD.Print($"Mouse exited sell button");
    var cards = _inventory.GetCards();
    foreach (var card in cards)
    {
      card.CardNode.Set("_mouseInCardActionButton", false);
    }
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

  private void Button_sell_pressed()
  {
    Console.WriteLine("Sell button pressed");
    SellCard();
  }

  private void Button_go_pressed()
  {
    Console.WriteLine("Go button pressed");
  }

  private void DropCard(Card card, Vector2 droppedPosition)
  {
    DisableCardActionButtons();
    card.CardNode.Set("_selected", false);
    card.CardNode.Set("_dropped", true);
    card.CardNode.Set("_droppedPosition", droppedPosition);
    card.CardNode.Set("_startingPosition", droppedPosition);
    card.CardNode.Set("_currentCardSlot", card.Slot);
  }

  private void DeselectAllCards()
  {
    DisableCardActionButtons();
    var cards = _inventory.GetCards();
    foreach (var card in cards)
    {
      card.CardNode.Set("_selected", false);
    }
  }

  private void EnableCardActionButtons(bool isInShop)
  {
    _freezeButton.Disabled = !isInShop;
    _sellButton.Disabled = isInShop;
  }

  private void DisableCardActionButtons()
  {
    _freezeButton.Disabled = true;
    _sellButton.Disabled = true;
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
      cardInstance.Connect(nameof(CardScript.droppedOnSellButton), this, nameof(_on_Card_droppedOnSellButton));
      cardInstance.Connect(nameof(CardScript.cardSelected), this, nameof(_on_Card_selected));
      cardInstance.Connect(nameof(CardScript.cardDeselected), this, nameof(_on_Card_deselected));
      AddChild(cardInstance);
    }
  }

  private void CardShopClear()
  {
    var shopCardNodes = GetCardNodesInShop();
    foreach (var shopCardNode in shopCardNodes)
    {
      shopCardNode.QueueFree();
    }
  }

  private void SellCard()
  {
    if (_selectedCard == null)
    {
      GD.Print("Error: _selectedCard is null in PrepMain.cs");
      return;
    }
    if (_selectedCard.Slot == -1)
    {
      GD.Print("Can't sell card that's in the shop");
      return;
    }
    _inventory.RemoveCard(_selectedCard.Slot);
    DisableCardActionButtons();
  }

  private List<CardScript> GetCardNodesInShop()
  {
    var shopCards = new List<CardScript>();
    var cardNodes = GetTree().GetNodesInGroup(PrepSceneData.GroupCard);
    foreach (var cardNode in cardNodes)
    {
      if (cardNode is CardScript cardScript)
      {
        if (cardScript.Card.Slot == -1)
        {
          shopCards.Add(cardScript);
        }

      }
    }
    return shopCards;
  }
}