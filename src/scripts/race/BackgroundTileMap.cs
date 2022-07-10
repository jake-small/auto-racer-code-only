using Godot;
using System;
using System.Collections.Generic;

public class BackgroundTileMap : Godot.TileMap
{
  public bool IsScrolling { get; set; }

  private bool _scrollRight = false;
  private bool _scrollLeft = false;
  private float _moveToX = 0;
  private float _scrollSpeedMultiplier = 1.5F;


  public override void _PhysicsProcess(float delta)
  {
    if (_scrollRight)
    {
      var newX = Position.x + ((-RaceSceneData.GameSpeed * _scrollSpeedMultiplier) * delta);
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
      var newX = Position.x + ((RaceSceneData.GameSpeed * _scrollSpeedMultiplier) * delta);
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
    _scrollSpeedMultiplier = CalculateSpeedMultiplier(amount);
    Console.WriteLine($"ScrollRight: amount={amount} multiplier={_scrollSpeedMultiplier} moveToX={_moveToX}");
  }

  public void ScrollLeft(float amount)
  {
    IsScrolling = true;
    _moveToX = Position.x + amount;
    _scrollLeft = true;
    _scrollSpeedMultiplier = CalculateSpeedMultiplier(amount);
    Console.WriteLine($"ScrollLeft: amount={amount} multiplier={_scrollSpeedMultiplier} moveToX={_moveToX}");
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

  private float CalculateSpeedMultiplier(float distanceToMove)
  {
    var spaces = distanceToMove / RaceSceneData.SpaceWidth;
    if (spaces < 2)
    {
      return 1f;
    }
    else if (spaces < 10)
    {
      return 1.5f;
    }
    else if (spaces < 20)
    {
      return 2f;
    }
    else if (spaces < 30)
    {
      return 2.5f;
    }
    else if (spaces < 40)
    {
      return 3f;
    }
    else if (spaces < 50)
    {
      return 3.5f;
    }
    else
    {
      return 4f;
    }
  }
}
