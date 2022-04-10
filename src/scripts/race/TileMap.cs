using Godot;
using System;

public class TileMap : Godot.TileMap
{
  private float _velocity = -200.0f;

  public override void _Process(float delta)
  {
    Position = (new Vector2(Position.x + (_velocity * delta), Position.y));
    AttemptToReposition();
  }

  private void AttemptToReposition()
  {
    if (Position.x < -GetViewport().Size.x)
    {
      Position = new Vector2(Position.x + (3 * GetViewport().Size.x), Position.y);
    }
  }
}
