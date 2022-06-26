using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class RaceMain : Node2D
{
  private AutoRaceEngine _autoRaceEngine;
  private RaceViewManager _raceViewManager;
  private TextureButton _forwardButton;
  // private TextureButton _backButton;
  private TextureButton _endRaceButton;
  private TextureButton _autoPlayButton;
  private Label _labelTurnPhase;
  // private RichTextLabel _labelGameState;
  // private Label[] _labelCardArray;
  private string _updateTurnPhaseLabel;
  // private string _updatePositionStateLabel;
  // private List<string> _updateCardStateLabel = new List<string>();
  // private List<string> _positionStates = new List<string>();
  // private List<List<string>> _cardStates = new List<List<string>>();
  private List<CardScript> _displayCards;
  private List<Sprite> _slotTurnIndicators;
  private int _currentTurnView;
  private bool _raceOver = false;
  private bool _isPaused = false;
  private float _pauseDuration = 0;
  private bool _waitingOnCardEffect = false;
  private bool _waitingOnMovement = false;
  private bool _autoplay = false;

  public override void _Ready()
  {
    GameManager.LocalPlayer.Position = 0;
    _autoRaceEngine = LoadRaceEngine(GameManager.LocalPlayer, GameManager.NameGenerator);

    var players = _autoRaceEngine.GetPlayers();
    LoadPlayerNames(players);
    _currentTurnView = _autoRaceEngine.GetTurn();

    var tileMapManager = new TileMapManager(
      new[] {
        (GetNode(RaceSceneData.BackgroundTileMap1Path) as BackgroundTileMap),
        (GetNode(RaceSceneData.BackgroundTileMap2Path) as BackgroundTileMap),
        (GetNode(RaceSceneData.BackgroundTileMap3Path) as BackgroundTileMap)
      }
    );
    var characterSoftLeftBound = GetNode<Position2D>(RaceSceneData.CharacterSoftLeftBoundPath).Position;
    var characterSoftRightBound = GetNode<Position2D>(RaceSceneData.CharacterSoftRightBoundPath).Position;
    var characterHardLeftBound = GetNode<Position2D>(RaceSceneData.CharacterHardLeftBoundPath).Position;
    var characterHardRightBound = GetNode<Position2D>(RaceSceneData.CharacterHardRightBoundPath).Position;
    var characters = LoadCharacterSprites(players, characterSoftLeftBound);
    var offscreenIndicatorPairs = LoadOffscreenIndicators(characters);
    _raceViewManager = new RaceViewManager(tileMapManager, characters, offscreenIndicatorPairs,
      characterSoftLeftBound, characterSoftRightBound, characterHardLeftBound, characterHardRightBound);

    _labelTurnPhase = GetNode(RaceSceneData.Label_TurnPhase) as Label;
    // _labelGameState = GetNode(RaceSceneData.RichTextLabel_GameState) as RichTextLabel;

    // var labelCardP1 = GetNode(RaceSceneData.Label_CardP1) as Label;
    // var labelCardP2 = GetNode(RaceSceneData.Label_CardP2) as Label;
    // var labelCardP3 = GetNode(RaceSceneData.Label_CardP3) as Label;
    // var labelCardP4 = GetNode(RaceSceneData.Label_CardP4) as Label;
    // _labelCardArray = new Label[] {
    //   labelCardP1, labelCardP2, labelCardP3, labelCardP4
    // };

    _endRaceButton = GetNode<TextureButton>(RaceSceneData.ButtonFinishPath);
    _endRaceButton.Disabled = true;
    _endRaceButton.Connect("pressed", this, nameof(Button_finish_pressed));
    _forwardButton = GetNode<TextureButton>(RaceSceneData.ButtonForwardPath);
    _forwardButton.Connect("pressed", this, nameof(Button_forward_pressed));
    // _backButton = GetNode<TextureButton>(RaceSceneData.ButtonBackPath);
    // _backButton.Connect("pressed", this, nameof(Button_back_pressed));
    _autoPlayButton = GetNode<TextureButton>(RaceSceneData.ButtonAutoPlay);
    _autoPlayButton.Connect("pressed", this, nameof(Button_autoplay_pressed));

    _slotTurnIndicators = new List<Sprite>{
      GetNode<Sprite>(RaceSceneData.SelectedSlot0),
      GetNode<Sprite>(RaceSceneData.SelectedSlot1),
      GetNode<Sprite>(RaceSceneData.SelectedSlot2),
      GetNode<Sprite>(RaceSceneData.SelectedSlot3)
    };
  }

  public override void _Process(float delta)
  {
    if (_labelTurnPhase != null && !string.IsNullOrWhiteSpace(_updateTurnPhaseLabel))
    {
      _labelTurnPhase.Text = _updateTurnPhaseLabel;
      _updateTurnPhaseLabel = "";
    }

    // if (!string.IsNullOrWhiteSpace(_updatePositionStateLabel))
    // {
    //   _labelGameState.Text = _updatePositionStateLabel;
    //   _updatePositionStateLabel = "";
    // }

    // if (_updateCardStateLabel.Any())
    // {
    //   UpdateCardStates(_updateCardStateLabel);
    //   _updateCardStateLabel.Clear();
    // }

    if (_raceOver && _endRaceButton.Disabled == true)
    {
      _endRaceButton.Disabled = false;
      _autoPlayButton.Disabled = true;
    }

    if (_isPaused)
    {
      _forwardButton.Disabled = true;
      if (_pauseDuration > 0)
      {
        _pauseDuration -= delta;
      }
      else
      {
        _isPaused = false;
        _pauseDuration = 0;
      }
    }

    if (_waitingOnCardEffect)
    {
      _forwardButton.Disabled = true;
      if (GetTree().GetNodesInGroup(RaceSceneData.GroupProjectiles).Count <= 0)
      {
        _waitingOnCardEffect = false;
      }
    }

    if (_waitingOnMovement)
    {
      _forwardButton.Disabled = true;
      if (!_raceViewManager.AreCharactersMoving && !_raceViewManager.IsScrolling)
      {
        _waitingOnMovement = false;
      }
    }

    if (!_raceOver && !_isPaused && !_waitingOnCardEffect && !_waitingOnMovement)
    {
      _forwardButton.Disabled = false;
      if (_autoplay)
      {
        AdvanceRace();
      }
    }
  }

  private AutoRaceEngine LoadRaceEngine(Player player1, NameGenerator nameGenerator)
  {
    var players = new List<Player>();
    players.Add(player1);
    if (GameManager.Opponents != null && GameManager.Opponents.Any())
    {
      players.AddRange(GameManager.Opponents);
    }
    GameManager.Opponents = null;
    GD.Print($"Number of bots: {GameData.NumPlayers - players.Count}");
    if (players.Count < GameData.NumPlayers)
    {
      players.AddRange(GetBots(GameData.NumPlayers - players.Count, nameGenerator));
    }
    return new AutoRaceEngine(players, 5, 5);
  }

  private void LoadPlayerNames(IEnumerable<Player> players)
  {
    var playerLabels = new List<string>{
      RaceSceneData.LabelPlayerName0,
      RaceSceneData.LabelPlayerName1,
      RaceSceneData.LabelPlayerName2,
      RaceSceneData.LabelPlayerName3
    };
    var i = 0;
    foreach (Player player in players)
    {
      if (i >= playerLabels.Count)
      {
        GD.PrintErr($"This race does not support more than {playerLabels.Count} players");
        return;
      }
      var playerNameLabel = GetNode<Label>(playerLabels[i]);
      playerNameLabel.Text = player.Name ?? "";
      i = i + 1;
    }
  }

  private IEnumerable<CharacterScript> LoadCharacterSprites(IEnumerable<Player> players, Vector2 topSpawnPosition)
  {
    var characters = new List<CharacterScript>();
    foreach (var player in players.OrderBy(p => p.Id))
    {
      var characterScene = ResourceLoader.Load(RaceSceneData.CharacterScenePath) as PackedScene;
      var characterInstance = (CharacterScript)characterScene.Instance();
      characterInstance.CharacterSkin = player.Skin;
      characterInstance.Position = new Vector2(topSpawnPosition.x, topSpawnPosition.y + (RaceSceneData.CharacterSpawnYOffset * player.Id));
      characterInstance.AnimationState = AnimationStates.running;
      characterInstance.Id = player.Id;
      characters.Add(characterInstance);
      AddChild(characterInstance);
    }
    return characters;
  }

  private IEnumerable<(OffscreenIndicatorScript, OffscreenIndicatorScript)> LoadOffscreenIndicators(IEnumerable<CharacterScript> characters)
  {
    var offscreenIndicatorPairs = new List<(OffscreenIndicatorScript, OffscreenIndicatorScript)> {
      (GetNode<OffscreenIndicatorScript>(RaceSceneData.OffscreenIndicatorL1),GetNode<OffscreenIndicatorScript>(RaceSceneData.OffscreenIndicatorR1)),
      (GetNode<OffscreenIndicatorScript>(RaceSceneData.OffscreenIndicatorL2),GetNode<OffscreenIndicatorScript>(RaceSceneData.OffscreenIndicatorR2)),
      (GetNode<OffscreenIndicatorScript>(RaceSceneData.OffscreenIndicatorL3),GetNode<OffscreenIndicatorScript>(RaceSceneData.OffscreenIndicatorR3))
    };

    var i = 1;
    foreach (var indicatorPair in offscreenIndicatorPairs)
    {
      indicatorPair.Item1.Id = i;
      indicatorPair.Item2.Id = i;
      indicatorPair.Item1.CharacterRef = characters.FirstOrDefault(c => c.Id == i);
      indicatorPair.Item2.CharacterRef = characters.FirstOrDefault(c => c.Id == i);
      i = i + 1;
    }

    return offscreenIndicatorPairs;
  }

  private void LoadCardScript(Card card, int playerId)
  {
    var existingDisplayCard = _displayCards?.FirstOrDefault(c => c.Slot == playerId);
    if (existingDisplayCard != null)
    {
      existingDisplayCard.QueueFree();
      _displayCards.Remove(existingDisplayCard);
    }
    HideSelectedCardData(playerId);
    var cardScene = ResourceLoader.Load(PrepSceneData.CardScenePath) as PackedScene;
    var cardInstance = (CardScript)cardScene.Instance();
    var containerPosition = GetNode<Sprite>($"CardSlots/slot_{playerId}").Position;
    var position = containerPosition + PrepSceneData.CardSlotOffset;
    cardInstance.Card = card;
    cardInstance.Slot = playerId;
    cardInstance.Position = position;
    cardInstance.DisplayOnly = true;
    cardInstance.Connect(nameof(CardScript.cardDisplaySelected), this, nameof(_on_Card_display_selected));
    if (_displayCards == null)
    {
      _displayCards = new List<CardScript>();
    }
    _displayCards.Add(cardInstance);
    AddChild(cardInstance);
  }

  private void _on_Card_display_selected(CardScript cardScript)
  {
    ToggleDisplaySelectedCardData(cardScript.Card, cardScript.Slot);
  }

  private void Button_finish_pressed()
  {
    GD.Print("Finish button pressed");
    _autoRaceEngine.Clear();
    _autoRaceEngine = null;
    var result = GetTree().ChangeScene("res://src/scenes/game/RaceEnd.tscn");
    if (result != Error.Ok)
    {
      GD.Print($"Error changing scenes with error code: {(Error)result}");
    }
  }

  // TODO Big Refactor Needed Here
  private void Button_forward_pressed()
  {
    GD.Print("Forward button pressed");
    AdvanceRace();
  }

  // private void Button_back_pressed()
  // {
  //   GD.Print("Back button pressed");
  //   _currentTurnView = _currentTurnView - 1;
  //   if (_currentTurnView < 2)
  //   {
  //     _currentTurnView = 1;
  //     _backButton.Disabled = true;
  //   }
  //   _forwardButton.Disabled = false;
  //   _updateTurnPhaseLabel = $"Viewing T{_currentTurnView}";
  //   GD.Print($"current turn after back press: {_currentTurnView}");
  //   // GD.Print($"position states after back press: {_positionStates.Count()}");
  //   // _updatePositionStateLabel = _positionStates[_currentTurnView - 1];
  //   if (_cardStates.Count > _currentTurnView - 1)
  //   {
  //     _updateCardStateLabel.AddRange(_cardStates[_currentTurnView - 1]);
  //   }
  // }

  // private void UpdateCardStates(List<string> states)
  // {
  //   var p = 0;
  //   foreach (var state in states)
  //   {
  //     GD.Print($"Updating Card States: {state}");
  //     _labelCardArray[p].Text = state;
  //     p = p + 1;
  //   }
  // }

  private void Button_autoplay_pressed()
  {
    _autoplay = !_autoplay;
  }

  private void AdvanceRace()
  {
    if (_autoRaceEngine.GetTurn() != _currentTurnView)
    {
      _currentTurnView = _currentTurnView + 1;
      GD.Print($"history turn view: {_currentTurnView}");
      // GD.Print($"position states after forward history press: {_positionStates.Count()}");
      _updateTurnPhaseLabel = $"Viewing T{_currentTurnView}";
      // _updatePositionStateLabel = _positionStates[_currentTurnView - 1];
      // if (_cardStates.Count > _currentTurnView - 1)
      // {
      //   _updateCardStateLabel.AddRange(_cardStates[_currentTurnView - 1]);
      // }
      // _backButton.Disabled = false;
      return;
    }

    if (_raceOver && _autoRaceEngine.GetTurn() == _currentTurnView)
    {
      _forwardButton.Disabled = true;
      return;
    }

    var didWin = _autoRaceEngine.AdvanceRace();
    var turnPhase = _autoRaceEngine.GetTurnPhase();
    if (turnPhase == TurnPhases.Start)
    {
      didWin = _autoRaceEngine.AdvanceRace();
      turnPhase = _autoRaceEngine.GetTurnPhase();
    }

    _currentTurnView = _autoRaceEngine.GetTurn();
    GD.Print($"current turn: {_currentTurnView}");

    if (turnPhase == TurnPhases.Abilities1)
    {
      _updateTurnPhaseLabel = $"T{_currentTurnView}: Abilities Phase 1";
      var turnResults = _autoRaceEngine.GetTurnResults();
      // var currentCardStates = new List<string>();
      var p = 0;
      foreach (var turnResult in turnResults)
      {
        var card = turnResult.Player.Cards[_currentTurnView - 1];
        LoadCardScript(card, p);
        // var cardState = $"{card.GetName()}\n{card.BaseMove}\n{card.GetDescription()}";
        // _labelCardArray[p].Text = cardState;
        // currentCardStates.Add(cardState);
        p = p + 1;
      }
      // _cardStates.Add(currentCardStates);
    }

    if (turnPhase == TurnPhases.AbilitiesP0 || turnPhase == TurnPhases.AbilitiesP1 || turnPhase == TurnPhases.AbilitiesP2 || turnPhase == TurnPhases.AbilitiesP3
      || turnPhase == TurnPhases.Abilities2P0 || turnPhase == TurnPhases.Abilities2P1 || turnPhase == TurnPhases.Abilities2P2 || turnPhase == TurnPhases.Abilities2P3)
    {
      HandleAbilitiesPhase(turnPhase);
    }

    if (turnPhase == TurnPhases.Abilities2)
    {
      HideSlotTurnIndicators();
      _updateTurnPhaseLabel = $"T{_currentTurnView}: Abilities Phase 2";
    }

    if (turnPhase == TurnPhases.Move || turnPhase == TurnPhases.HandleRemainingTokens)
    {
      var phase = turnPhase == TurnPhases.Move ? "Move" : "Handle Remaining Tokens";
      _updateTurnPhaseLabel = $"T{_currentTurnView}: {phase}";
      HideSlotTurnIndicators();
      var turnResults = _autoRaceEngine.GetTurnResults();
      _autoRaceEngine.PrintPositions();
      _raceViewManager.UpdateTokenCounts(turnResults);
      if (turnResults != null && turnResults.ToList().Count > 0)
      {
        _waitingOnMovement = true;
        _raceViewManager.MovePlayers(turnResults);
        // var positionState = EngineTesting.GetPositionTextView(turnResults);
        // _updatePositionStateLabel = positionState;
        // _positionStates.Add(positionState);
      }
    }

    if (turnPhase == TurnPhases.Move)
    {
      // didWin = _autoRaceEngine.AdvanceRace();
      didWin = _autoRaceEngine.AdvanceRace();
      turnPhase = _autoRaceEngine.GetTurnPhase();
    }
    // if (turnPhase == TurnPhases.End || turnPhase == TurnPhases.HandleRemainingTokens)
    // {
    //   _backButton.Disabled = false;
    // }
    // else
    // {
    //   _backButton.Disabled = true;
    // }

    if (didWin)
    {
      _raceViewManager?.RaceEndAnimation();
      _forwardButton.Disabled = true;
      _raceOver = true;
      CalculateStandings();
      return;
    }
  }

  private void HandleAbilitiesPhase(TurnPhases turnPhase)
  {
    // _updateTurnPhaseLabel = $"T{_currentTurnView}: {turnPhase}";
    var turnResults = _autoRaceEngine.GetTurnResults();
    var turnId = int.Parse(turnPhase.ToString().Last().ToString());
    var abilityPhase = turnPhase.ToString().StartsWith("Abilities2") ? TurnPhases.Abilities2 : TurnPhases.Abilities1;
    var turnResult = turnResults.Where(r => r.Phase == abilityPhase).FirstOrDefault(r => r.Player.Id == turnId);
    if (turnResult.TokensGiven != null && turnResult.TokensGiven.Any())
    {
      ShowSlotTurnIndicator(turnId, true);
      _raceViewManager.GiveTokens(turnResult);
      _waitingOnCardEffect = true;
    }
    else
    {
      ShowSlotTurnIndicator(turnId, false);
      Pause(0.3f);
    }
  }

  private void CalculateStandings()
  {
    var standings = _autoRaceEngine.GetStandings();
    var results = new List<PlayerResult>();
    foreach (var key in standings.Keys)
    {
      foreach (var player in standings[key])
      {
        results.Add(new PlayerResult { Player = player, Position = player.Position, Place = key });
        GD.Print($"Player {player.Id} finished {key.IntToPlaceStr()}");
        if (GameManager.LocalPlayer.Id == player.Id)
        {
          GameManager.Score.AddResult(key);
        }
      }
    }
    GameManager.RaceHistory.AddResult(GameManager.CurrentRace, results);
  }

  private void ToggleDisplaySelectedCardData(Card card, int playerId)
  {
    var selectedCardPath = RaceSceneData.ContainerSelectedCard + playerId.ToString();
    var selectedCardPanel = GetNode<Node2D>(selectedCardPath);
    if (selectedCardPanel.Visible)
    {
      HideSelectedCardData(playerId);
    }
    else
    {
      DisplaySelectedCardData(card, playerId);
    }
  }

  private void DisplaySelectedCardData(Card card, int playerId)
  {
    var selectedCardPath = RaceSceneData.ContainerSelectedCard + playerId.ToString();
    var selectedCardPanel = GetNode<Node2D>(selectedCardPath);
    var selectedCardNameLabel = GetNode<Label>(selectedCardPath + RaceSceneData.LabelSelectedNameRelPath);
    var selectedCardDescriptionLabel = GetNode<Label>(selectedCardPath + RaceSceneData.LabelSelectedDescriptionRelPath);
    var selectedCardPhaseLabel = GetNode<Label>(selectedCardPath + RaceSceneData.LabelSelectedPhaseRelPath);
    var selectedCardTierLabel = GetNode<Label>(selectedCardPath + RaceSceneData.LabelSelectedTierRelPath);
    selectedCardPanel.Visible = true;
    selectedCardNameLabel.Text = card.GetName();
    selectedCardDescriptionLabel.Text = card.GetDescription();
    selectedCardPhaseLabel.Text = card.GetAbilityPhase();
    selectedCardTierLabel.Text = $"Tier {card.Tier}";
  }

  private void HideSelectedCardData(int playerId)
  {
    var selectedCardPath = RaceSceneData.ContainerSelectedCard + playerId.ToString();
    var selectedCardPanel = GetNode<Node2D>(selectedCardPath);
    var selectedCardNameLabel = GetNode<Label>(selectedCardPath + RaceSceneData.LabelSelectedNameRelPath);
    var selectedCardDescriptionLabel = GetNode<Label>(selectedCardPath + RaceSceneData.LabelSelectedDescriptionRelPath);
    selectedCardPanel.Visible = false;
    selectedCardNameLabel.Text = "";
    selectedCardDescriptionLabel.Text = "";
  }

  private void ShowSlotTurnIndicator(int turnId, bool abilityActivated)
  {
    _slotTurnIndicators[turnId].Visible = true;
    var transparentValue = abilityActivated ? 1f : 0.5f;
    _slotTurnIndicators[turnId].Modulate = new Color(1, 0.16f, 0.24f, transparentValue);
  }

  private void HideSlotTurnIndicators()
  {
    foreach (var indicator in _slotTurnIndicators)
    {
      indicator.Visible = false;
    }
  }

  private void Pause(float duration)
  {
    _isPaused = true;
    _pauseDuration = duration;
  }

  // TODO refactor out to a matchmaker
  private List<Player> GetBots(int numBots, NameGenerator nameGenerator)
  {
    var bots = new List<Player>();
    for (var i = 0; i < numBots; i++)
    {
      var id = GameData.NumPlayers - numBots + i;
      bots.Add(GetBot(id, nameGenerator.GetRandomName()));
    }
    return bots;
  }

  private Player GetBot(int id, string name)
  {
    return new BotBasic(id, GameManager.CurrentRace, GameManager.PrepEngine.ShopService, name);
  }
}