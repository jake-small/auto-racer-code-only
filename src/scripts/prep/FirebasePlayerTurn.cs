using System.Linq;

public class FirebasePlayerTurn
{
  public string CardVersion { get; set; }
  public string Skin { get; set; }
  public int AmountUsed { get; set; }
  public string PlayerName { get; set; }
  public string PlayerId { get; set; }
  public int Turn { get; set; }
  public FirebaseCards Cards { get; set; }

  public FirebasePlayerTurn(Godot.Collections.Dictionary playerTurnDict)
  {
    foreach (var key in playerTurnDict.Keys)
    {
      switch (key.ToString())
      {
        case "card_version":
          CardVersion = playerTurnDict[key].ToString();
          break;
        case "skin":
          Skin = playerTurnDict[key].ToString();
          break;
        case "amount_used":
          AmountUsed = (int)playerTurnDict[key];
          break;
        case "player_name":
          PlayerName = playerTurnDict[key].ToString();
          break;
        case "player_id":
          PlayerId = playerTurnDict[key].ToString();
          break;
        case "turn":
          Turn = (int)playerTurnDict[key];
          break;
        case "cards":
          var firebaseCardsUntyped = playerTurnDict[key] as Godot.Collections.Array;
          var firebaseCards = new Godot.Collections.Array<Godot.Collections.Dictionary>(firebaseCardsUntyped);
          Cards = new FirebaseCards(firebaseCards);
          break;
        default:
          break;
      }
    }
  }
}