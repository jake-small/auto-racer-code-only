using System.Collections.Generic;
using Godot;

public class RaceEndMain : Node2D
{
  public override void _Ready()
  {
    var results = GameManager.RaceHistory.GetResult(GameManager.CurrentRace);

    var labelFirst = GetNode(RaceEndSceneData.Label_first) as Label;
    var labelFirstPlayer = GetNode(RaceEndSceneData.Label_first_player) as Label;
    var characterFirst = GetNode<CharacterScript>(RaceEndSceneData.Character_first);
    if (GameManager.NumPlayers > 0)
    {
      labelFirst.Text = results[0].Place.IntToPlaceStr();
      labelFirstPlayer.Text = results[0].Player.Name + "\nDistance: " + results[0].Position;
      characterFirst.CharacterSkin = results[0].Player.Skin;
    }
    else
    {
      labelFirst.Visible = false;
      labelFirstPlayer.Visible = false;
      characterFirst.Visible = false;
    }

    var labelSecond = GetNode(RaceEndSceneData.Label_second) as Label;
    var labelSecondPlayer = GetNode(RaceEndSceneData.Label_second_player) as Label;
    var characterSecond = GetNode<CharacterScript>(RaceEndSceneData.Character_second);
    if (GameManager.NumPlayers > 1)
    {
      labelSecond.Text = results[1].Place.IntToPlaceStr();
      UpdateLabelPosition(labelSecond, results[1].Place);
      labelSecondPlayer.Text = results[1].Player.Name + "\nDistance: " + results[1].Position;
      characterSecond.CharacterSkin = results[1].Player.Skin;
    }
    else
    {
      labelSecond.Visible = false;
      labelSecondPlayer.Visible = false;
      characterSecond.Visible = false;
    }

    var labelThird = GetNode(RaceEndSceneData.Label_third) as Label;
    var labelThirdPlayer = GetNode(RaceEndSceneData.Label_third_player) as Label;
    var characterThird = GetNode<CharacterScript>(RaceEndSceneData.Character_third);
    if (GameManager.NumPlayers > 2)
    {
      labelThird.Text = results[2].Place.IntToPlaceStr();
      UpdateLabelPosition(labelThird, results[2].Place);
      labelThirdPlayer.Text = results[2].Player.Name + "\nDistance: " + results[2].Position;
      characterThird.CharacterSkin = results[2].Player.Skin;
    }
    else
    {
      labelThird.Visible = false;
      labelThirdPlayer.Visible = false;
      characterThird.Visible = false;
    }

    var labelFourth = GetNode(RaceEndSceneData.Label_fourth) as Label;
    var labelFourthPlayer = GetNode(RaceEndSceneData.Label_fourth_player) as Label;
    var characterFourth = GetNode<CharacterScript>(RaceEndSceneData.Character_fourth);
    if (GameManager.NumPlayers > 3)
    {
      labelFourth.Text = results[3].Place.IntToPlaceStr();
      UpdateLabelPosition(labelFourth, results[3].Place);
      labelFourthPlayer.Text = results[3].Player.Name + "\nDistance: " + results[3].Position;
      characterFourth.CharacterSkin = results[3].Player.Skin;
    }
    else
    {
      labelFourth.Visible = false;
      labelFourthPlayer.Visible = false;
      characterFourth.Visible = false;
    }

    if (GameManager.NumPlayers == 2)
    {
      labelFirst.RectPosition = new Vector2(labelFirst.RectPosition.x + 329, labelFirst.RectPosition.y);
      labelSecond.RectPosition = new Vector2(labelSecond.RectPosition.x + 329, labelSecond.RectPosition.y);
    }

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