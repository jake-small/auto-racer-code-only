using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PrepMain : Node2D
{
  private Node2D _firebaseNode;
  private int? _newCoinTotal;
  private Label _coinTotalLabel;
  private CardInfoScript _selectedCardInfo;
  private CardScript _selectedCard = null;
  private CostButtonUi _rerollButton;
  private TextureButton _freezeButton;
  private CostButtonUi _sellButton;
  private TextureButton _goButton;
  private AnimationPlayer _animationPlayerGold;
  private List<Node2D> _cardCostContainers;
  private Label _debugInventoryLabel;
  private Timer _dropCardTimer;
  private const float _dropCardTimerLength = 0.1f;
  private bool _canDropCard = true;
  private bool _waitingOnCardEffect;

  public override void _Ready()
  {
    if (GameManager.CurrentRace > 0 && GameManager.ShopSize < 6 && GameManager.CurrentRace % GameData.TierIncreaseEveryNLevels == 0)
    {
      GameManager.ShopSize += 1;
    }

    if (GameManager.ShowTutorial)
    {
      if (GameManager.CurrentRace == 0)
      {
        GetNode<Node2D>(PrepSceneData.TutorialPath1).Visible = true;
        GetNode<Node2D>(PrepSceneData.TutorialPath2).Visible = false;
      }
      else if (GameManager.CurrentRace == 1)
      {
        GetNode<Node2D>(PrepSceneData.TutorialPath1).Visible = false;
        GetNode<Node2D>(PrepSceneData.TutorialPath2).Visible = true;
      }
      else
      {
        GetNode<Node2D>(PrepSceneData.TutorialPath1).Visible = false;
        GetNode<Node2D>(PrepSceneData.TutorialPath2).Visible = false;
      }
    }
    _firebaseNode = GetNode<Node2D>("PrepFirebase");
    var playerNameLabel = GetNode<Label>(PrepSceneData.LabelPlayerName);
    playerNameLabel.Text = GameManager.LocalPlayer.Name;
    var playerCharacter = GetNode<CharacterScript>(PrepSceneData.CharacterPath);
    playerCharacter.CharacterSkin = GameManager.LocalPlayer.Skin;

    var firstPlacesLabel = GetNode<Label>(PrepSceneData.LabelNumFirstPlaces);
    var secondPlacesLabel = GetNode<Label>(PrepSceneData.LabelNumSecondPlaces);
    var thirdPlacesLabel = GetNode<Label>(PrepSceneData.LabelNumThirdPlaces);
    var fourthPlacesLabel = GetNode<Label>(PrepSceneData.LabelNumFourthPlaces);
    if (GameManager.NumPlayers > 0)
    {
      firstPlacesLabel.Text = GameManager.Score.GetResult(0).ToString();
    }
    else
    {
      GetNode<Sprite>("Container_win_record/trophy").Visible = false;
      firstPlacesLabel.Visible = false;
    }
    if (GameManager.NumPlayers > 1)
    {
      secondPlacesLabel.Text = GameManager.Score.GetResult(1).ToString();
    }
    else
    {
      GetNode<Sprite>("Container_win_record/medal2").Visible = false;
      secondPlacesLabel.Visible = false;
    }
    if (GameManager.NumPlayers > 2)
    {
      thirdPlacesLabel.Text = GameManager.Score.GetResult(2).ToString();
    }
    else
    {
      GetNode<Sprite>("Container_win_record/medal1").Visible = false;
      thirdPlacesLabel.Visible = false;
    }
    if (GameManager.NumPlayers > 3)
    {
      fourthPlacesLabel.Text = GameManager.Score.GetResult(3).ToString();
    }
    else
    {
      GetNode<Sprite>("Container_win_record/star").Visible = false;
      fourthPlacesLabel.Visible = false;
    }

    if (GameManager.NumPlayers == 2)
    {
      firstPlacesLabel.RectPosition = new Vector2(firstPlacesLabel.RectPosition.x + 55, firstPlacesLabel.RectPosition.y);
      var firstPlaceIcon = GetNode<Sprite>("Container_win_record/trophy");
      firstPlaceIcon.Position = new Vector2(firstPlaceIcon.Position.x + 55, firstPlaceIcon.Position.y);
      secondPlacesLabel.RectPosition = new Vector2(secondPlacesLabel.RectPosition.x + 55, secondPlacesLabel.RectPosition.y);
      var secondPlaceIcon = GetNode<Sprite>("Container_win_record/medal2");
      secondPlaceIcon.Position = new Vector2(secondPlaceIcon.Position.x + 55, secondPlaceIcon.Position.y);
    }

    var raceLabel = GetNode<Label>(PrepSceneData.LabelRaceTotalPath);
    raceLabel.Text = $"{GameManager.CurrentRace + 1}/{GameManager.TotalRaces}";
    var heartLabel = GetNode<Label>(PrepSceneData.LabelHeartsPath);
    heartLabel.Text = GameManager.LifeTotal.ToString();
    _selectedCardInfo = GetNode<CardInfoScript>(PrepSceneData.SelectedCardInfo);
    _coinTotalLabel = GetNode<Label>(PrepSceneData.LabelCoinsPath);
    _debugInventoryLabel = GetNode<Label>(PrepSceneData.LabelDebugInventory);

    _cardCostContainers = new List<Node2D>();
    for (var i = 0; i < GameManager.ShopSize; i++)
    {
      var shopSlot = GetNode<Sprite>(PrepSceneData.ShopSlotPrefix + $"{i}");
      shopSlot.Visible = true;
      _cardCostContainers.Add(GetNode<Node2D>(PrepSceneData.ContainerCardCostPrefix + $"{i}"));
    }

    _dropCardTimer = new Timer();
    _dropCardTimer.WaitTime = _dropCardTimerLength;
    _dropCardTimer.Connect("timeout", this, nameof(_on_dropCardTimer_timeout));
    AddChild(_dropCardTimer);

    _rerollButton = GetNode<CostButtonUi>(PrepSceneData.ButtonRerollPath);
    _rerollButton.Connect("pressed", this, nameof(Button_reroll_pressed));
    _rerollButton.Cost = GameManager.PrepEngine.Bank.RerollCost;
    _freezeButton = GetNode<TextureButton>(PrepSceneData.ButtonFreezePath);
    _freezeButton.Connect("pressed", this, nameof(Button_freeze_pressed));
    _sellButton = GetNode<CostButtonUi>(PrepSceneData.ButtonSellPath);
    _sellButton.Connect("pressed", this, nameof(Button_sell_pressed));
    _sellButton.CostVisible = false;
    _goButton = GetNode<TextureButton>(PrepSceneData.ButtonGoPath);
    _goButton.Connect("pressed", this, nameof(Button_go_pressed));
    _goButton.Disabled = false;
    _animationPlayerGold = GetNode<AnimationPlayer>(PrepSceneData.AnimationPlayerGoldPath);

    PlayerInventoryFill();
    var frozenCards = GetFrozenCards().ToList();
    CardShopFill(frozenCards);
    GameManager.PrepEngine.Bank.SetStartingCoins();
    var prepAbilityResults = GameManager.PrepEngine.CalculateStartTurnAbilities();
    _newCoinTotal = GameManager.PrepEngine.Bank.CoinTotal;
    UpdateUiForAllCards();
    AnimatePrepAbilityEffects(prepAbilityResults);

    DisableCardActionButtons();
  }

  public override void _Process(float delta)
  {
    if (_newCoinTotal != null)
    {
      _coinTotalLabel.Text = _newCoinTotal.ToString();
      _newCoinTotal = null;
    }

    if (_waitingOnCardEffect)
    {
      DisableNonCardActionButtons();
      if (GetTree().GetNodesInGroup(RaceSceneData.GroupProjectiles).Count <= 0)
      {
        _waitingOnCardEffect = false;
        EnableNonCardActionButtons();
      }
    }

    if (_selectedCard == null)
    {
      DisableCardActionButtons();
    }
  }

  public void _on_Card_selected(CardScript cardScript)
  {
    _selectedCard = cardScript;
    DisplaySelectedCardData(cardScript);
    EnableCardActionButtons(cardScript.IsInShop());
  }

  public void _on_Card_deselected(CardScript cardScript)
  {
    DeselectCard();
  }

  public void _on_Card_droppedOnFreezeButton(CardScript cardScript)
  {
    var mousePosition = GetGlobalMousePosition();
    var freezeButtonRect = new Rect2(_freezeButton.RectGlobalPosition, _freezeButton.RectSize * 1.5f);
    if (freezeButtonRect.HasPoint(mousePosition))
    {
      FreezeCard();
    }
    DropCard(_selectedCard, _selectedCard.StartingPosition);
  }

  public void _on_Card_droppedOnSellButton(CardScript cardScript)
  {
    var mousePosition = GetGlobalMousePosition();
    var sellButtonRect = new Rect2(_sellButton.RectGlobalPosition, _sellButton.RectSize * 1.5f);
    if (sellButtonRect.HasPoint(mousePosition))
    {
      SellCard();
    }
    else
    {
      DropCard(_selectedCard, _selectedCard.StartingPosition);
    }
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
          AnimatePrepAbilityEffects(bankResult.PrepAbilityResults);
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
        DeselectCard();
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
          AnimatePrepAbilityEffects(bankResult.PrepAbilityResults);
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

  public void SetupRace(Godot.Collections.Array<Godot.Collections.Dictionary> gdFirebaseOpponentTurns)
  {
    GD.Print($"Setting up race...");
    GD.Print(gdFirebaseOpponentTurns);
    var id = 1;
    var opponents = new List<Player>();
    foreach (var opponentTurn in gdFirebaseOpponentTurns.ToArray())
    {
      var firebasePlayer = new FirebasePlayer(id, new FirebasePlayerTurn(opponentTurn), GameManager.PrepEngine.ShopService.GetAvailableCards());
      opponents.Add(firebasePlayer);
      id = id + 1;
    }
    GameManager.Opponents = opponents;
    GetTree().ChangeScene("res://src/scenes/game/Race.tscn");
  }

  private void Button_go_pressed()
  {
    _goButton.Disabled = true;
    Console.WriteLine("Go button pressed");
    var goButtonLabel = _goButton.GetNode<Label>("Label");
    goButtonLabel.Text = "searching...";
    var prepAbilityResults = GameManager.PrepEngine.CalculateEndTurnAbilities();
    AnimatePrepAbilityEffects(prepAbilityResults);
    // TODO: wait for animations to finish
    GameManager.CurrentRace = GameManager.CurrentRace + 1;
    GameManager.LocalPlayer.Cards = GameManager.PrepEngine.PlayerInventory.GetCards();
    if (GameManager.VsBots)
    {
      GetTree().ChangeScene("res://src/scenes/game/Race.tscn");
      return;
    }
    var firebaseCards = new FirebaseCards(GameManager.LocalPlayer.Cards).GodotCards;
    _firebaseNode.Call("SendPlayerTurn", this, GameManager.LocalPlayer.Name, GameManager.LocalPlayer.Skin,
      GameManager.NumPlayers - 1, GameManager.CurrentRace, firebaseCards,
      GameManager.PrepEngine.ShopService.CardVersion ?? "null");
  }

  private void Button_reroll_pressed()
  {
    Console.WriteLine("Reroll button pressed");
    var bankResult = GameManager.PrepEngine.Bank.Reroll();
    if (bankResult.Success)
    {
      _newCoinTotal = bankResult.CoinTotal;
      Reroll();
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

  private void Reroll()
  {
    var frozenCards = GetFrozenCards().ToList();
    CardShopFill(frozenCards);
  }

  private void DropCard(CardScript cardScript, Vector2 droppedPosition)
  {
    DisableCardActionButtons();
    HideSelectedCardData();
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

  private void EnableNonCardActionButtons()
  {
    _rerollButton.Disabled = false;
    _goButton.Disabled = false;
  }

  private void DisableNonCardActionButtons()
  {
    _rerollButton.Disabled = true;
    _goButton.Disabled = true;
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
    if (frozenCards.Count > GameManager.ShopSize)
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
    var cards = GameManager.PrepEngine.ShopService.GetRandomCards(GameManager.ShopSize, GameManager.CurrentRace);
    for (int slot = frozenCards.Count; slot < GameManager.ShopSize; slot++)
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
    cardInstance.OnShopSpawn();
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
    HideSelectedCardData();
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
        AnimatePrepAbilityEffects(bankResult.PrepAbilityResults);
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
      var leveledUp = targetCardScript.Card.AddExp(droppedCardScript.Card.Exp);
      if (leveledUp)
      {
        targetCardScript.OnLevelUp();
      }
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
      targetCardScript.OnExpGainAnimate();
    }
    else
    {
      var leveledUp = droppedCardScript.Card.AddExp(targetCardScript.Card.Exp);
      if (leveledUp)
      {
        droppedCardScript.OnLevelUp();
      }
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
      droppedCardScript.OnExpGainAnimate();
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

  private void DisplaySelectedCardData(CardScript cardScript)
  {
    if (cardScript.IsInShop())
    {
      _cardCostContainers[cardScript.Slot].Visible = true;
    }
    var card = cardScript.Card;
    _selectedCardInfo.SetCard(card);
    _sellButton.Cost = GameManager.PrepEngine.Bank.GetSellValue(card);
    _sellButton.CostVisible = true;
  }

  private void HideSelectedCardData()
  {
    foreach (var container in _cardCostContainers)
    {
      container.Visible = false;
    }
    _selectedCardInfo.Clear();
    _sellButton.CostVisible = false;
  }

  private void AnimatePrepAbilityEffects(IEnumerable<PrepAbilityResult> abilityResults)
  {
    foreach (var ability in abilityResults)
    {
      switch (ability.Effect)
      {
        case Effect.Basemove:
          BaseMoveEffectAnimation(ability);
          break;
        case Effect.Exp:
          ExperienceEffectAnimation(ability);
          break;
        case Effect.Gold:
          GoldEffectAnimation(ability);
          break;
        case Effect.Reroll:
          RerollEffectAnimation(ability);
          break;
      }
    }
  }

  private void BaseMoveEffectAnimation(PrepAbilityResult ability)
  {
    var cardScript = GetCardScriptsInScene().FirstOrDefault(c => c.Card == ability.Card);
    if (cardScript == null)
    {
      return;
    }
    var cardScriptSize = cardScript.GetBackgroundSprite().Texture.GetSize();
    var spawn = new Vector2(cardScript.Position.x + (cardScriptSize.x / 2), cardScript.Position.y);
    var projectileScene = ResourceLoader.Load("res://src/scenes/objects/effects/PrepProjectileBaseMove.tscn") as PackedScene;
    foreach (var targetCard in ability.Targets)
    {
      var selfBuff = ability.Card == targetCard;
      var targetCardScript = GetCardScriptsInScene().FirstOrDefault(c => c.Card == targetCard);
      SpawnProjectiles(spawn, targetCardScript.Position, cardScriptSize, ability.Value, selfBuff, projectileScene, AddBaseMoveCallBack, targetCard);
    };
  }

  private void ExperienceEffectAnimation(PrepAbilityResult ability)
  {
    var cardScript = GetCardScriptsInScene().FirstOrDefault(c => c.Card == ability.Card);
    if (cardScript == null)
    {
      return;
    }
    var cardScriptSize = cardScript.GetBackgroundSprite().Texture.GetSize();
    var spawn = new Vector2(cardScript.Position.x + (cardScriptSize.x / 2), cardScript.Position.y);
    var projectileScene = ResourceLoader.Load("res://src/scenes/objects/effects/PrepProjectileExp.tscn") as PackedScene;
    foreach (var targetCard in ability.Targets)
    {
      var selfBuff = ability.Card == targetCard;
      var targetCardScript = GetCardScriptsInScene().FirstOrDefault(c => c.Card == targetCard);
      SpawnProjectiles(spawn, targetCardScript.Position, cardScriptSize, ability.Value, selfBuff, projectileScene, AddExpCallBack, targetCard);
    };
  }

  private void GoldEffectAnimation(PrepAbilityResult ability)
  {
    var cardScript = GetCardScriptsInScene().FirstOrDefault(c => c.Card == ability.Card);
    if (cardScript == null)
    {
      return;
    }
    var cardScriptSize = cardScript.GetBackgroundSprite().Texture.GetSize();
    var spawn = new Vector2(cardScript.Position.x + (cardScriptSize.x / 2), cardScript.Position.y);
    var projectileScene = ResourceLoader.Load("res://src/scenes/objects/effects/PrepProjectileGold.tscn") as PackedScene;
    SpawnProjectiles(spawn, _coinTotalLabel.RectGlobalPosition, _coinTotalLabel.RectSize, ability.Value, false, projectileScene, AddToBankCallback);
  }

  private void RerollEffectAnimation(PrepAbilityResult ability)
  {
    var cardScript = GetCardScriptsInScene().FirstOrDefault(c => c.Card == ability.Card);
    if (cardScript == null)
    {
      return;
    }
    var cardScriptSize = cardScript.GetBackgroundSprite().Texture.GetSize();
    var spawn = new Vector2(cardScript.Position.x + (cardScriptSize.x / 2), cardScript.Position.y);
    var projectileScene = ResourceLoader.Load("res://src/scenes/objects/effects/PrepProjectileReroll.tscn") as PackedScene;
    SpawnProjectiles(spawn, _rerollButton.RectGlobalPosition, _rerollButton.RectSize, 1, false, projectileScene, RerollCallback);
  }

  private void SpawnProjectiles(Vector2 spawn, Vector2 target, Vector2 targetSize, int amount, bool selfBuff,
    PackedScene projectileScene, Action<Card> effectEvent, Card targetCard = null)
  {
    for (int i = 0; i < amount; i++)
    {
      var projectileInstance = (PrepProjectile)projectileScene.Instance();
      projectileInstance.Position = spawn;
      projectileInstance.Target = target;
      projectileInstance.TargetSize = targetSize;
      projectileInstance.SelfBuff = selfBuff;
      projectileInstance.DelayedTakeoffAmount = (i + 2) * 0.1f;
      projectileInstance.EffectEvent = effectEvent;
      projectileInstance.TargetCard = targetCard;
      GetTree().Root.AddChild(projectileInstance);
    }
    _waitingOnCardEffect = true;
  }

  // TODO: try to solve these problems with closures
  private void RerollCallback(Card card)
  {
    _rerollButton.OnHitEffect();
    Reroll();
  }

  private void AddToBankCallback(Card card)
  {
    _newCoinTotal = GameManager.PrepEngine.Bank.AddCoins(1);
    _animationPlayerGold.Stop(true);
    _animationPlayerGold.Play("OnGoldGain");
  }

  private void AddBaseMoveCallBack(Card card)
  {
    card.BaseMove += 1;
    var targetCardScript = GetCardScriptsInScene().FirstOrDefault(c => c.Card == card);
    if (targetCardScript != null)
    {
      targetCardScript.OnBuffAnimate();
    }
    UpdateUiForAllCards();
  }

  private void AddExpCallBack(Card card)
  {
    var leveledUp = card.AddExp(1);
    var targetCardScript = GetCardScriptsInScene().FirstOrDefault(c => c.Card == card);
    if (targetCardScript != null)
    {
      if (leveledUp)
      {
        targetCardScript.OnLevelUp();
      }
      targetCardScript.OnExpGainAnimate();
    }
    UpdateUiForAllCards();
  }
}