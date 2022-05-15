using Godot;
using System;

public class Projectile : Sprite
{
  public CharacterScript Target { get; set; }
  public float? DelayedTakeoffAmount { get; set; }
  public bool IsPositive { get; set; }

  private float _speed = 260;
  private float _time = 0;
  private const float TowardsStrength = 0.5f;
  private const float PerpendicularStrength = 0.5f;
  private bool _transparent = true;
  private float _transparentValue = 0.5f;

  public override void _Ready()
  {
    SpawnPosition();
    Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, _transparentValue);
  }

  public override void _Process(float delta)
  {
    if (Target == null || DelayedTakeoffAmount == null)
    {
      return;
    }

    _time += delta;
    if (_time <= DelayedTakeoffAmount)
    {
      return;
    }

    if (_transparent)
    {
      Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 1f);
      _transparent = false;
    }

    if (Target.Position != Position)
    {
      if (_time > 5 + DelayedTakeoffAmount)
      {
        Despawn();
      }
      _speed = _speed + (_speed * delta);

      var towardsTarget = (Target.Position - Position).Normalized();
      var perpendicular = new Vector2(towardsTarget.y, -towardsTarget.x);
      Position += (TowardsStrength * towardsTarget + PerpendicularStrength * perpendicular * (float)Math.Sin(_time)) * _speed * delta;
      if (Math.Abs(Target.Position.x - Position.x) < 20 && Math.Abs(Target.Position.y - Position.y) < 20)
      {
        Despawn();
      }
    }
    else
    {
      Despawn();
    }
  }

  private void SpawnPosition()
  {
    var random = new Random();
    var radius = random.Next(32, 64);
    var min = 0;
    var max = Math.PI * 2;
    var angle = random.NextDouble() * (max - min) + min;
    var offsetX = (float)Math.Sin(angle) * radius;
    var offsetY = (float)Math.Cos(angle) * radius;
    Position = new Vector2(Position.x + offsetX, Position.y + offsetY);
  }

  private void Despawn()
  {
    if (Target == null)
    {
      return;
    }
    if (IsPositive)
    {
      Target.PositiveTokenValue += 1;
    }
    else
    {
      Target.NegativeTokenValue -= 1;
    }

    QueueFree();
  }
}