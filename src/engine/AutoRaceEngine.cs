using System;
using System.Collections.Generic;
using System.Linq;

public class AutoRaceEngine
{
  // take in list of players (including their cards) and number of turns
  // simulate X number of turns, returning state for each turn
  //   - start with base move 1
  // print out state of each turn for debugging purposes

  private IEnumerable<Player> _players;
  private int _raceLength;
  private int _turn;
  private int _currentSlot;
  private readonly int _slotCount;
  private readonly int _finishLine;

  public AutoRaceEngine(IEnumerable<Player> players, int raceLength, int slotCount, int finishLine)
  {
    _players = players;
    _raceLength = raceLength;
    _slotCount = slotCount;
    _finishLine = finishLine;
    _turn = 0;
    _currentSlot = -1;
  }

  public IEnumerable<PlayerTurnResult> NextTurn()
  {
    IncrementTurn();
    var turnManager = new TurnManager();
    foreach (var player in _players)
    {
      turnManager.AddPlayerTurn(CalculatePlayerTurn(player));
    }

    turnManager.ApplyTokensGiven();
    // TODO: update ui showing tokens given
    turnManager.UpdatePositions();
    // TODO: update ui showing characters moving
    return turnManager.PlayerTurns;
  }

  private void IncrementTurn()
  {
    _turn = _turn + 1;
    _currentSlot = _currentSlot + 1;
    if (_currentSlot + 1 > _slotCount)
    {
      _currentSlot = 0;
    }
  }

  private PlayerTurnResult CalculatePlayerTurn(Player player)
  {
    var result = new PlayerTurnResult();
    if (player.Cards.TryGetValue(_currentSlot, out var card))
    {
      result.Movement = CalculateMovement(card);
    }
    return result;
  }

  private int CalculateMovement(Card card)
  {
    if (!Int32.TryParse(card.BaseMove, out var baseMove))
    {
      return 0;
    }

    // TODO: apply abilities

    return baseMove;
  }
}