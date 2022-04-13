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

  public float MoveToX { get; private set; } = 0;
  public bool Moving { get; private set; } = false;

  private AnimatedSprite _sprite;
  private bool raceOver = false;

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
    if (Moving)
    {
      float newX;
      if (MoveToX > Position.x)
      {
        newX = Position.x + (RaceSceneData.GameSpeed * delta);
        if (newX >= MoveToX)
        {
          newX = MoveToX;
          StopMoving();
        }

      }
      else
      {
        newX = Position.x - (RaceSceneData.GameSpeed * delta);
        if (newX <= MoveToX)
        {
          newX = MoveToX;
          StopMoving();
        }
      }
      Position = (new Vector2(newX, Position.y));
    }
  }

  public override void _Process(float delta)
  {
    if (raceOver && !Moving)
    {
      AnimationState = AnimationStates.facing_front;
    }
  }

  public void Move(float xAmount)
  {
    if (xAmount == 0)
    {
      return;
    }
    if (Moving)
    {
      MoveToX = MoveToX + xAmount;
      return;
    }
    MoveToX = Position.x + xAmount;
    _sprite.Animation = AnimationStates.running.ToString();
    _sprite.Playing = true;
    Moving = true;
  }

  public void RaceOverAnimation()
  {
    raceOver = true;
  }

  private void StopMoving()
  {
    Moving = false;
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
      _sprite.Playing = animationState == AnimationStates.running;
    }
  }
}
