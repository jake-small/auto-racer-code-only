using System;
using System.Collections.Generic;
using System.Linq;

public class FirebasePlayer : Player
{
  public FirebasePlayer(int id, FirebasePlayerTurn firebasePlayerTurn, IEnumerable<Card> availableCards)
  {
    Id = id;
    Name = firebasePlayerTurn.CharacterName;
    Position = 0;
    Cards = ConvertFirebaseCards(firebasePlayerTurn.Cards, availableCards);
    Skin = firebasePlayerTurn.Skin;
  }

  private Dictionary<int, Card> ConvertFirebaseCards(FirebaseCards firebaseCards, IEnumerable<Card> availableCards)
  {
    var cards = new Dictionary<int, Card>();
    foreach (var cardGDict in firebaseCards.GodotCards.ToArray())
    {
      var slot = (int)cardGDict["slot"];
      var guid = (string)cardGDict["guid"];
      if (string.IsNullOrWhiteSpace(guid))
      {
        cards[slot] = new CardEmpty();
        continue;
      }
      var cardBase = availableCards.FirstOrDefault(c => c.Guid == guid);
      if (cardBase == null)
      {
        cards[slot] = new CardEmpty();
        continue;
      }
      var card = (Card)cardBase.Clone();
      var baseMove = (int)cardGDict["base_move"];
      card.BaseMove = baseMove;
      var level = (int)cardGDict["level"];
      card.Level = level;
      cards[slot] = card;
    }
    return cards;
  }
}