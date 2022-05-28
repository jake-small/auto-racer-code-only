using Godot;
using System;

public class Credits : Control
{
  public override void _Ready()
  {
    var backButton = GetNode<TextureButton>("Button_back");
    backButton.Connect("pressed", this, nameof(Button_back_pressed));
  }

  private void Button_back_pressed()
  {
    Console.WriteLine("Back button pressed");
    GetTree().ChangeScene("res://src/scenes/menus/MainMenu.tscn");
  }
}
