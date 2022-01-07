using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static AutoRaceEngine;

public class RaceMain : Node2D
{
  private AutoRaceEngine _autoRaceEngine;
  private Button _forwardButton;
  private Button _backButton;
  private Label _labelTurnPhase;
  private RichTextLabel _labelGameState;
  private Label[] _labelCardArray;
  private string _updateTurnPhaseLabel;
  private string _updatePositionStateLabel;
  private List<string> _updateCardStateLabel = new List<string>();
  private List<string> _positionStates = new List<string>();
  private List<List<string>> _cardStates = new List<List<string>>();
  private int _currentTurnView;
  private bool _raceOver = false;

  public override void _Ready()
  {
    _labelTurnPhase = GetNode(RaceSceneData.Label_TurnPhase) as Label;
    _labelGameState = GetNode(RaceSceneData.RichTextLabel_GameState) as RichTextLabel;

    var labelCardP1 = GetNode(RaceSceneData.Label_CardP1) as Label;
    var labelCardP2 = GetNode(RaceSceneData.Label_CardP2) as Label;
    var labelCardP3 = GetNode(RaceSceneData.Label_CardP3) as Label;
    var labelCardP4 = GetNode(RaceSceneData.Label_CardP4) as Label;
    _labelCardArray = new Label[] {
      labelCardP1, labelCardP2, labelCardP3, labelCardP4
    };


    var goButton = GetNode(RaceSceneData.ButtonFinishPath) as Button;
    goButton.Connect("pressed", this, nameof(Button_finish_pressed));
    _forwardButton = GetNode(RaceSceneData.ButtonForwardPath) as Button;
    _forwardButton.Connect("pressed", this, nameof(Button_forward_pressed));
    _backButton = GetNode(RaceSceneData.ButtonBackPath) as Button;
    _backButton.Connect("pressed", this, nameof(Button_back_pressed));

    _autoRaceEngine = EngineTesting.RaceEngine(GameManager.Player1);
    _currentTurnView = _autoRaceEngine.GetTurn();
  }

  public override void _Process(float delta)
  {
    if (!string.IsNullOrWhiteSpace(_updateTurnPhaseLabel))
    {
      _labelTurnPhase.Text = _updateTurnPhaseLabel;
      _updateTurnPhaseLabel = "";
    }

    if (!string.IsNullOrWhiteSpace(_updatePositionStateLabel))
    {
      _labelGameState.Text = _updatePositionStateLabel;
      _updatePositionStateLabel = "";
    }

    if (_updateCardStateLabel.Any())
    {
      UpdateCardStates(_updateCardStateLabel);
      _updateCardStateLabel.Clear();
    }
  }

  private void Button_finish_pressed()
  {
    GD.Print("Finish button pressed");
    GetTree().ChangeScene("res://src/scenes/game/Prep.tscn");
  }

  private void Button_forward_pressed()
  {
    GD.Print("Forward button pressed");
    if (_autoRaceEngine.GetTurn() != _currentTurnView)
    {
      _currentTurnView = _currentTurnView + 1;
      GD.Print($"history turn view: {_currentTurnView}");
      GD.Print($"position states after forward history press: {_positionStates.Count()}");
      _updateTurnPhaseLabel = $"Viewing T{_currentTurnView}";
      _updatePositionStateLabel = _positionStates[_currentTurnView - 1];
      _updateCardStateLabel.AddRange(_cardStates[_currentTurnView - 1]);
      _backButton.Disabled = false;
      return;
    }

    if (_raceOver && _autoRaceEngine.GetTurn() == _currentTurnView)
    {
      _forwardButton.Disabled = true;
      return;
    }

    var didWin = _autoRaceEngine.AdvanceRace();
    _currentTurnView = _autoRaceEngine.GetTurn();
    GD.Print($"current turn: {_currentTurnView}");
    var turnPhase = _autoRaceEngine.GetTurnPhase();
    _updateTurnPhaseLabel = $"T{_currentTurnView}: {turnPhase.ToString()}";
    if (turnPhase == TurnPhases.Abilities1)
    {
      var turnResults = _autoRaceEngine.GetTurnResults();
      var currentCardStates = new List<string>();
      var p = 0;
      foreach (var turnResult in turnResults)
      {
        var card = turnResult.Player.Cards[_currentTurnView - 1];
        var cardState = $"{card.GetName()}\n{card.BaseMove}\n{card.GetDescription()}";
        _labelCardArray[p].Text = cardState;
        currentCardStates.Add(cardState);
        p = p + 1;
      }
      _cardStates.Add(currentCardStates);
    }
    if (turnPhase == TurnPhases.Move)
    {
      var turnResults = _autoRaceEngine.GetTurnResults();
      var positionState = EngineTesting.GetPositionTextView(turnResults);
      _updatePositionStateLabel = positionState;
      _positionStates.Add(positionState);
    }
    if (turnPhase == TurnPhases.End)
    {
      _backButton.Disabled = false;
    }
    else
    {
      _backButton.Disabled = true;
    }

    if (didWin)
    {
      _forwardButton.Disabled = true;
      _raceOver = true;
      return;
    }
  }

  private void Button_back_pressed()
  {
    GD.Print("Back button pressed");
    _currentTurnView = _currentTurnView - 1;
    if (_currentTurnView < 2)
    {
      _currentTurnView = 1;
      _backButton.Disabled = true;
    }
    _forwardButton.Disabled = false;
    _updateTurnPhaseLabel = $"Viewing T{_currentTurnView}";
    GD.Print($"current turn after back press: {_currentTurnView}");
    GD.Print($"position states after back press: {_positionStates.Count()}");
    _updatePositionStateLabel = _positionStates[_currentTurnView - 1];
    _updateCardStateLabel.AddRange(_cardStates[_currentTurnView - 1]);
  }

  private void UpdateCardStates(List<string> states)
  {
    var p = 0;
    foreach (var state in states)
    {
      GD.Print($"Updating Card States: {state}");
      _labelCardArray[p].Text = state;
      p = p + 1;
    }
  }
}