using System.Collections.Generic;
using Godot;

public class RaceEndMain : Node2D
{
  public override void _Ready()
  {
    var results = GameManager.RaceHistory.GetResult(GameManager.CurrentRace);

    var labelFirstPlayer = GetNode(RaceEndSceneData.Label_first_player) as Label;
    labelFirstPlayer.Text = results[0].Id + "\nDistance: " + results[0].Position;
    var labelSecondPlayer = GetNode(RaceEndSceneData.Label_second_player) as Label;
    labelSecondPlayer.Text = results[1].Id + "\nDistance: " + results[1].Position;
    var labelThirdPlayer = GetNode(RaceEndSceneData.Label_third_player) as Label;
    labelThirdPlayer.Text = results[2].Id + "\nDistance: " + results[2].Position;
    var labelFourthPlayer = GetNode(RaceEndSceneData.Label_fourth_player) as Label;
    labelFourthPlayer.Text = results[3].Id + "\nDistance: " + results[3].Position;

    // var characterFirst = GetNode<CharacterScript>(RaceEndSceneData.Character_first);
    // characterFirst.CharacterSkin = GameManager.PlayerCharacterSkin;
    // var characterSecond = GetNode<CharacterScript>(RaceEndSceneData.Character_second);
    // characterFirst.CharacterSkin = GameManager.OpposingCharacterSkins[0];
    // var characterThird = GetNode<CharacterScript>(RaceEndSceneData.Character_third);
    // characterFirst.CharacterSkin = GameManager.OpposingCharacterSkins[1];
    // var characterFourth = GetNode<CharacterScript>(RaceEndSceneData.Character_fourth);
    // characterFirst.CharacterSkin = GameManager.OpposingCharacterSkins[2];

    var endRaceButton = GetNode(RaceSceneData.ButtonFinishPath) as Button;
    endRaceButton.Connect("pressed", this, nameof(Button_finish_pressed));
  }

  private void Button_finish_pressed()
  {
    GD.Print("Finish button pressed");
    // GameManager.OpposingCharacterSkins = new List<string>();
    if (GameManager.CurrentRace >= GameManager.TotalRaces)
    {
      GD.Print("Race over!");
      GetTree().ChangeScene("res://src/scenes/game/GameEnd.tscn");
    }
    else
    {
      GetTree().ChangeScene("res://src/scenes/game/Prep.tscn");
    }
  }
}