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
    var allTokensGiven = new Dictionary<int, List<Token>>();
    foreach (var playerTurn in PlayerTurns)
    {
      var tokensGiven = playerTurn.TokensGiven;
      foreach (var token in tokensGiven)
      {
        if (allTokensGiven.ContainsKey(token.Key))
        {
          allTokensGiven[token.Key].AddRange(token.Value);
          continue;
        }
        allTokensGiven.Add(token.Key, token.Value.ToList());
      }
    }

    foreach (var playerTokens in allTokensGiven)
    {
      var playerTurn = PlayerTurns.FirstOrDefault(p => p.Player.Id == playerTokens.Key);
      foreach (var token in playerTokens.Value)
      {
        if (token is MoveToken moveToken)
        {
          // TODO handle all move token affects (duration, type)
          playerTurn.Movement = moveToken.Calculate(playerTurn.Movement);
        }
        // TODO deal with all tokens
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