using System;
using Godot;

public class CardViewModel : Godot.Object // this inheritance is necessary for signals to function
{
  public int Slot { get; set; } = -1;
  public CardScript CardNode { get; set; }
  public Card Card { get; set; }

  public CardViewModel() { }
  public CardViewModel(CardViewModel anotherCard)
  {
    Slot = anotherCard.Slot;
    CardNode = anotherCard.CardNode;
    Card = anotherCard.Card;
  }

  public CardViewModel Clone() { return new CardViewModel(this); }

  public bool AddLevels(int level)
  {
    Card.Level += level;
    if (CardNode != null)
    {
      var levelLabel = CardNode.GetNode<Label>(PrepSceneData.LabelCardLevel);
      levelLabel.Text = "lvl" + Card.Level.ToString();
    }
    return true;
  }
}
