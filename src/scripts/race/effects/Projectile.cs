using Godot;
using System;

public class Projectile : Area2D
{
  public CharacterScript Target { get; set; }
  public float? DelayedTakeoffAmount { get; set; }

  private float _speed = 260;
  private float _time = 0;
  private const float TowardsStrength = 0.5f;
  private const float PerpendicularStrength = 0.5f;

  public override void _Ready()
  {
    var random = new Random();
    var offsetX = random.Next(32, 64);
    var offsetY = random.Next(32, 64);
    var directionX = random.Next(2) == 0 ? -1 : 1;
    var directionY = random.Next(2) == 0 ? -1 : 1;
    Position = new Vector2(Position.x + (offsetX * directionX), Position.y + (offsetY * directionY));
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

  private void Despawn()
  {
    if (Target == null)
    {
      return;
    }
    Target.NegativeTokenValue -= 1;
    QueueFree();
  }
}