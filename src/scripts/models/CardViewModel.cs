using System;
using Godot;

public class CardViewModel
{
  public string Name { get; set; }
  public string Body { get; set; }
  public int Level { get; private set; } = 1;
  public int Slot { get; set; } = -1;
  public CardScript CardNode { get; set; }

  public CardViewModel() { }
  public CardViewModel(CardViewModel anotherCard)
  {
    Name = anotherCard.Name;
    Body = anotherCard.Body;
    Level = anotherCard.Level;
    Slot = anotherCard.Slot;
    CardNode = anotherCard.CardNode;
  }

  public CardViewModel Clone() { return new CardViewModel(this); }

  public bool AddLevels(int level)
  {
    Level += level;
    if (CardNode != null)
    {
      var levelLabel = CardNode.GetNode<Label>(PrepSceneData.LabelCardLevel);
      levelLabel.Text = Level.ToString();
    }
    return true;
  }
}
