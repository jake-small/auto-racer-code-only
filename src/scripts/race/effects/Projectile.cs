using Godot;
using System;

public class Projectile : Area2D
{
  public Vector2 Target { get; set; }
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
    if (Target != null && Target != Position)
    {
      _time += delta;
      var towardsTarget = (Target - Position).Normalized();
      var perpendicular = new Vector2(towardsTarget.y, -towardsTarget.x);
      Position += (TowardsStrength * towardsTarget + PerpendicularStrength * perpendicular * (float)Math.Sin(_time)) * Speed * delta;
      if (Math.Abs(Target.x - Position.x) < 5 && Math.Abs(Target.y - Position.y) < 5)
      {
        QueueFree();
      }
    }
    else if (Target != null && Target == Position)
    {
      QueueFree();
    }
  }

  private Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
  {
    Vector2 q0 = p0.LinearInterpolate(p1, t);
    Vector2 q1 = p1.LinearInterpolate(p2, t);
    Vector2 q2 = p2.LinearInterpolate(p3, t);

    Vector2 r0 = q0.LinearInterpolate(q1, t);
    Vector2 r1 = q1.LinearInterpolate(q2, t);

    Vector2 s = r0.LinearInterpolate(r1, t);
    return s;
  }
}