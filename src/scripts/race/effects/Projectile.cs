using Godot;
using System;

public class Projectile : Area2D
{
  public CharacterScript Target { get; set; }
  private const float Speed = 260;
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
    if (Target != null && Target.Position != Position)
    {
      _time += delta;
      var towardsTarget = (Target.Position - Position).Normalized();
      var perpendicular = new Vector2(towardsTarget.y, -towardsTarget.x);
      Position += (TowardsStrength * towardsTarget + PerpendicularStrength * perpendicular * (float)Math.Sin(_time)) * Speed * delta;
      if (Math.Abs(Target.Position.x - Position.x) < 5 && Math.Abs(Target.Position.y - Position.y) < 5)
      {
        Target.NegativeTokenValue -= 1;
        // GD.Print($"target id: {Target.Id}, negative tokens {Target.NegativeTokenValue}");
        QueueFree();
      }
    }
    if (Target != null && Target.Position == Position)
    {
      QueueFree();
    }
  }
}