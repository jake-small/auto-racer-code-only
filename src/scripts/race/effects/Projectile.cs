using Godot;
using System;

public class Projectile : Area2D
{
  public Vector2 Target { get; set; }
  private const float Speed = 160;
  private Vector2 _velocity;

  public override void _Ready()
  {
    var random = new Random();
    var offsetX = random.Next(32, 64);
    var offsetY = random.Next(32, 64);
    var directionX = random.Next(2) == 0 ? -1 : 1;
    var directionY = random.Next(2) == 0 ? -1 : 1;
    Position = new Vector2(Position.x + (offsetX * directionX), Position.y + (offsetY * directionY));
  }

  public override void _PhysicsProcess(float delta)
  {
    if (Target != null && Target != Position)
    {
      Position = Position.MoveToward(Target, delta * Speed);
    }
    else if (Target != null && Target == Position)
    {
      QueueFree();
    }
  }
}