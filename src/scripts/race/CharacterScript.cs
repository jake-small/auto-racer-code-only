using Godot;
using System;

public class CharacterScript : Node2D
{
  public int Id { get; set; }
  public int PositiveTokenValue { get; set; } = 0;
  public int NegativeTokenValue { get; set; } = 0;

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

  public float MoveToX { get; private set; } = 0;
  public bool Moving { get; private set; } = false;

  private AnimatedSprite _sprite;
  private AnimationPlayer _animationPlayer;
  private Label _positiveTokenLabel;
  private Label _negativeTokenLabel;
  private bool raceOver = false;

  public override void _Ready()
  {
    _sprite = GetNode<AnimatedSprite>(RaceSceneData.CharacterSpritePath);
    _animationPlayer = GetNode<AnimationPlayer>(RaceSceneData.CharacterAnimationPlayerPath);
    _positiveTokenLabel = GetNode<Label>(RaceSceneData.CharacterTokenPositiveLabel);
    _negativeTokenLabel = GetNode<Label>(RaceSceneData.CharacterTokenNegativeLabel);
    if (CharacterSkin == null)
    {
      GD.Print("Getting random skin for player");
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
      var doneMoving = false;
      float newX;
      if (MoveToX > Position.x)
      {
        newX = Position.x + (RaceSceneData.GameSpeed * delta);
        if (newX >= MoveToX)
        {
          newX = MoveToX;
          doneMoving = true;
        }

      }
      else
      {
        newX = Position.x - (RaceSceneData.GameSpeed * delta);
        if (newX <= MoveToX)
        {
          newX = MoveToX;
          doneMoving = true;
        }
      }
      Position = (new Vector2(newX, Position.y));
      if (doneMoving)
      {
        StopMoving();
      }
    }
  }

  public override void _Process(float delta)
  {
    if (raceOver && !Moving)
    {
      AnimationState = AnimationStates.facing_front;
    }
    _positiveTokenLabel.Text = PositiveTokenValue > 0 ? $"+{PositiveTokenValue}" : "";
    _positiveTokenLabel.Visible = PositiveTokenValue > 0;
    _negativeTokenLabel.Text = NegativeTokenValue < 0 ? $"{NegativeTokenValue}" : "";
    _negativeTokenLabel.Visible = NegativeTokenValue < 0;
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

  public void ProjectileAttackAnimation(CharacterScript target, int amount, int duration)
  {
    var projectileScene = ResourceLoader.Load("res://src/scenes/objects/effects/Projectile.tscn") as PackedScene;
    amount = amount < 0 ? amount * -1 : amount;
    SpawnProjectiles(target, amount, duration, projectileScene, false);
  }

  public void ProjectileBuffAnimation(CharacterScript target, int amount, int duration)
  {
    var projectileScene = ResourceLoader.Load("res://src/scenes/objects/effects/ProjectileBuff.tscn") as PackedScene;
    SpawnProjectiles(target, amount, duration, projectileScene, true);
  }

  private void SpawnProjectiles(CharacterScript target, int amount, int duration, PackedScene projectileScene, bool isPositive)
  {
    for (int i = 0; i < amount; i++)
    {
      var projectileInstance = (Projectile)projectileScene.Instance();
      projectileInstance.Position = Position;
      projectileInstance.Target = target;
      projectileInstance.IsPositive = isPositive;
      projectileInstance.Length = (duration - 1) * 10;
      projectileInstance.DelayedTakeoffAmount = (i + 6) * 0.1f;
      GetTree().Root.AddChild(projectileInstance);
    }
  }

  public void OnHitAnimate()
  {
    _animationPlayer.Stop(true);
    _animationPlayer.Play("OnHit");
    emitOnHitSignal();
  }

  public void OnBuffAnimate()
  {
    _animationPlayer.Stop(true);
    _animationPlayer.Play("OnBuff");
    emitOnBuffSignal();
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

  [Signal]
  public delegate void onHit(CharacterScript characterScript);
  public void emitOnHitSignal()
  {
    GD.Print($"OnHit signal EMITTED for CharacterScript with Id: {Id}");
    EmitSignal(nameof(onHit), this);
  }

  [Signal]
  public delegate void onBuff(CharacterScript characterScript);
  public void emitOnBuffSignal()
  {
    GD.Print($"OnBuff signal EMITTED for CharacterScript with Id: {Id}");
    EmitSignal(nameof(onBuff), this);
  }
}

public enum AnimationStates
{
  running,
  standing,
  facing_front
}