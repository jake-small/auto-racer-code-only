using System.Collections.Generic;

public class FirebaseCards
{
  public Godot.Collections.Array<Godot.Collections.Dictionary> GodotCards { get; private set; }

  public FirebaseCards(Dictionary<int, Card> slottedCards)
  {
    var gArray = new Godot.Collections.Array<Godot.Collections.Dictionary>();
    foreach (var slottedCard in slottedCards)
    {
      var gDict = new Godot.Collections.Dictionary();
      gDict.Add("guid", slottedCard.Value.Guid);
      gDict.Add("base_move", slottedCard.Value.BaseMove);
      gDict.Add("slot", slottedCard.Key);
      gDict.Add("level", slottedCard.Value.Level);
      gArray.Add(gDict);
    }
    GodotCards = gArray;
  }

  public FirebaseCards(Godot.Collections.Array<Godot.Collections.Dictionary> firebaseCards)
  {
    GodotCards = firebaseCards;
  }
}