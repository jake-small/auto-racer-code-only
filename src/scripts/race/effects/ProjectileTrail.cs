using Godot;
using System;

public class ProjectileTrail : Line2D
{
  public int Length { get; set; }
  public override void _Ready()
  {
    SetAsToplevel(true);
  }

  public override void _Process(float delta)
  {
    if (Length == 0)
    {
      return;
    }
    var point = ((Sprite)GetParent()).GlobalPosition;
    AddPoint(point);
    if (Points.Length > Length)
    {
      RemovePoint(0);
    }
  }
}