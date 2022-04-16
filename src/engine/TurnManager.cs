using System;
using System.Collections.Generic;
using System.Linq;

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

  public void DistributeTokens()
  {
    // TODO combine these loops to reduce iteration
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
        playerTurn.Player.Tokens.Add(token);
      }
    }
  }

  public bool ApplyTokens()
  {
    var noRemainingTokens = true;
    foreach (var playerTurn in PlayerTurns)
    {
      foreach (var token in playerTurn.Player.Tokens)
      {
        ApplyToken(playerTurn, token);
      }
      playerTurn.Player.Tokens.RemoveAll(d => d.Duration <= 0);
      if (playerTurn.Player.Tokens.Any())
      {
        noRemainingTokens = false;
      }
    }

    return noRemainingTokens;
  }

  private void ApplyToken(PlayerTurnResult playerTurn, Token token)
  {
    if (token is MoveToken moveToken)
    {
      playerTurn.Movement = moveToken.Calculate(playerTurn.Movement);
    }
    token.Duration = token.Duration - 1;
  }

  public void UpdatePositions()
  {
    foreach (var playerTurn in PlayerTurns)
    {
      playerTurn.Player.Position = playerTurn.Player.Position + playerTurn.Movement;
      Console.WriteLine($"player {playerTurn.Player.Id} moves {playerTurn.Movement}");
    }
  }
}