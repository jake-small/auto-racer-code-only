using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class EngineTesting
{
  public static AutoRaceEngine RaceEngine()
  {
    var players = new List<Player>();
    for (var i = 0; i < 4; i++)
    {
      var player = new Player
      {
        Id = i,
        Cards = new Dictionary<int, Card>(),
        Position = 0
      };
      for (var c = 0; c < 5; c++)
      {
        player.Cards.Add(c, GetSampleCard());
      }
      players.Add(player);
    }

    return new AutoRaceEngine(players, 5, 5);
  }

  public static string GetPositionTextView(IEnumerable<PlayerTurnResult> turnResults)
  {
    var positionView = new List<string>{
    "|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_| ",
    "|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_| ",
    "|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_| ",
    "|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_| "
  };
    var p = 0;
    foreach (var turnResult in turnResults)
    {
      var position = turnResult.Player.Position;
      // |_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|
      // 0123456789
      //  0 1 2 3 4 5 6 7 8 9
      var positionOffset = position + position + 1;
      positionView[p] = positionView[p].Remove(positionOffset, 1).Insert(positionOffset, "X");
      p = p + 1;
    }
    var positionViewString = string.Join("\n", positionView);
    GD.Print(positionViewString);
    return positionViewString;
  }

  private static Card GetSampleCard()
  {
    var rnd = new Random();
    int number = rnd.Next(1, 5);
    return new Card
    {
      Name = $"Card {number}",
      Description = $"sample card with base move of {number}",
      BaseMove = number.ToString()
    };
  }
}