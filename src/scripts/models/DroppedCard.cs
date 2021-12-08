using System;
using Godot;

public class DroppedCard : Godot.Object
{
  public Card Card { get; set; }
  public KinematicBody2D CardNode { get; set; }
  public Vector2 DroppedPosition { get; set; }
  public Vector2 OriginalPosition { get; set; }


  public DroppedCard() { }
}
