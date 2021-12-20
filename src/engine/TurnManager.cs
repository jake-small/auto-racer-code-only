using System.Collections.Generic;
using System.Linq;
using Godot;

public class TurnManager
{
  public List<PlayerTurnResult> PlayerTurns { get; private set; }

  public TurnManager()
  {
    PlayerTurns = new List<PlayerTurnResult>();
  }

  public void AddPlayerTurn(PlayerTurnResult playerTurn)
  {
    PlayerTurns.Add(playerTurn);
  }

  public void ClearPlayerTurns()
  {
    PlayerTurns.Clear();
  }

  public void ApplyTokensGiven()
  {
    var allTokensGiven = new Dictionary<int, List<int>>();
    foreach (var playerTurn in PlayerTurns)
    {
      var tokensGiven = playerTurn.TokensGiven;
      foreach (var token in tokensGiven)
      {
        if (allTokensGiven.ContainsKey(token.Key))
        {
          allTokensGiven[token.Key].Add(token.Value);
          continue;
        }
        allTokensGiven.Add(token.Key, new List<int>(token.Value));
      }
    }

    foreach (var playerTokens in allTokensGiven)
    {
      var playerTurn = PlayerTurns.FirstOrDefault(p => p.Player.Id == playerTokens.Key);
      foreach (var token in playerTokens.Value)
      {
        playerTurn.Movement = playerTurn.Movement + token;
      }
    }
  }

  public void UpdatePositions()
  {
    foreach (var playerTurn in PlayerTurns)
    {
      playerTurn.Player.Position = playerTurn.Player.Position + playerTurn.Movement;
      GD.Print($"player {playerTurn.Player.Id} moves {playerTurn.Movement}");
    }
  }
}