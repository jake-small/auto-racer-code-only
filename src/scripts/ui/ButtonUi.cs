using Godot;
using System;

public class ButtonUi : TextureButton
{
  private float _moveDownAmount = 4;

  public void _on_TextureButton_down()
  {
    var newPosition = RectPosition + new Vector2(0, _moveDownAmount);
    SetPosition(newPosition);
  }

  public void _on_TextureButton_up()
  {
    var newPosition = RectPosition - new Vector2(0, _moveDownAmount);
    SetPosition(newPosition);
  }
}