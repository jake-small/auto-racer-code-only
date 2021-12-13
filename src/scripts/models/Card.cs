using System;
using Godot;

public class Card : Godot.Object
{
  public string Name { get; set; }
  public string Body { get; set; }

  public int Slot { get; set; } = -1;
  public KinematicBody2D CardNode { get; set; }

  public Card() { }
  public Card(Card anotherCard)
  {
    Name = anotherCard.Name;
    Body = anotherCard.Body;
    Slot = anotherCard.Slot;
    CardNode = anotherCard.CardNode;
  }

  public Card Clone() { return new Card(this); }
}
