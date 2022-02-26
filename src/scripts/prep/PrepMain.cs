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
  private Label _selectedCardSellsForLabel;
  private Label _selectedCardBaseMoveLabel;
  private CardScript _selectedCard = null;
  private Button _freezeButton;
  private Button _sellButton;
  private Label _debugInventoryLabel;
  private Timer _dropCardTimer;
  private const float _dropCardTimerLength = 0.1f;
  private bool _canDropCard = true;

  public override void _Ready()
  {
    if (GameManager.LocalPlayer is null)
    {
      GameManager.LocalPlayer = new Player
      {
        Id = 0,
        Cards = GameManager.PrepEngine.PlayerInventory.GetCards(),
        Position = 0
      };
    }

    var firstPlacesLabel = GetNode<Label>(PrepSceneData.LabelNumFirstPlaces);
    var secondPlacesLabel = GetNode<Label>(PrepSceneData.LabelNumSecondPlaces);
    var thirdPlacesLabel = GetNode<Label>(PrepSceneData.LabelNumThirdPlaces);
    var fourthPlacesLabel = GetNode<Label>(PrepSceneData.LabelNumFourthPlaces);
    firstPlacesLabel.Text = GameManager.Score.GetResult(1).ToString();
    secondPlacesLabel.Text = GameManager.Score.GetResult(2).ToString();
    thirdPlacesLabel.Text = GameManager.Score.GetResult(3).ToString();
    fourthPlacesLabel.Text = GameManager.Score.GetResult(4).ToString();

    var raceLabel = GetNode<Label>(PrepSceneData.LabelRaceTotalPath);
    raceLabel.Text = GameManager.CurrentRace.ToString();
    var heartLabel = GetNode<Label>(PrepSceneData.LabelHeartsPath);
    heartLabel.Text = GameManager.LifeTotal.ToString();

    _selectedCardPanel = GetNode<Node2D>(PrepSceneData.ContainerSelectedCard);
    _selectedCardNameLabel = GetNode<Label>(PrepSceneData.LabelSelectedNamePath);
    _selectedCardDescriptionLabel = GetNode<Label>(PrepSceneData.LabelSelectedDescriptionPath);
    _selectedCardSellsForLabel = GetNode<Label>(PrepSceneData.LabelSelectedSellsForPath);
    _selectedCardBaseMoveLabel = GetNode<Label>(PrepSceneData.LabelSelectedBaseMovePath);
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


    PlayerInventoryFill();
    var frozenCards = GetFrozenCards().ToList();
    CardShopFill(frozenCards);
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
    DeselectCard();
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
      var targetCard = GameManager.PrepEngine.PlayerInventory.GetCardInSlot(slot);
      var targetCardScript = GetCardScriptsInScene().FirstOrDefault(c => c.Card == targetCard);
      if (cardScript.Card.GetName() == targetCardScript.Card.GetName() && !targetCardScript.Card.IsMaxLevel()) // Combine cards of same type
      {
        var combineResult = CombineCards(cardScript, targetCardScript, false);
        if (combineResult)
        {
          return;
        }
      }

      var result = GameManager.PrepEngine.PlayerInventory.SwapCards(slot, cardScript.Slot);
      if (result) // Swap cards in player inventory
      {
        targetCardScript.Slot = cardScript.Slot;
        cardScript.Slot = slot;
        DropCard(targetCardScript, originalPosition);
        DropCard(cardScript, droppedPosition);
        return;
      }
    }
    else if (GameManager.PrepEngine.PlayerInventory.IsCardInSlot(slot) && cardScript.IsInShop()) // Card in shop but card exists in targetted slot
    {
      DeselectAllCards();
      var targetCard = GameManager.PrepEngine.PlayerInventory.GetCardInSlot(slot);
      var targetCardScript = GetCardScriptsInScene().FirstOrDefault(c => c.Card == targetCard);
      if (cardScript.Card.GetName() == targetCardScript.Card.GetName() && !targetCardScript.Card.IsMaxLevel()) // Combine cards of same type
      {
        DeselectAllCards();
        var bankResult = GameManager.PrepEngine.Bank.Buy(cardScript.Card);
        if (bankResult.Success)
        {
          _newCoinTotal = bankResult.CoinTotal;
          var combineResult = CombineCards(cardScript, targetCardScript, true);
          if (combineResult)
          {
            return;
          }
        }
      }
    }
    else if (!cardScript.IsInShop()) // Card in player inventory
    {
      var result = GameManager.PrepEngine.PlayerInventory.MoveCard(cardScript.Card, cardScript.Slot, slot);
      if (result)
      {
        cardScript.Slot = slot;
        DropCard(cardScript, droppedPosition);
        return;
      }
    }
    else // Card in shop
    {
      DeselectAllCards();
      var bankResult = GameManager.PrepEngine.Bank.Buy(cardScript.Card);
      if (bankResult.Success)
      {
        _newCoinTotal = bankResult.CoinTotal;
        var fromSlot = cardScript.Slot;
        var result = GameManager.PrepEngine.PlayerInventory.AddCard(cardScript.Card, slot);
        if (result)
        {
          GameManager.PrepEngine.ShopInventory.RemoveCard(fromSlot);
          cardScript.Slot = slot;
          cardScript.Card.Frozen = false;
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
    var shopCardNodes = GetShopCardNodes();
    foreach (var shopCardNode in shopCardNodes)
    {
      shopCardNode.MouseInCardActionButton = true;
    }
  }

  public void _on_Button_Freeze_mouse_exited()
  {
    GD.Print($"Mouse exited freeze button");
    var shopCardNodes = GetShopCardNodes();
    foreach (var shopCardNode in shopCardNodes)
    {
      shopCardNode.MouseInCardActionButton = false;
    }
  }

  public void _on_Button_Sell_mouse_entered()
  {
    GD.Print($"Mouse entered sell button");
    foreach (var cardScript in GetPlayerCardNodes())
    {
      cardScript.MouseInCardActionButton = true;
    }
  }

  public void _on_Button_Sell_mouse_exited()
  {
    GD.Print($"Mouse exited sell button");
    foreach (var cardScript in GetPlayerCardNodes())
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
      var frozenCards = GetFrozenCards().ToList();
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
    GameManager.CurrentRace = GameManager.CurrentRace + 1;
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
    UpdateUiForAllCards();
  }

  private void UpdateUiForAllCards()
  {
    foreach (var cardScript in GetCardScriptsInScene())
    {
      cardScript.UpdateUi();
    }
  }

  private void DeselectCard()
  {
    _selectedCard = null;
    HideSelectedCardData();
    DisableCardActionButtons();
  }

  private void DeselectAllCards()
  {
    DeselectCard();
    foreach (var cardScript in GetPlayerCardNodes())
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
    foreach (var (slot, card) in cardDict.Select(d => (d.Key, d.Value)))
    {
      if (card is CardEmpty)
      {
        continue;
      }
      CreateCardScript(card, slot, false);
      var addResult = GameManager.PrepEngine.PlayerInventory.AddCard(card, slot);
      if (!addResult)
      {
        GD.Print($"Error adding card to shop inventory {card.GetRawName()} at slot {slot}");
      }
    }
  }

  private void CardShopFill(List<Card> frozenCards = null)
  {
    CardShopClear();
    frozenCards = frozenCards ?? new List<Card>();
    if (frozenCards.Count > GameData.ShopInventorySize)
    {
      GD.Print("Error: more frozen cards than there are shop slots");
      return;
    }

    // add any cards that were frozen to scene
    for (int slot = 0; slot < frozenCards.Count; slot++)
    {
      var frozenCard = frozenCards[slot];
      CreateCardScript(frozenCard, slot, true, true);
      GameManager.PrepEngine.ShopInventory.AddCard(frozenCard, slot);
    }

    // fill in the rest of the slots with cards
    var cards = GameManager.PrepEngine.ShopService.GetRandomCards(GameData.ShopInventorySize);
    for (int slot = frozenCards.Count; slot < GameData.ShopInventorySize; slot++)
    {
      var card = cards[slot];
      CreateCardScript(card, slot, true);
      GameManager.PrepEngine.ShopInventory.AddCard(card, slot);
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
    cardInstance.Card.Frozen = isFrozen;
    cardInstance.Card.InventoryType = inShopInventory ? InventoryType.Shop : InventoryType.Player;
    cardInstance.Connect(nameof(CardScript.droppedInSlot), this, nameof(_on_Card_droppedInSlot));
    cardInstance.Connect(nameof(CardScript.droppedOnSellButton), this, nameof(_on_Card_droppedOnSellButton));
    cardInstance.Connect(nameof(CardScript.droppedOnFreezeButton), this, nameof(_on_Card_droppedOnFreezeButton));
    cardInstance.Connect(nameof(CardScript.cardSelected), this, nameof(_on_Card_selected));
    cardInstance.Connect(nameof(CardScript.cardDeselected), this, nameof(_on_Card_deselected));
    AddChild(cardInstance);
  }

  private void CardShopClear()
  {
    var shopCardNodes = GetShopCardNodes();
    GameManager.PrepEngine.ShopInventory.Clear();
    foreach (var shopCardNode in shopCardNodes)
    {
      if (IsInstanceValid(shopCardNode))
      {
        shopCardNode.QueueFree();
      }
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
    _selectedCard.Card.Frozen = !_selectedCard.Card.Frozen;

    UpdateUiForAllCards();
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
    var bankResult = GameManager.PrepEngine.Bank.Sell(_selectedCard.Card);
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
    UpdateUiForAllCards();
  }

  private bool CombineCards(CardScript droppedCardScript, CardScript targetCardScript, bool fromShopInventory)
  {
    if (droppedCardScript.Card.IsMaxLevel() || targetCardScript.Card.IsMaxLevel())
    {
      return false;
    }

    if (targetCardScript.Card.Level >= droppedCardScript.Card.Level || fromShopInventory)
    {
      targetCardScript.Card.AddExp(droppedCardScript.Card.Exp);
      targetCardScript.Card.CombineBaseMove(droppedCardScript.Card.BaseMove);
      targetCardScript.UpdateUi();
      if (fromShopInventory)
      {
        GameManager.PrepEngine.ShopInventory.RemoveCard(droppedCardScript.Slot); // Remove dropped card
      }
      else
      {
        GameManager.PrepEngine.PlayerInventory.RemoveCard(droppedCardScript.Slot); // Remove dropped card
      }
      droppedCardScript.QueueFree(); // Remove dropped card node
    }
    else
    {
      droppedCardScript.Card.AddExp(targetCardScript.Card.Exp);
      droppedCardScript.Card.CombineBaseMove(targetCardScript.Card.BaseMove);
      DropCard(droppedCardScript, targetCardScript.Position);
      var targetSlot = targetCardScript.Slot;
      GameManager.PrepEngine.PlayerInventory.RemoveCard(droppedCardScript.Slot); // Remove dropped card
      GameManager.PrepEngine.PlayerInventory.RemoveCard(targetSlot); // Remove target card
      var addResult = GameManager.PrepEngine.PlayerInventory.AddCard(droppedCardScript.Card, targetSlot);
      if (!addResult)
      {
        throw new Exception("Unable to add card in PrepMain.CombineCards()");
      }
      droppedCardScript.Slot = targetSlot;
      targetCardScript.QueueFree(); // Remove dropped card node
    }
    return true;
  }

  private List<CardScript> GetCardScriptsInScene()
  {
    var cardScriptNodes = GetTree().GetNodesInGroup(PrepSceneData.GroupCard);
    var cardScripts = new List<CardScript>();
    foreach (CardScript cardScript in cardScriptNodes)
    {
      cardScripts.Add(cardScript);
    }
    return cardScripts;
  }

  private CardScript GetCardNode(Card card)
  {
    return GetCardScriptsInScene().FirstOrDefault(c => c.Card == card);
  }

  private IEnumerable<CardScript> GetPlayerCardNodes()
  {
    var playerCards = GameManager.PrepEngine.PlayerInventory.GetCardsAsList();
    return GetCardScriptsInScene().Where(c => playerCards.Contains(c.Card));
  }

  private IEnumerable<CardScript> GetShopCardNodes()
  {
    var shopCards = GameManager.PrepEngine.ShopInventory.GetCardsAsList();
    return GetCardScriptsInScene().Where(c => shopCards.Contains(c.Card));
  }

  private IEnumerable<Card> GetFrozenCards()
  {
    return GameManager.PrepEngine.ShopInventory.GetCardsAsList().Where(c => c.Frozen);
  }

  private void DisplaySelectedCardData(Card card)
  {
    _selectedCardPanel.Visible = true;
    _selectedCardNameLabel.Text = card.GetName();
    _selectedCardDescriptionLabel.Text = card.GetDescription();
    _selectedCardSellsForLabel.Text = GameManager.PrepEngine.Bank.GetSellValue(card).ToString();
    _selectedCardBaseMoveLabel.Text = card.BaseMove.ToString();
  }

  private void HideSelectedCardData()
  {
    _selectedCardPanel.Visible = false;
    _selectedCardNameLabel.Text = "";
    _selectedCardDescriptionLabel.Text = "";
    _selectedCardSellsForLabel.Text = "";
    _selectedCardBaseMoveLabel.Text = "";
  }
}