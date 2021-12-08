using System;

public class Card : Godot.Object
{
  public string Name { get; set; }
  public string Body { get; set; }

  public int Slot { get; set; } = -1;

  public Card() { }
}
