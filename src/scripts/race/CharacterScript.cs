using Godot;
using System;

public class CharacterScript : Node2D
{
  public int Id { get; set; }

  private string _characterSkin;
  public string CharacterSkin
  {
    get
    {
      return _characterSkin;
    }
    set
    {
      _characterSkin = value;
      SetSkin(_characterSkin);
    }
  }

  private AnimationStates _animationState = AnimationStates.facing_front;
  public AnimationStates AnimationState
  {
    get
    {
      return _animationState;
    }
    set
    {
      _animationState = value;
      SetAnimationState(_animationState);
    }
  }

  public enum AnimationStates
  {
    running,
    standing,
    facing_front
  }

  private AnimatedSprite _sprite;

  private bool _moving = false;
  private float _moveToX = 0;
  private float _velocity = 200.0f;


  public override void _Ready()
  {
    _sprite = GetNode<AnimatedSprite>(RaceSceneData.CharacterSpritePath);
    if (CharacterSkin == null)
    {
      CharacterSkin = GetRandomSkin();
    }
    else
    {
      _sprite.Frames = (SpriteFrames)GD.Load(CharacterSkin);
    }
    _sprite.Animation = AnimationState.ToString();
    if (AnimationState == AnimationStates.running)
    {
      _sprite.Playing = true;
    }
    else
    {
      _sprite.Playing = false;
    }
  }

  public override void _PhysicsProcess(float delta)
  {
    if (_moving)
    {
      float newX;
      if (_moveToX > Position.x)
      {
        newX = Position.x + (_velocity * delta);
        if (newX >= _moveToX)
        {
          newX = _moveToX;
          _moving = false;
        }

      }
      else
      {
        newX = Position.x - (_velocity * delta);
        if (newX <= _moveToX)
        {
          newX = _moveToX;
          _moving = false;
        }
      }
      Position = (new Vector2(newX, Position.y));
    }
  }

  public void Move(float toXPosition)
  {
    if (toXPosition == Position.x)
    {
      return;
    }
    // _moveToX = Position.x + (RaceSceneData.SpaceWidth * numSpaces);
    _moveToX = toXPosition;
    _sprite.Animation = AnimationStates.running.ToString();
    _sprite.Playing = true;
    _moving = true;
  }

  private void StopMoving()
  {
    _moving = false;
    _sprite.Animation = AnimationStates.standing.ToString();
    _sprite.Playing = false;
  }

  private string GetRandomSkin()
  {
    var random = new Random();
    var index = random.Next(GameManager.CharacterSkins.Count);
    return GameManager.CharacterSkins[index];
  }

  private void SetSkin(string skinPath)
  {
    if (_sprite != null)
    {
      _sprite.Frames = (SpriteFrames)GD.Load(skinPath);
    }
  }

  private void SetAnimationState(AnimationStates animationState)
  {
    if (_sprite != null)
    {
      _sprite.Animation = AnimationState.ToString();
    }
  }
}
