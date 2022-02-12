using Godot;

public class RaceEndMain : Node2D
{
  public override void _Ready()
  {
    var results = GameManager.RaceHistory.GetResult(GameManager.RaceNumber);

    var labelFirstPlayer = GetNode(RaceEndSceneData.Label_first_player) as Label;
    labelFirstPlayer.Text = results[0].Id + "\nDistance: " + results[0].Position;
    var labelSecondPlayer = GetNode(RaceEndSceneData.Label_second_player) as Label;
    labelSecondPlayer.Text = results[1].Id + "\nDistance: " + results[1].Position;
    var labelThirdPlayer = GetNode(RaceEndSceneData.Label_third_player) as Label;
    labelThirdPlayer.Text = results[2].Id + "\nDistance: " + results[2].Position;
    var labelFourthPlayer = GetNode(RaceEndSceneData.Label_fourth_player) as Label;
    labelFourthPlayer.Text = results[3].Id + "\nDistance: " + results[3].Position;

    var endRaceButton = GetNode(RaceSceneData.ButtonFinishPath) as Button;
    endRaceButton.Connect("pressed", this, nameof(Button_finish_pressed));
  }

  private void Button_finish_pressed()
  {
    GD.Print("Finish button pressed");
    GetTree().ChangeScene("res://src/scenes/game/Prep.tscn");
  }
}