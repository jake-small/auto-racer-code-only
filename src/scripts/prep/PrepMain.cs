using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;

public class PrepMain : Node2D
{
  private readonly Vector2 shopSlotOffset = new Vector2(6, 4);
  private static Inventory _inventory = new Inventory();
  private static Bank _bank;
  private int? _newCoinTotal;
  private Label _coinTotalLabel;
  private List<Sprite> _cardSlots = new List<Sprite>();
  private CardViewModel _selectedCard = null;
  private Button _freezeButton;
  private Button _sellButton;
  private Label _debugInventoryLabel;
  private IEnumerable<CardViewModel> _cachedDebugCards = new List<CardViewModel>();
  private Timer _dropCardTimer;
  private const float _dropCardTimerLength = 0.1f;
  private bool _canDropCard = true;

  public override void _Ready()
  {
    var bankData = LoadBankDataJson(); // TODO: load bank data from config
    GD.Print($"json bank data Starting Coins: {bankData.StartingCoins}");
    _bank = new Bank(bankData);

    CardShopFill();
    var cardSlotNodes = GetTree().GetNodesInGroup(PrepSceneData.GroupCardSlots);
    foreach (Sprite sprite in cardSlotNodes)
    {
      _cardSlots.Add(sprite);
    }

    _coinTotalLabel = GetNode<Label>(PrepSceneData.LabelCoinsPath);
    _debugInventoryLabel = GetNode<Label>(PrepSceneData.LabelDebugInventory);

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

    _newCoinTotal = _bank.SetStartingCoins();
  }

  public override void _Process(float delta)
  {
    if (_newCoinTotal != null)
    {
      _coinTotalLabel.Text = _newCoinTotal.ToString();
      _newCoinTotal = null;
    }

    var cards = _inventory.GetCards();
    if (cards.SequenceEqual(_cachedDebugCards))
    {
      return;
    }
    _cachedDebugCards = cards;
    var cardsText = "";
    for (int i = 0; i < GameData.InventorySize; i++)
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

  public void _on_Card_selected(CardViewModel card)
  {
    _selectedCard = card;
    EnableCardActionButtons(card.Slot == -1);
  }

  public void _on_Card_deselected(CardViewModel card)
  {
    _selectedCard = null;
    DisableCardActionButtons();
  }

  public void _on_Card_droppedOnFreezeButton(CardViewModel card)
  {
    FreezeCard();
  }

  public void _on_Card_droppedOnSellButton(CardViewModel card)
  {
    SellCard();
  }

  public void _on_Card_droppedInSlot(CardViewModel card, int slot, Vector2 droppedPosition, Vector2 originalPosition)
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
        var bankResult = _bank.Buy();
        if (bankResult.Success)
        {
          _newCoinTotal = bankResult.CoinTotal;
          targetCard.AddLevels(card.Level);
          card.CardNode.QueueFree(); // Remove dropped card node
          return;
        }
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
      var bankResult = _bank.Buy();
      if (bankResult.Success)
      {
        _newCoinTotal = bankResult.CoinTotal;
        var result = _inventory.AddCard(card, slot);
        if (result)
        {
          DropCard(card, droppedPosition);
          return;
        }
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
    var cards = _inventory.GetCards();
    foreach (var card in cards)
    {
      card.CardNode.MouseInCardActionButton = true;
    }
  }

  public void _on_Button_Sell_mouse_exited()
  {
    GD.Print($"Mouse exited sell button");
    var cards = _inventory.GetCards();
    foreach (var card in cards)
    {
      card.CardNode.MouseInCardActionButton = false;
    }
  }

  private void Button_reroll_pressed()
  {
    Console.WriteLine("Reroll button pressed");
    var bankResult = _bank.Reroll();
    if (bankResult.Success)
    {
      _newCoinTotal = bankResult.CoinTotal;
      CardShopClear();
      var frozenCards = GetFrozenCardNodesInShop().ToList();
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
    GetTree().ChangeScene("res://src/scenes/game/Race.tscn");
  }

  private void DropCard(CardViewModel card, Vector2 droppedPosition)
  {
    DisableCardActionButtons();
    card.CardNode.Selected = false;
    card.CardNode.Dropped = true;
    card.CardNode.DroppedPosition = droppedPosition;
    card.CardNode.StartingPosition = droppedPosition;
    card.CardNode.CurrentCardSlot = card.Slot;
  }

  private void DeselectAllCards()
  {
    DisableCardActionButtons();
    var cards = _inventory.GetCards();
    foreach (var card in cards)
    {
      card.CardNode.Selected = false;
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

  private void CardShopFill(List<CardViewModel> frozenCards = null)
  {
    frozenCards = frozenCards ?? new List<CardViewModel>();
    if (frozenCards.Count > GameData.ShopSize)
    {
      GD.Print("Error: more frozen cards than there are shop slots");
      return;
    }

    // add any cards that were frozen to scene
    for (int i = 0; i < frozenCards.Count; i++)
    {
      var frozenCard = frozenCards[i];
      CreateShopCard(frozenCard, i);
    }

    // fill in the rest of the slots with cards
    var shopService = new ShopService();
    var cards = shopService.GetRandomCards(GameData.ShopSize);
    for (int i = frozenCards.Count; i < GameData.ShopSize; i++)
    {
      var card = cards[i];
      CreateShopCard(card, i);
    }
  }

  private void CreateShopCard(CardViewModel card, int slot)
  {
    var cardScene = ResourceLoader.Load(PrepSceneData.CardScenePath) as PackedScene;
    var cardInstance = (CardScript)cardScene.Instance();
    var containerPosition = GetNode<Sprite>($"shop_slot_{slot}").Position;
    var position = containerPosition + shopSlotOffset;
    cardInstance.Position = position;
    cardInstance.CardVM = card;
    cardInstance.Frozen = card.CardNode?.Frozen ?? false;
    cardInstance.Connect(nameof(CardScript.droppedInSlot), this, nameof(_on_Card_droppedInSlot));
    cardInstance.Connect(nameof(CardScript.droppedOnSellButton), this, nameof(_on_Card_droppedOnSellButton));
    cardInstance.Connect(nameof(CardScript.droppedOnFreezeButton), this, nameof(_on_Card_droppedOnFreezeButton));
    cardInstance.Connect(nameof(CardScript.cardSelected), this, nameof(_on_Card_selected));
    cardInstance.Connect(nameof(CardScript.cardDeselected), this, nameof(_on_Card_deselected));
    AddChild(cardInstance);
  }

  private void CardShopClear()
  {
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
    if (_selectedCard.Slot != -1)
    {
      GD.Print("Can't freeze card that's in player inventory");
      return;
    }
    var selectedCardNode = (_selectedCard.CardNode as CardScript);
    selectedCardNode.Frozen = !selectedCardNode.Frozen;
  }

  private void SellCard()
  {
    DisableCardActionButtons();
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
    var bankResult = _bank.Sell();
    if (bankResult.Success)
    {
      _newCoinTotal = bankResult.CoinTotal;
      _inventory.RemoveCard(_selectedCard.Slot);
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
        if (cardScript.CardVM.Slot == -1)
        {
          shopCards.Add(cardScript);
        }
      }
    }
    return shopCards;
  }

  private IEnumerable<CardViewModel> GetFrozenCardNodesInShop()
  {
    var cardsInShop = GetCardNodesInShop();
    return cardsInShop.Where(c => c.Frozen).Select(cs => cs.CardVM).ToList();
  }

  private BankData LoadBankDataJson()
  {
    var bankDataConfigArr = System.IO.File.ReadAllLines(PrepSceneData.BankDataConfigRelativePath);
    var bankDataConfig = String.Join("\n", bankDataConfigArr);
    GD.Print($"Bank Data:\n{bankDataConfig}");
    // TODO: error handling
    return JsonSerializer.Deserialize<BankData>(bankDataConfig);
  }
}