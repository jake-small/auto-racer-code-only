using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PrepMain : Node2D
{
  private int? _newCoinTotal;
  private Label _coinTotalLabel;
  private Node2D _selectedCardPanel;
  private Label _selectedCardNameLabel;
  private Label _selectedCardDescriptionLabel;
  private Label _selectedCardBaseMoveLabel;
  private CardScript _selectedCard = null;
  private Button _freezeButton;
  private Button _sellButton;
  private Label _debugInventoryLabel;
  private Timer _dropCardTimer;
  private const float _dropCardTimerLength = 0.1f;
  private bool _canDropCard = true;
  private ShopService _shopService = new ShopService();

  public override void _Ready()
  {
    if (GameManager.LocalPlayer == null)
    {
      GameManager.LocalPlayer = new Player
      {
        Id = 0,
        Cards = GameManager.PrepEngine.PlayerInventory.GetCards(),
        Position = 0
      };
    }

    _selectedCardPanel = GetNode<Node2D>(PrepSceneData.ContainerSelectedCard);
    _selectedCardNameLabel = GetNode<Label>(PrepSceneData.LabelSelectedNamePath);
    _selectedCardDescriptionLabel = GetNode<Label>(PrepSceneData.LabelSelectedDescriptionPath);
    _selectedCardBaseMoveLabel = GetNode<Label>(PrepSceneData.LabelSelectedBaseMovePath);
    _coinTotalLabel = GetNode<Label>(PrepSceneData.LabelCoinsPath);
    _debugInventoryLabel = GetNode<Label>(PrepSceneData.LabelDebugInventory);

    _dropCardTimer = new Timer();
    _dropCardTimer.WaitTime = _dropCardTimerLength;
    _dropCardTimer.Connect("timeout", this, nameof(_on_dropCardTimer_timeout));
    AddChild(_dropCardTimer);

    var rerollButton = GetNode(PrepSceneData.ButtonRerollPath) as Button;
    GD.Print("rerollbutton connected");
    rerollButton.Connect("pressed", this, nameof(Button_reroll_pressed));
    _freezeButton = GetNode(PrepSceneData.ButtonFreezePath) as Button;
    _freezeButton.Connect("pressed", this, nameof(Button_freeze_pressed));
    _sellButton = GetNode(PrepSceneData.ButtonSellPath) as Button;
    _sellButton.Connect("pressed", this, nameof(Button_sell_pressed));
    var goButton = GetNode(PrepSceneData.ButtonGoPath) as Button;
    goButton.Connect("pressed", this, nameof(Button_go_pressed));


    PlayerInventoryFill();
    CardShopClear();
    CardShopFill();
    _newCoinTotal = GameManager.PrepEngine.Bank.SetStartingCoins();
    GameManager.PrepEngine.CalculateStartTurnAbilities();
  }

  public override void _Process(float delta)
  {
    if (_newCoinTotal != null)
    {
      _coinTotalLabel.Text = _newCoinTotal.ToString();
      _newCoinTotal = null;
    }
  }

  public void _on_Card_selected(CardScript cardScript)
  {
    _selectedCard = cardScript;
    DisplaySelectedCardData(cardScript.Card);
    EnableCardActionButtons(cardScript.IsInShop());
  }

  public void _on_Card_deselected(CardScript cardScript)
  {
    _selectedCard = null;
    HideSelectedCardData();
    DisableCardActionButtons();
  }

  public void _on_Card_droppedOnFreezeButton(CardScript cardScript)
  {
    FreezeCard();
  }

  public void _on_Card_droppedOnSellButton(CardScript cardScript)
  {
    SellCard();
  }

  public void _on_Card_droppedInSlot(CardScript cardScript, int slot, Vector2 droppedPosition, Vector2 originalPosition)
  {
    if (_canDropCard && _dropCardTimer.IsStopped())
    {
      GD.Print($"drop card timer started!!!");
      _dropCardTimer.Start();
      _canDropCard = false;
    }
    else if (!_canDropCard)
    {
      GD.Print($"Can't drop card. Too quick... Despite drop signal RECEIVED for {cardScript.Card.GetName()} at slot {cardScript.Slot} to {slot} at position {droppedPosition}");
      DropCard(cardScript, originalPosition);
      DeselectAllCards();
      return;
    }

    GD.Print($"Drop signal RECEIVED for {cardScript.Card.GetName()} at slot {cardScript.Slot} to {slot} at position {droppedPosition}");
    if (GameManager.PrepEngine.PlayerInventory.IsCardInSlot(slot) && !cardScript.IsInShop()) // Card in player inventory but card exists in targetted slot
    {
      DeselectAllCards();
      var targetCardScript = GameManager.PrepEngine.PlayerInventory.GetCardInSlot(slot);
      if (cardScript.Card.GetName() == targetCardScript.Card.GetName() && !targetCardScript.Card.IsMaxLevel()) // Combine cards of same type
      {
        CombineCards(cardScript, targetCardScript, false);
        return;
      }

      var result = GameManager.PrepEngine.PlayerInventory.SwapCards(slot, cardScript.Slot);
      if (result) // Swap cards in player inventory
      {
        DropCard(targetCardScript, originalPosition);
        DropCard(cardScript, droppedPosition);
        return;
      }
    }
    else if (GameManager.PrepEngine.PlayerInventory.IsCardInSlot(slot)) // Card in shop but card exists in targetted slot
    {
      DeselectAllCards();
      var targetCardScript = GameManager.PrepEngine.PlayerInventory.GetCardInSlot(slot);
      if (cardScript.Card.GetName() == targetCardScript.Card.GetName() && !targetCardScript.Card.IsMaxLevel()) // Combine cards of same type
      {
        var bankResult = GameManager.PrepEngine.Bank.Buy(cardScript);
        if (bankResult.Success)
        {
          _newCoinTotal = bankResult.CoinTotal;
          CombineCards(cardScript, targetCardScript, true);
          return;
        }
      }
    }
    else if (!cardScript.IsInShop()) // Card in player inventory
    {
      var result = GameManager.PrepEngine.PlayerInventory.MoveCard(cardScript, slot);
      if (result)
      {
        cardScript.Slot = slot;
        DropCard(cardScript, droppedPosition);
        return;
      }
    }
    else // Card in shop
    {
      var bankResult = GameManager.PrepEngine.Bank.Buy(cardScript);
      if (bankResult.Success)
      {
        _newCoinTotal = bankResult.CoinTotal;
        var result = GameManager.PrepEngine.PlayerInventory.AddCard(cardScript, slot, true);
        if (result)
        {
          DropCard(cardScript, droppedPosition);
          return;
        }
      }
    }

    // Card couldn't be dropped in slot
    DropCard(cardScript, originalPosition);
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
      shopCardNode.MouseInCardActionButton = true;
    }
  }

  public void _on_Button_Freeze_mouse_exited()
  {
    GD.Print($"Mouse exited freeze button");
    var shopCardNodes = GetCardNodesInShop();
    foreach (var shopCardNode in shopCardNodes)
    {
      shopCardNode.MouseInCardActionButton = false;
    }
  }

  public void _on_Button_Sell_mouse_entered()
  {
    GD.Print($"Mouse entered sell button");
    foreach (var cardScript in GameManager.PrepEngine.PlayerInventory.GetCardsAsList())
    {
      cardScript.MouseInCardActionButton = true;
    }
  }

  public void _on_Button_Sell_mouse_exited()
  {
    GD.Print($"Mouse exited sell button");
    foreach (var cardScript in GameManager.PrepEngine.PlayerInventory.GetCardsAsList())
    {
      cardScript.MouseInCardActionButton = false;
    }
  }

  private void Button_reroll_pressed()
  {
    Console.WriteLine("Reroll button pressed");
    var bankResult = GameManager.PrepEngine.Bank.Reroll();
    if (bankResult.Success)
    {
      _newCoinTotal = bankResult.CoinTotal;
      var frozenCards = GetFrozenCardNodesInShop().ToList();
      CardShopClear();
      CardShopFill(frozenCards);
    }
  }

  private void Button_freeze_pressed()
  {
    Console.WriteLine("Freeze button pressed");
    FreezeCard();
  }

  private void Button_sell_pressed()
  {
    Console.WriteLine("Sell button pressed");
    SellCard();
  }

  private void Button_go_pressed()
  {
    Console.WriteLine("Go button pressed");
    GameManager.PrepEngine.CalculateEndTurnAbilities();
    GameManager.RaceNumber = GameManager.RaceNumber + 1;
    GameManager.LocalPlayer.Cards = GameManager.PrepEngine.PlayerInventory.GetCards();
    GetTree().ChangeScene("res://src/scenes/game/Race.tscn");
  }

  private void DropCard(CardScript cardScript, Vector2 droppedPosition)
  {
    DisableCardActionButtons();
    cardScript.Selected = false;
    cardScript.Dropped = true;
    cardScript.DroppedPosition = droppedPosition;
    cardScript.StartingPosition = droppedPosition;
  }

  private void DeselectAllCards()
  {
    DisableCardActionButtons();
    foreach (var cardScript in GameManager.PrepEngine.PlayerInventory.GetCardsAsList())
    {
      cardScript.Selected = false;
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

  private void PlayerInventoryFill()
  {
    var cardDict = GameManager.LocalPlayer?.Cards;
    foreach (var item in cardDict)
    {
      if (item.Value is CardEmpty)
      {
        continue;
      }
      CreateCardScript(item.Value, item.Key, false);
    }
  }

  private void CardShopFill(List<CardScript> frozenCards = null)
  {
    frozenCards = frozenCards ?? new List<CardScript>();
    if (frozenCards.Count > GameData.ShopSize)
    {
      GD.Print("Error: more frozen cards than there are shop slots");
      return;
    }

    // add any cards that were frozen to scene
    for (int i = 0; i < frozenCards.Count; i++)
    {
      var frozenCard = frozenCards[i];
      CreateCardScript(frozenCard.Card, i, true, true);
    }

    // fill in the rest of the slots with cards
    var cards = _shopService.GetRandomCards(GameData.ShopSize);
    for (int i = frozenCards.Count; i < GameData.ShopSize; i++)
    {
      var card = cards[i];
      // var cardScript = new CardScript(card);
      CreateCardScript(card, i, true);
    }
  }

  private void CreateCardScript(Card card, int slot, bool inShopInventory, bool isFrozen = false)
  {
    var cardScene = ResourceLoader.Load(PrepSceneData.CardScenePath) as PackedScene;
    var cardInstance = (CardScript)cardScene.Instance();
    var spriteName = inShopInventory ? "shop_slot_" : "slot_";
    var containerPosition = GetNode<Sprite>($"{spriteName}{slot}").Position;
    var position = containerPosition + PrepSceneData.CardSlotOffset;
    cardInstance.Card = card;
    cardInstance.Slot = slot;
    cardInstance.Position = position;
    cardInstance.Frozen = isFrozen;
    cardInstance.Inventory = inShopInventory ? InventoryTarget.Shop : InventoryTarget.Player;
    cardInstance.Connect(nameof(CardScript.droppedInSlot), this, nameof(_on_Card_droppedInSlot));
    cardInstance.Connect(nameof(CardScript.droppedOnSellButton), this, nameof(_on_Card_droppedOnSellButton));
    cardInstance.Connect(nameof(CardScript.droppedOnFreezeButton), this, nameof(_on_Card_droppedOnFreezeButton));
    cardInstance.Connect(nameof(CardScript.cardSelected), this, nameof(_on_Card_selected));
    cardInstance.Connect(nameof(CardScript.cardDeselected), this, nameof(_on_Card_deselected));
    AddChild(cardInstance);
    if (!inShopInventory)
    {
      GameManager.PrepEngine.PlayerInventory.RemoveCard(slot);
    }
    var result = inShopInventory
      ? GameManager.PrepEngine.ShopInventory.AddCard(cardInstance, slot)
      : GameManager.PrepEngine.PlayerInventory.AddCard(cardInstance, slot, false);
    if (!result)
    {
      GD.Print($"Error adding card to shop inventory {card.GetRawName()} at slot {slot}");
    }
  }

  private void CardShopClear()
  {
    GameManager.PrepEngine.ShopInventory.Clear();
    var shopCardNodes = GetCardNodesInShop();
    foreach (var shopCardNode in shopCardNodes)
    {
      shopCardNode.QueueFree();
    }
  }

  private void FreezeCard()
  {
    DisableCardActionButtons();
    if (_selectedCard == null)
    {
      GD.Print("Error: _selectedCard is null in PrepMain.cs");
      return;
    }
    if (!_selectedCard.IsInShop())
    {
      GD.Print("Can't freeze card that's in player inventory");
      return;
    }
    _selectedCard.Frozen = !_selectedCard.Frozen;
  }

  private void SellCard()
  {
    DisableCardActionButtons();
    if (_selectedCard == null)
    {
      GD.Print("Error: _selectedCard is null in PrepMain.cs");
      return;
    }
    if (_selectedCard.IsInShop())
    {
      GD.Print("Can't sell card that's in the shop");
      return;
    }
    var bankResult = GameManager.PrepEngine.Bank.Sell(_selectedCard);
    if (bankResult.Success)
    {
      _newCoinTotal = bankResult.CoinTotal;
      var result = GameManager.PrepEngine.PlayerInventory.RemoveCard(_selectedCard.Slot);
      if (result)
      {
        // Remove card node
        _selectedCard.QueueFree();
      }
    }
  }

  private void CombineCards(CardScript droppedCardScript, CardScript targetCardScript, bool fromShopInventory)
  {
    if (targetCardScript.Card.Level >= droppedCardScript.Card.Level || fromShopInventory)
    {
      targetCardScript.Card.AddExp(droppedCardScript.Card.Exp);
      targetCardScript.UpdateUi();
      if (!fromShopInventory)
      {
        GameManager.PrepEngine.PlayerInventory.RemoveCard(droppedCardScript.Slot); // Remove dropped card
      }

      droppedCardScript.QueueFree(); // Remove dropped card node
    }
    else
    {
      droppedCardScript.Card.AddExp(targetCardScript.Card.Exp);
      DropCard(droppedCardScript, targetCardScript.Position);
      droppedCardScript.UpdateUi();
      var targetSlot = targetCardScript.Slot;
      GameManager.PrepEngine.PlayerInventory.RemoveCard(droppedCardScript.Slot); // Remove dropped card
      GameManager.PrepEngine.PlayerInventory.RemoveCard(targetSlot); // Remove target card
      GameManager.PrepEngine.PlayerInventory.AddCard(droppedCardScript, targetSlot, false);
      targetCardScript.QueueFree(); // Remove dropped card node
    }
  }

  private IEnumerable<CardScript> GetCardNodesInShop()
  {
    var shopCards = new List<CardScript>();
    var cardNodes = GetTree().GetNodesInGroup(PrepSceneData.GroupCard);
    foreach (var cardNode in cardNodes)
    {
      if (cardNode is CardScript cardScript)
      {
        if (cardScript.IsInShop())
        {
          shopCards.Add(cardScript);
        }
      }
    }
    return shopCards;
  }

  private IEnumerable<CardScript> GetFrozenCardNodesInShop()
  {
    var cardsInShop = GetCardNodesInShop();
    return cardsInShop.Where(c => c.Frozen).ToList();
  }

  private void DisplaySelectedCardData(Card card)
  {
    _selectedCardPanel.Visible = true;
    _selectedCardNameLabel.Text = card.GetName();
    _selectedCardDescriptionLabel.Text = card.GetDescription();
    _selectedCardBaseMoveLabel.Text = card.BaseMove.ToString();
  }

  private void HideSelectedCardData()
  {
    _selectedCardPanel.Visible = false;
    _selectedCardNameLabel.Text = "";
    _selectedCardDescriptionLabel.Text = "";
    _selectedCardBaseMoveLabel.Text = "";
  }
}