using Godot;
using System;

public class CharacterScript : Node2D
{
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

  private AnimatedSprite Sprite;

  public override void _Ready()
  {
    Sprite = GetNode<AnimatedSprite>(RaceSceneData.CharacterSpritePath);
    if (CharacterSkin == null)
    {
      CharacterSkin = GetRandomSkin();
    }
    else
    {
      Sprite.Frames = (SpriteFrames)GD.Load(CharacterSkin);
    }
    Sprite.Animation = AnimationState.ToString();
  }

  public void StartRunAnimation()
  {
    Sprite.Animation = AnimationStates.running.ToString();
    Sprite.Playing = true;
  }

  public void StopRunAnimation()
  {
    Sprite.Animation = AnimationStates.standing.ToString();
    Sprite.Playing = false;
  }

  private string GetRandomSkin()
  {
    var random = new Random();
    var index = random.Next(GameManager.CharacterSkins.Count);
    return GameManager.CharacterSkins[index];
  }

  private void SetSkin(string skinPath)
  {
    if (Sprite != null)
    {
      Sprite.Frames = (SpriteFrames)GD.Load(skinPath);
    }
  }

  private void SetAnimationState(AnimationStates animationState)
  {
    if (Sprite != null)
    {
      Sprite.Animation = AnimationState.ToString();
    }
  }
}
