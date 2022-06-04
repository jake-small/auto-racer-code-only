using Godot;
using System;

public class ButtonUi : TextureButton
{
  private Vector2 _startingPosition;
  private float _moveDownAmount = 4;

  public override void _Ready()
  {
    _startingPosition = this.RectPosition;
  }

  public void _on_TextureButton_down()
  {
    var newPosition = RectPosition + new Vector2(0, _moveDownAmount);
    SetPosition(newPosition);
  }

  public void _on_TextureButton_up()
  {
    SetPosition(_startingPosition);
  }
}