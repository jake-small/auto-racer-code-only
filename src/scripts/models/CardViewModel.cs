using System;
using Godot;

public class CardViewModel : Godot.Object // this inheritance is necessary for signals to function
{
  public int Level { get; private set; } = 1;
  public int Slot { get; set; } = -1;
  public CardScript CardNode { get; set; }
  public Card Card { get; set; }

  public CardViewModel() { }
  public CardViewModel(CardViewModel anotherCard)
  {
    Level = anotherCard.Level;
    Slot = anotherCard.Slot;
    CardNode = anotherCard.CardNode;
    Card = anotherCard.Card;
  }

  public CardViewModel Clone() { return new CardViewModel(this); }

  public bool AddLevels(int level)
  {
    Level += level;
    if (CardNode != null)
    {
      var levelLabel = CardNode.GetNode<Label>(PrepSceneData.LabelCardLevel);
      levelLabel.Text = "lvl" + Level.ToString();
    }
    return true;
  }
}
