using Godot;
using System;

public class MainMenu : Control
{
  public override void _Ready()
  {
    var startButton = GetNode("MarginContainer/VBoxContainer/HBoxContainer/Button_Start") as Button;
    startButton.Connect("pressed", this, "Button_start_pressed");
    var quitButton = GetNode("MarginContainer/VBoxContainer/HBoxContainer/Button_Quit") as Button;
    quitButton.Connect("pressed", this, "Button_quit_pressed");
  }

  private void Button_start_pressed()
  {
    Console.WriteLine("Start Game button pressed");
    GetTree().ChangeScene("res://src/scenes/game/Prep.tscn");
  }

  private void Button_quit_pressed()
  {
    Console.WriteLine("Quit button pressed");
    GetTree().Quit();
  }
}
