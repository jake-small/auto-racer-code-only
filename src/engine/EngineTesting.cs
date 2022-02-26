using System;
using System.Collections.Generic;
using System.Linq;

public static class EngineTesting
{
  public static void Log(string message, bool shouldLog = true)
  {
    // TODO implement logging
    if (shouldLog)
    {
      Console.WriteLine(message);
    }
  }

  public static AutoRaceEngine RaceEngine(Player player1)
  {
    var players = new List<Player>();
    players.Add(player1);
    // Add other players
    players.AddRange(GetBots(3));

    return new AutoRaceEngine(players, 5, 5);
  }

  public static Card GetSampleCard()
  {
    var rnd = new Random();
    int number = rnd.Next(1, 4);
    return new Card
    {
      Name = $"Card {number}",
      Description = $"sample card with base move of {number}",
      BaseMove = number
    };
  }

  public static string GetPositionTextView(IEnumerable<PlayerTurnResult> turnResults)
  {
    var positionView = new List<string>{
    "_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_| ",
    "_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_| ",
    "_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_| ",
    "_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_| "
  };
    var playerNum = 0;
    foreach (var turnResult in turnResults)
    {
      var position = turnResult.Player.Position;
      if (position < 0)
      {
        positionView[playerNum] = positionView[playerNum].Remove(0, 1).Insert(0, position.ToString());
      }
      else if (position * 2 >= positionView.FirstOrDefault().Length)
      {
        var trackLength = positionView[playerNum].Length;
        positionView[playerNum] = positionView[playerNum].Remove(trackLength - 1, 1).Insert(trackLength - 1, position.ToString());
      }
      else
      {
        // _|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|_|
        // 0123456789
        // 0 1 2 3 4 5 6 7 8 9
        var positionOffset = position * 2;
        positionView[playerNum] = positionView[playerNum].Remove(positionOffset, 1).Insert(positionOffset, position.ToString());
      }
      playerNum = playerNum + 1;
    }
    var positionViewString = string.Join("\n", positionView);
    Console.WriteLine(positionViewString);
    return positionViewString;
  }

  private static List<Player> GetBots(int numOpponents)
  {
    var players = new List<Player>();
    for (var i = 1; i < numOpponents + 1; i++)
    {
      players.Add(GetBot(i));
    }
    return players;
  }

  private static Player GetBot(int id)
  {
    return new BotBasic(id, GameManager.RaceNumber);
  }
}