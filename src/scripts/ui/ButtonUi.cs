using Godot;
using System;

public class ButtonUi : TextureButton
{
  private Vector2 _startingPosition;
  private float _moveDownAmount = 4;

  public override void _Ready()
  {
    _startingPosition = this.RectGlobalPosition;
  }

  public override void _Process(float delta)
  {
    if (_startingPosition != default(Vector2)
      && RectGlobalPosition != _startingPosition
      && Disabled)
    {
      SetGlobalPosition(_startingPosition);
    }
  }

  public void _on_TextureButton_down()
  {
    var newPosition = RectGlobalPosition + new Vector2(0, _moveDownAmount);
    SetGlobalPosition(newPosition);
  }

  public void _on_TextureButton_up()
  {
    SetGlobalPosition(_startingPosition);
  }
}