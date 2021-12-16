using System;
using Godot;

public class Card : Godot.Object
{
  public string Name { get; set; }
  public string Body { get; set; }
  public int Level { get; private set; } = 1;
  public int Slot { get; set; } = -1;
  public CardScript CardNode { get; set; }

  public Card() { }
  public Card(Card anotherCard)
  {
    Name = anotherCard.Name;
    Body = anotherCard.Body;
    Level = anotherCard.Level;
    Slot = anotherCard.Slot;
    CardNode = anotherCard.CardNode;
  }

  public Card Clone() { return new Card(this); }

  public bool AddLevels(int level)
  {
    Level += level;
    if (CardNode != null)
    {
      var levelLabel = CardNode.GetNode<Label>(PrepSceneData.CardLabelLevel);
      levelLabel.Text = Level.ToString();
    }
    return true;
  }
}
