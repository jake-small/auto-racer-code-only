using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AutoRaceEngine
{
  private TurnManager _turnManager;
  private IEnumerable<Player> _players;
  private int _raceLength;
  private int _turn;
  private int _currentSlot;
  private readonly int _slotCount;
  private TurnPhases _phase;
  public enum TurnPhases
  {
    PreRace = -1,
    Start = 0,
    Abilities1 = 1,
    Move = 2,
    Abilities2 = 3,
    End = 4,
  }

  public AutoRaceEngine(IEnumerable<Player> players, int raceLength, int slotCount)
  {
    _players = players;
    _raceLength = raceLength;
    _slotCount = slotCount;
    _turn = 0;
    _currentSlot = -1;
    _phase = TurnPhases.PreRace;
    _turnManager = new TurnManager();
  }

  public int GetTurn() => _turn;
  public TurnPhases GetTurnPhase() => _phase;

  public IEnumerable<PlayerTurnResult> GetTurnResults()
  {
    return _turnManager.PlayerTurns;
  }

  public bool AdvanceRace()
  {
    _phase = NextTurnPhase(_phase);
    GD.Print($"engine phase: {_phase}");
    switch (_phase)
    {
      case TurnPhases.Start:
        StartTurnPhase();
        break;
      case TurnPhases.Abilities1:
        Abilities1Phase();
        break;
      case TurnPhases.Move:
        MovePhase();
        break;
      case TurnPhases.Abilities2:
        Abilities2Phase();
        break;
      case TurnPhases.End:
        var isRaceOver = EndTurnPhase();
        return isRaceOver;
    }
    return false;
  }

  private void StartTurnPhase()
  {
    GD.Print("StartTurnPhase");
    NextTurn();
    GD.Print($"engine turn: {_turn}");
  }

  private void Abilities1Phase()
  {
    GD.Print("Abilities1Phase");
    GD.Print($"number of players: {_players.Count()}");
    foreach (var player in _players)
    {
      _turnManager.AddPlayerTurn(CalculatePlayerTurn(player));
    }
    _turnManager.ApplyTokensGiven();
  }

  private void MovePhase()
  {
    GD.Print("MovePhase");
    _turnManager.UpdatePositions();
  }

  private void Abilities2Phase()
  {
    GD.Print("Abilities2Phase");
  }

  private bool EndTurnPhase()
  {
    GD.Print("EndTurnPhase");
    _turnManager.ClearPlayerTurns();
    if (_turn >= _raceLength)
    {
      GD.Print("Race is over");
      return true;
    }
    return false;
  }

  private TurnPhases NextTurnPhase(TurnPhases phase)
  {
    var nextPhaseInt = (int)phase + 1;
    if (nextPhaseInt > 4)
    {
      nextPhaseInt = 0;
    }
    return (TurnPhases)nextPhaseInt;
  }

  private void NextTurn()
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
    var result = new PlayerTurnResult(player);
    if (player.Cards.TryGetValue(_currentSlot, out var card))
    {
      result.Movement = CalculateBaseMovement(card);
    }
    result.TokensGiven = CalculateTokensGiven(card);
    return result;
  }

  private int CalculateBaseMovement(Card card)
  {
    if (!Int32.TryParse(card.GetBaseMove(), out var baseMove))
    {
      return 0;
    }
    return baseMove;
  }

  private Dictionary<int, List<int>> CalculateTokensGiven(Card card)
  {
    var leveledCard = CalculationLayer.ApplyLevelValues(card);
    var calculatedCard = CalculationLayer.ApplyFunctionValues(leveledCard);
    var tokenAbilities = calculatedCard.Abilities?.TokenAbilities ?? new List<TokenAbility>();
    var tokensGiven = CalculateTokens(tokenAbilities);
    return tokensGiven;
  }

  private Dictionary<int, List<int>> CalculateTokens(List<TokenAbility> tokenAbilities)
  {
    var tokensGiven = new Dictionary<int, List<int>>();
    foreach (var tokenAbility in tokenAbilities)
    {
      // TODO convert abilities to tokens given
      // 1. get targets

      // 2. get value
      // 3. get duration
      // 4. create tokens
    }
    return tokensGiven;
  }
}