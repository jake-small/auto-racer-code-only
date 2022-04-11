using Godot;
using System;

public class BackgroundTileMap : Godot.TileMap
{
  private bool _scrollRight = false;
  private bool _scrollLeft = false;
  private float _moveToX = 0;


  public override void _PhysicsProcess(float delta)
  {
    if (_scrollRight)
    {
      var newX = Position.x + ((-RaceSceneData.GameSpeed / 2) * delta);
      if (newX <= _moveToX)
      {
        newX = _moveToX;
        _scrollRight = false;
      }
      Position = (new Vector2(newX, Position.y));
      AttemptToRepositionRight();
    }
    else if (_scrollLeft)
    {
      var newX = Position.x + ((RaceSceneData.GameSpeed / 2) * delta);
      if (newX >= _moveToX)
      {
        newX = _moveToX;
        _scrollLeft = false;
      }
      Position = (new Vector2(newX, Position.y));
      AttemptToRepositionLeft();
    }
  }

  public void ScrollRight(float amount)
  {
    _moveToX = Position.x - amount;
    _scrollRight = true;
  }

  public void ScrollLeft(float amount)
  {
    _moveToX = Position.x + amount;
    _scrollLeft = true;
  }

  private void AttemptToRepositionRight()
  {
    if (Position.x < -GetViewport().Size.x)
    {
      var m = _moveToX - Position.x;
      Position = new Vector2(Position.x + (3 * GetViewport().Size.x), Position.y);
      _moveToX = Position.x + m;
    }
  }

  private void AttemptToRepositionLeft()
  {
    if (Position.x > GetViewport().Size.x)
    {
      var m = _moveToX - Position.x;
      Position = new Vector2(Position.x + (3 * -GetViewport().Size.x), Position.y);
      _moveToX = Position.x + m;
    }
  }
}
