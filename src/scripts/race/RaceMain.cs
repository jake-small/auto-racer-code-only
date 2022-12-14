using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class RaceMain : Node2D
{
  private AutoRaceEngine _autoRaceEngine;
  private RaceViewManager _raceViewManager;
  private TextureButton _forwardButton;
  private TextureButton _endRaceButton;
  private TextureButton _autoPlayButton;
  private Material _autoPlayButtonPulseMaterial;
  private Label _labelTurnPhase;
  private string _updateTurnPhaseLabel;
  private IEnumerable<(OffscreenIndicatorScript, OffscreenIndicatorScript)> _offscreenIndicatorPairs;
  private List<CardScript> _displayCards;
  private List<CharacterScript> _characterUiDisplays;
  private List<Sprite> _slotTurnIndicators;
  private List<Sprite> _phaseAbilityIndicators;
  private List<Sprite> _phaseMoveIndicators;
  private Sprite _phaseRemainingTokensIndicator;
  private int _currentTurn;
  private bool _raceOver = false;
  private bool _isPaused = false;
  private float _pauseDuration = 0;
  private bool _waitingOnCardEffect = false;
  private bool _waitingOnMovement = false;
  private bool _autoplay = false;
  private int _abilityPhasePlayerId = -1;

  public override void _Ready()
  {
    GameManager.LocalPlayer.Position = 0;
    _autoRaceEngine = LoadRaceEngine(GameManager.LocalPlayer, GameManager.NameGenerator);

    var players = _autoRaceEngine.GetPlayers();
    LoadPlayerNames(players);
    _currentTurn = _autoRaceEngine.GetTurn();

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
    _offscreenIndicatorPairs = LoadOffscreenIndicators(characters);
    _raceViewManager = new RaceViewManager(tileMapManager, characters, _offscreenIndicatorPairs,
      characterSoftLeftBound, characterSoftRightBound, characterHardLeftBound, characterHardRightBound);

    _labelTurnPhase = GetNode(RaceSceneData.Label_TurnPhase) as Label;

    _endRaceButton = GetNode<TextureButton>(RaceSceneData.ButtonFinishPath);
    _endRaceButton.Disabled = true;
    _endRaceButton.Connect("pressed", this, nameof(Button_finish_pressed));
    _forwardButton = GetNode<TextureButton>(RaceSceneData.ButtonForwardPath);
    _forwardButton.Connect("pressed", this, nameof(Button_forward_pressed));
    _autoPlayButton = GetNode<TextureButton>(RaceSceneData.ButtonAutoPlay);
    _autoPlayButtonPulseMaterial = _autoPlayButton.Material;
    _autoPlayButton.Connect("pressed", this, nameof(Button_autoplay_pressed));

    _slotTurnIndicators = new List<Sprite>{
      GetNode<Sprite>(RaceSceneData.SelectedSlot0),
      GetNode<Sprite>(RaceSceneData.SelectedSlot1),
      GetNode<Sprite>(RaceSceneData.SelectedSlot2),
      GetNode<Sprite>(RaceSceneData.SelectedSlot3)
    };

    _phaseAbilityIndicators = new List<Sprite>{
      GetNode<Sprite>(RaceSceneData.PhaseIndicatorAbilitiesPrefix + "1"),
      GetNode<Sprite>(RaceSceneData.PhaseIndicatorAbilitiesPrefix + "2"),
      GetNode<Sprite>(RaceSceneData.PhaseIndicatorAbilitiesPrefix + "3"),
      GetNode<Sprite>(RaceSceneData.PhaseIndicatorAbilitiesPrefix + "4"),
      GetNode<Sprite>(RaceSceneData.PhaseIndicatorAbilitiesPrefix + "5"),
    };
    _phaseMoveIndicators = new List<Sprite>{
      GetNode<Sprite>(RaceSceneData.PhaseIndicatorMovePrefix + "1"),
      GetNode<Sprite>(RaceSceneData.PhaseIndicatorMovePrefix + "2"),
      GetNode<Sprite>(RaceSceneData.PhaseIndicatorMovePrefix + "3"),
      GetNode<Sprite>(RaceSceneData.PhaseIndicatorMovePrefix + "4"),
      GetNode<Sprite>(RaceSceneData.PhaseIndicatorMovePrefix + "5"),
    };
    _phaseRemainingTokensIndicator = GetNode<Sprite>(RaceSceneData.PhaseIndicatorRemainingTokens);
  }

  public override void _Process(float delta)
  {
    if (_labelTurnPhase != null && !string.IsNullOrWhiteSpace(_updateTurnPhaseLabel))
    {
      _labelTurnPhase.Text = _updateTurnPhaseLabel;
      _updateTurnPhaseLabel = "";
    }

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
        Pause(0.5f);
      }
    }

    if (_waitingOnMovement)
    {
      _forwardButton.Disabled = true;
      if (!_raceViewManager.AreCharactersMoving && !_raceViewManager.IsScrolling)
      {
        _waitingOnMovement = false;
        Pause(0.5f);
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
      var expectedNumOpponents = GameManager.NumPlayers - 1;
      if (GameManager.Opponents.Count() > expectedNumOpponents)
      {
        GD.Print($"Too many opponents given, expected {expectedNumOpponents} got {GameManager.Opponents.Count()}");
        GameManager.Opponents = GameManager.Opponents.Take(expectedNumOpponents);
      }
      players.AddRange(GameManager.Opponents);
    }
    GameManager.Opponents = null;
    GD.Print($"Number of bots: {GameManager.NumPlayers - players.Count}");
    if (players.Count < GameManager.NumPlayers)
    {
      players.AddRange(GetBots(GameManager.NumPlayers - players.Count, nameGenerator));
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
    _characterUiDisplays = new List<CharacterScript>();
    foreach (var player in players.OrderBy(p => p.Id))
    {
      var characterScene = ResourceLoader.Load(RaceSceneData.CharacterScenePath) as PackedScene;
      var characterInstance = (CharacterScript)characterScene.Instance();
      characterInstance.CharacterSkin = player.Skin;
      characterInstance.Position = new Vector2(
        topSpawnPosition.x + (RaceSceneData.SpaceWidth * player.Position),
        topSpawnPosition.y + (RaceSceneData.CharacterSpawnYOffset * player.Id)
      );
      characterInstance.AnimationState = AnimationStates.running;
      characterInstance.Id = player.Id;
      characters.Add(characterInstance);

      var cardSlot = GetNode<Sprite>($"CardSlots/slot_{player.Id}");
      if (GameManager.NumPlayers == 2)
      {
        cardSlot.Position = new Vector2(cardSlot.Position.x + 384, cardSlot.Position.y);
        var selectedCardPanel0 = GetNode<Node2D>(RaceSceneData.ContainerSelectedCard + 0);
        selectedCardPanel0.Position = new Vector2(selectedCardPanel0.Position.x + 192, selectedCardPanel0.Position.y);
        var selectedCardPanel1 = GetNode<Node2D>(RaceSceneData.ContainerSelectedCard + 1);
        selectedCardPanel1.Position = new Vector2(selectedCardPanel1.Position.x + 192, selectedCardPanel1.Position.y);
      }
      var characterUiInstance = (CharacterScript)characterScene.Instance();
      characterUiInstance.CharacterSkin = player.Skin;
      characterUiInstance.Position = new Vector2(cardSlot.Position.x - 64, cardSlot.Position.y + 64);
      characterUiInstance.AnimationState = AnimationStates.facing_front;
      characterUiInstance.Id = player.Id;
      _characterUiDisplays.Add(characterUiInstance);

      characterInstance.Connect(nameof(CharacterScript.onHit), this, nameof(CharacterOnHit));
      characterInstance.Connect(nameof(CharacterScript.onBuff), this, nameof(CharacterOnBuff));

      AddChild(characterInstance);
      AddChild(characterUiInstance);
    }

    if (GameManager.NumPlayers == 2)
    {
      GetNode<Sprite>("CardSlots/slot_2").Visible = false;
      GetNode<Sprite>("CardSlots/slot_3").Visible = false;
      characters[0].Position = new Vector2(characters[0].Position.x, characters[0].Position.y + 50);
      characters[1].Position = new Vector2(characters[1].Position.x, characters[1].Position.y + 150);
    }
    return characters;
  }

  // Offscreen indicators for OPPONENTS
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
      if (i > GameManager.NumPlayers - 1)
      {
        break;
      }
      indicatorPair.Item1.Id = i;
      indicatorPair.Item2.Id = i;
      indicatorPair.Item1.CharacterRef = characters.FirstOrDefault(c => c.Id == i);
      indicatorPair.Item2.CharacterRef = characters.FirstOrDefault(c => c.Id == i);

      if (GameManager.NumPlayers == 2)
      {
        var indicatorLeft = indicatorPair.Item1;
        var indicatorRight = indicatorPair.Item2;
        indicatorLeft.Position = new Vector2(indicatorLeft.Position.x, indicatorLeft.Position.y + 150);
        indicatorRight.Position = new Vector2(indicatorRight.Position.x, indicatorRight.Position.y + 150);
      }

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
    if (playerId != 0)
    {
      cardInstance.HideExp = true;
    }
    cardInstance.Connect(nameof(CardScript.cardDisplaySelected), this, nameof(_on_Card_display_selected));
    if (_displayCards == null)
    {
      _displayCards = new List<CardScript>();
    }
    _displayCards.Add(cardInstance);
    AddChild(cardInstance);
  }

  private void HideCardScripts()
  {
    var i = 0;
    foreach (var existingDisplayCard in _displayCards)
    {
      if (existingDisplayCard != null)
      {
        existingDisplayCard.QueueFree();
      }
      HideSelectedCardData(i);
      i = i + 1;
    }
    _displayCards.Clear();
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

  private void Button_forward_pressed()
  {
    GD.Print("Forward button pressed");
    AdvanceRace();
  }

  private void Button_autoplay_pressed()
  {
    _autoplay = !_autoplay;
    if (_autoplay)
    {
      _autoPlayButton.Material = null;
    }
    else
    {
      _autoPlayButton.Material = _autoPlayButtonPulseMaterial;
    }
  }

  // TODO Big Refactor Needed Here
  private void AdvanceRace()
  {
    if (_autoRaceEngine.GetTurn() != _currentTurn)
    {
      _currentTurn = _currentTurn + 1;
      GD.Print($"history turn view: {_currentTurn}");
      _updateTurnPhaseLabel = $"Viewing T{_currentTurn}";
      return;
    }

    if (_raceOver && _autoRaceEngine.GetTurn() == _currentTurn)
    {
      _forwardButton.Disabled = true;
      return;
    }

    // If we start and are in an ability phase, handle the remaining players
    var turnPhase = _autoRaceEngine.GetTurnPhase();
    if (_abilityPhasePlayerId != -1
      && (turnPhase == TurnPhases.Abilities1 || turnPhase == TurnPhases.Abilities3 || turnPhase == TurnPhases.Abilities4
          || turnPhase == TurnPhases.Abilities5))
    {
      HandleAbilitiesPhase(turnPhase);
      return;
    }

    var didWin = _autoRaceEngine.AdvanceRace();
    turnPhase = _autoRaceEngine.GetTurnPhase();
    _abilityPhasePlayerId = -1;

    _currentTurn = _autoRaceEngine.GetTurn();
    GD.Print($"current turn: {_currentTurn}");

    if (turnPhase == TurnPhases.Abilities1)
    {
      TogglePhaseIndicator(_currentTurn, turnPhase);
      _updateTurnPhaseLabel = $"T{_currentTurn}: Abilities Phase 1";
      var turnResults = _autoRaceEngine.GetTurnResults();
      var p = 0;
      foreach (var turnResult in turnResults)
      {
        var card = turnResult.Player.Cards[_currentTurn - 1];
        LoadCardScript(card, p);
        p = p + 1;
      }
      HideSlotTurnIndicators();
      HighlightUpcomingAbilities();
      HandleAbilitiesPhase(turnPhase);
    }
    else if (turnPhase == TurnPhases.Abilities2 || turnPhase == TurnPhases.Abilities3 || turnPhase == TurnPhases.Abilities4 || turnPhase == TurnPhases.Abilities5)
    {
      _updateTurnPhaseLabel = $"T{_currentTurn}: Abilities Phase {(int)turnPhase}";
      HandleAbilitiesPhase(turnPhase);
    }

    if (turnPhase == TurnPhases.Move || turnPhase == TurnPhases.HandleRemainingTokens)
    {
      TogglePhaseIndicator(_currentTurn, turnPhase);
      var phase = turnPhase == TurnPhases.Move ? "Move" : "Handle Remaining Tokens";
      _updateTurnPhaseLabel = $"T{_currentTurn}: {phase}";
      HideSlotTurnIndicators();
      var turnResults = _autoRaceEngine.GetTurnResults();
      _autoRaceEngine.PrintPositions();
      _raceViewManager.UpdateTokenCounts(turnResults);
      if (turnResults != null && turnResults.ToList().Count > 0)
      {
        _waitingOnMovement = true;
        _raceViewManager.MovePlayers(turnResults);
      }
      if (turnPhase == TurnPhases.HandleRemainingTokens)
      {
        HideCardScripts();
      }
    }

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
    if (_abilityPhasePlayerId == -1)
    {
      _abilityPhasePlayerId = 0;
    }
    var turnResult = _autoRaceEngine.GetTurnResults().Where(t => t.Phase == turnPhase).FirstOrDefault(t => t.Player.Id == _abilityPhasePlayerId);
    if (turnResult != null && turnResult.TokensGiven != null && turnResult.TokensGiven.Any())
    {
      ShowSlotTurnIndicator(turnResult.Player.Id, true);
      _raceViewManager.GiveTokens(turnResult);
      _waitingOnCardEffect = true;
    }
    // else
    // {
    //   ShowSlotTurnIndicator(turnResult.Player.Id, false);
    //   // Pause(0.3f);
    // }
    _abilityPhasePlayerId = _abilityPhasePlayerId + 1;
    if (_abilityPhasePlayerId > GameManager.NumPlayers - 1)
    {
      _abilityPhasePlayerId = -1;
      if (turnPhase == TurnPhases.Abilities5)
      {
        Pause(0.5f);
      }
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
    var selectedCardInfo = GetNode<CardInfoScript>(selectedCardPath);
    selectedCardInfo.SetCard(card);
  }

  private void HideSelectedCardData(int playerId)
  {
    var selectedCardPath = RaceSceneData.ContainerSelectedCard + playerId.ToString();
    var selectedCardInfo = GetNode<CardInfoScript>(selectedCardPath);
    selectedCardInfo.Clear();
  }

  private void HighlightUpcomingAbilities()
  {
    var triggeredAbilitySlots = _autoRaceEngine.GetTriggeredAbilitySlots();
    foreach (var slot in triggeredAbilitySlots)
    {
      ShowSlotTurnIndicator(slot, false);
    }
  }

  private void ShowSlotTurnIndicator(int turnId, bool isOpaque)
  {
    _slotTurnIndicators[turnId].Visible = true;
    var transparentValue = isOpaque ? 1f : 0.5f;
    _slotTurnIndicators[turnId].Modulate = new Color(212f / 255f, 26f / 255f, 48f / 255f, transparentValue);
  }

  private void HideSlotTurnIndicators()
  {
    foreach (var indicator in _slotTurnIndicators)
    {
      indicator.Visible = false;
    }
  }

  private void TogglePhaseIndicator(int turn, TurnPhases turnPhase)
  {
    var _transparentValue = 0.4f;
    for (int i = 0; i < _phaseAbilityIndicators.Count; i++)
    {
      _phaseAbilityIndicators[i].Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, _transparentValue);
      _phaseMoveIndicators[i].Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, _transparentValue);
    }

    if (turnPhase == TurnPhases.Abilities1 || turnPhase == TurnPhases.Abilities2 || turnPhase == TurnPhases.Abilities3
      || turnPhase == TurnPhases.Abilities4 || turnPhase == TurnPhases.Abilities5)
    {
      _phaseAbilityIndicators[turn - 1].Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 1f);
    }
    else if (turnPhase == TurnPhases.Move)
    {
      _phaseMoveIndicators[turn - 1].Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 1f);
    }
    else if (turnPhase == TurnPhases.HandleRemainingTokens)
    {
      _phaseRemainingTokensIndicator.Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 1f);
    }
  }

  private void CharacterOnBuff(CharacterScript character)
  {
    var characterUiDisplay = _characterUiDisplays.FirstOrDefault(c => c.Id == character.Id);
    characterUiDisplay.OnBuffAnimate();
  }

  private void CharacterOnHit(CharacterScript character)
  {
    var characterUiDisplay = _characterUiDisplays.FirstOrDefault(c => c.Id == character.Id);
    characterUiDisplay.OnHitAnimate();
  }

  private void Pause(float duration)
  {
    _isPaused = true;
    _pauseDuration = duration;
  }

  private List<Player> GetBots(int numBots, NameGenerator nameGenerator)
  {
    // Used to debug movement speed
    // var debugStartingPositions = new List<int> { 0, -20, 30 };
    // var debugStartingMoveTokens = new List<int> { 1000, 20, -25 };
    var bots = new List<Player>();
    for (var i = 0; i < numBots; i++)
    {
      var id = GameManager.NumPlayers - numBots + i;
      var bot = GetBot(id, nameGenerator.GetRandomName());
      // Used to debug movement speed
      // bot.Position = debugStartingPositions[i];
      // var testToken = new MoveToken
      // {
      //   CreatedBy = bot.Id,
      //   Duration = 1,
      //   Type = MoveTokenType.Additive,
      //   Target = i + 1,
      //   Value = debugStartingMoveTokens[i]
      // };
      // bot.Tokens.Add(testToken);
      bots.Add(bot);
    }
    return bots;
  }

  private Player GetBot(int id, string name)
  {
    return new BotBasic(id, GameManager.CurrentRace, GameManager.PrepEngine.ShopService, name);
  }
}