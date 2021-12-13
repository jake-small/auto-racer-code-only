using System;
using Godot;

public class DroppedCard : Godot.Object
{
  public KinematicBody2D CardNode { get; }
  public Vector2 DroppedPosition { get; }


  public DroppedCard(KinematicBody2D cardNode, Vector2 droppedPosition)
  {
    CardNode = cardNode;
    DroppedPosition = droppedPosition;
  }
}