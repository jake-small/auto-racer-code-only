using System.Collections.Generic;
using Godot;

public class RaceEndMain : Node2D
{
  public override void _Ready()
  {
    // var results = GameManager.RaceHistory.GetResult(GameManager.CurrentRace);

    // var labelFirst = GetNode(RaceEndSceneData.Label_first) as Label;
    // labelFirst.Text = results[0].Place.IntToPlaceStr();
    // var labelFirstPlayer = GetNode(RaceEndSceneData.Label_first_player) as Label;
    // labelFirstPlayer.Text = results[0].Player.Name + "\nDistance: " + results[0].Position;
    // var labelSecond = GetNode(RaceEndSceneData.Label_second) as Label;
    // labelSecond.Text = results[1].Place.IntToPlaceStr();
    // UpdateLabelPosition(labelSecond, results[1].Place);
    // var labelSecondPlayer = GetNode(RaceEndSceneData.Label_second_player) as Label;
    // labelSecondPlayer.Text = results[1].Player.Name + "\nDistance: " + results[1].Position;
    // var labelThird = GetNode(RaceEndSceneData.Label_third) as Label;
    // labelThird.Text = results[2].Place.IntToPlaceStr();
    // UpdateLabelPosition(labelThird, results[2].Place);
    // var labelThirdPlayer = GetNode(RaceEndSceneData.Label_third_player) as Label;
    // labelThirdPlayer.Text = results[2].Player.Name + "\nDistance: " + results[2].Position;
    // var labelFourth = GetNode(RaceEndSceneData.Label_fourth) as Label;
    // labelFourth.Text = results[3].Place.IntToPlaceStr();
    // UpdateLabelPosition(labelFourth, results[3].Place);
    // var labelFourthPlayer = GetNode(RaceEndSceneData.Label_fourth_player) as Label;
    // labelFourthPlayer.Text = results[3].Player.Name + "\nDistance: " + results[3].Position;

    // var characterFirst = GetNode<CharacterScript>(RaceEndSceneData.Character_first);
    // characterFirst.CharacterSkin = results[0].Player.Skin;
    // var characterSecond = GetNode<CharacterScript>(RaceEndSceneData.Character_second);
    // characterSecond.CharacterSkin = results[1].Player.Skin;
    // var characterThird = GetNode<CharacterScript>(RaceEndSceneData.Character_third);
    // characterThird.CharacterSkin = results[2].Player.Skin;
    // var characterFourth = GetNode<CharacterScript>(RaceEndSceneData.Character_fourth);
    // characterFourth.CharacterSkin = results[3].Player.Skin;

    var endRaceButton = GetNode<TextureButton>(RaceSceneData.ButtonFinishPath);
    endRaceButton.Connect("pressed", this, nameof(Button_finish_pressed));
  }

  private void Button_finish_pressed()
  {
    GD.Print("Finish button pressed");
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

  private void UpdateLabelPosition(Label label, int place)
  {
    label.RectPosition = new Vector2(label.RectPosition.x, label.RectPosition.y + 40 * place);
  }
}