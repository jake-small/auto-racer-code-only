using Godot;
using System;
using System.Collections.Generic;
using static AutoRaceEngine;

public class RaceMain : Node2D
{
  private AutoRaceEngine _autoRaceEngine;
  private Button _forwardButton;
  private Label _labelTurnPhase;
  private RichTextLabel _labelGameState;
  private string _updateTurnPhaseLabel;
  private string _updateGameStateLabel;

  public override void _Ready()
  {
    _labelTurnPhase = GetNode(RaceSceneData.Label_TurnPhase) as Label;
    _labelGameState = GetNode(RaceSceneData.RichTextLabel_GameState) as RichTextLabel;
    var goButton = GetNode(RaceSceneData.ButtonFinishPath) as Button;
    goButton.Connect("pressed", this, nameof(Button_finish_pressed));
    _forwardButton = GetNode(RaceSceneData.ButtonForwardPath) as Button;
    _forwardButton.Connect("pressed", this, nameof(Button_forward_pressed));
    var backButton = GetNode(RaceSceneData.ButtonBackPath) as Button;
    backButton.Connect("pressed", this, nameof(Button_back_pressed));

    // AutoRaceEngine(IEnumerable<Player> players, int raceLength, int slotCount, int finishLine)
    _autoRaceEngine = EngineTesting.RaceEngine();
  }

  public override void _Process(float delta)
  {
    if (!string.IsNullOrWhiteSpace(_updateTurnPhaseLabel))
    {
      _labelTurnPhase.Text = _updateTurnPhaseLabel;
    }
    else
    {
      _updateTurnPhaseLabel = "";
    }

    if (!string.IsNullOrWhiteSpace(_updateGameStateLabel))
    {
      _labelGameState.Text = _updateGameStateLabel;
    }
    else
    {
      _updateGameStateLabel = "";
    }
  }

  private void Button_finish_pressed()
  {
    Console.WriteLine("Finish button pressed");
    GetTree().ChangeScene("res://src/scenes/game/Prep.tscn");
  }

  private void Button_forward_pressed()
  {
    Console.WriteLine("Forward button pressed");
    var didWin = _autoRaceEngine.AdvanceRace();
    _updateTurnPhaseLabel = _autoRaceEngine.GetTurnPhase().ToString();
    if (_updateTurnPhaseLabel == TurnPhases.Move.ToString())
    {
      var turnResults = _autoRaceEngine.GetTurnResults();
      _updateGameStateLabel = EngineTesting.GetPositionTextView(turnResults);
    }

    if (didWin)
    {
      _forwardButton.Disabled = true;
      return;
    }
  }

  private void Button_back_pressed()
  {
    Console.WriteLine("Back button pressed");
  }
}
