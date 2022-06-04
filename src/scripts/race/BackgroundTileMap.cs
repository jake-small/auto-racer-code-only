using Godot;
using System;

public class BackgroundTileMap : Godot.TileMap
{
  public bool IsScrolling { get; set; }

  private bool _scrollRight = false;
  private bool _scrollLeft = false;
  private float _moveToX = 0;
  private float _gameSpeedMultiplier = 1.5F;


  public override void _PhysicsProcess(float delta)
  {
    if (_scrollRight)
    {
      var newX = Position.x + ((-RaceSceneData.GameSpeed * _gameSpeedMultiplier) * delta);
      if (newX <= _moveToX)
      {
        newX = _moveToX;
        _scrollRight = false;
      }
      Position = (new Vector2(newX, Position.y));
      AttemptToRepositionRight();
      IsScrolling = _scrollRight;
    }
    else if (_scrollLeft)
    {
      var newX = Position.x + ((RaceSceneData.GameSpeed * _gameSpeedMultiplier) * delta);
      if (newX >= _moveToX)
      {
        newX = _moveToX;
        _scrollLeft = false;
      }
      Position = (new Vector2(newX, Position.y));
      AttemptToRepositionLeft();
      IsScrolling = _scrollLeft;
    }
  }

  public void ScrollRight(float amount)
  {
    IsScrolling = true;
    _moveToX = Position.x - amount;
    _scrollRight = true;
  }

  public void ScrollLeft(float amount)
  {
    IsScrolling = true;
    _moveToX = Position.x + amount;
    _scrollLeft = true;
  }

  private void AttemptToRepositionRight()
  {
    if (Position.x < -GetViewportRect().Size.x)
    {
      var m = _moveToX - Position.x;
      Position = new Vector2(Position.x + (3 * GetViewportRect().Size.x), Position.y);
      _moveToX = Position.x + m;
    }
  }

  private void AttemptToRepositionLeft()
  {
    if (Position.x > GetViewportRect().Size.x)
    {
      var m = _moveToX - Position.x;
      Position = new Vector2(Position.x + (3 * -GetViewportRect().Size.x), Position.y);
      _moveToX = Position.x + m;
    }
  }
}
