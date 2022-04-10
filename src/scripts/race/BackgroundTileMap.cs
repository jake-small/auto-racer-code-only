using Godot;
using System;

public class BackgroundTileMap : Godot.TileMap
{
  private bool _scrollRight = false;
  private float _moveToX = 0;
  private float _velocity = -400.0f;


  public override void _PhysicsProcess(float delta)
  {
    if (_scrollRight)
    {
      var newX = Position.x + (_velocity * delta);
      if (newX <= _moveToX)
      {
        newX = _moveToX;
        _scrollRight = false;
      }
      Position = (new Vector2(newX, Position.y));
      AttemptToReposition();
    }
  }

  public void ScrollRight(int numSpaces)
  {
    _moveToX = Position.x - (128 * numSpaces);
    _scrollRight = true;
  }

  private void AttemptToReposition()
  {
    if (Position.x < -GetViewport().Size.x)
    {
      var m = _moveToX - Position.x;
      Position = new Vector2(Position.x + (3 * GetViewport().Size.x), Position.y);
      _moveToX = Position.x + m;
    }
  }
}
