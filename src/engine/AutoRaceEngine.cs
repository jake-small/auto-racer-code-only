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
    result.TokensGiven = CalculateTokensGiven(card, player);
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

  private Dictionary<int, List<Token>> CalculateTokensGiven(Card card, Player player)
  {
    var leveledCard = CalculationLayer.ApplyLevelValues(card);
    var calculatedCard = CalculationLayer.ApplyFunctionValues(leveledCard);
    var tokenAbilities = calculatedCard.Abilities?.MoveTokenAbilities ?? new List<MoveTokenAbility>();
    var tokensGiven = CalculateTokens(tokenAbilities, player);
    return tokensGiven;
  }

  private Dictionary<int, List<Token>> CalculateTokens(List<MoveTokenAbility> tokenAbilities, Player player)
  {
    var tokensGiven = new Dictionary<int, List<Token>>();
    foreach (var tokenAbility in tokenAbilities)
    {
      if (tokenAbility is MoveTokenAbility)
      {
        var moveTokensGiven = GetMoveToken(tokenAbility, player);
        tokensGiven.MergeDictionaries(moveTokensGiven);
      }
      // TODO implement ShieldTokenAbility
      // else if (tokenAbility is ShieldTokenAbility)
      // {
      //   var shieldTokensGiven = GetShieldToken(tokenAbility, player);
      //   tokensGiven.MergeDictionaries(shieldTokensGiven);
      // }
    }
    return tokensGiven;
  }

  private Dictionary<int, List<Token>> GetMoveToken(TokenAbility tokenAbility, Player player)
  {
    if (!(tokenAbility is MoveTokenAbility moveTokenAbililty))
    {
      GD.Print("Error: Can't get Move Token from tokenAbility as it's not a MoveTokenAbility");
      return new Dictionary<int, List<Token>>();
    }


    // 1. get targets
    var targets = GetTargets(moveTokenAbililty, player);
    // 2. get value
    // 3. get duration
    // 4. create tokens

    var tokensGiven = new Dictionary<int, List<Token>>();
    foreach (var target in targets)
    {
      MoveTokenType moveTokenType;
      if (!(Enum.TryParse<MoveTokenType>(moveTokenAbililty.Type, out moveTokenType)))
      {
        moveTokenType = MoveTokenType.Additive;
      }
      var token = new MoveToken
      {
        Duration = moveTokenAbililty.Duration.ToInt(),
        Type = moveTokenType,
        Target = target,
        Value = moveTokenAbililty.Value.ToInt()
      };

      if (tokensGiven.ContainsKey(target))
      {
        tokensGiven[target].Add(token);
      }
      else
      {
        tokensGiven.Add(target, new List<Token> { token });
      }
    }
    return tokensGiven;
  }

  private Dictionary<int, List<Token>> GetShieldToken(TokenAbility tokenAbility, Player player)
  {
    var tokensGiven = new Dictionary<int, List<Token>>();
    return tokensGiven;
  }

  private IEnumerable<int> GetTargets(TokenAbility tokenAbility, Player player)
  {
    var tokenTarget = tokenAbility.Target;
    if (tokenTarget.GetTargetType() == TargetType.Self)
    {
      return new List<int> { player.Id };
    }
    if (tokenTarget.Amount.ToInt() == 0)
    {
      return new List<int>();
    }

    var checkForward = false;
    var checkBack = false;
    switch (tokenTarget.GetDirection())
    {
      case Direction.Forward:
        checkForward = true;
        break;
      case Direction.Backward:
        checkBack = true;
        break;
      case Direction.Any:
        checkForward = true;
        checkBack = true;
        break;
    }

    // TODO handle type: self, others, all
    // TODO don't add player more than once to list of targets
    var targets = new List<Player>();
    if (checkForward)
    {
      var minRangeForward = player.Position + tokenTarget.Range.Min.ToInt();
      var maxRangeForward = player.Position + tokenTarget.Range.Max.ToInt();
      foreach (var otherPlayer in _players)
      {
        if (otherPlayer.Position >= minRangeForward && otherPlayer.Position <= maxRangeForward)
        {
          targets.Add(otherPlayer);
        }
      }
    }
    if (checkBack)
    {
      var minRangeBack = player.Position - tokenTarget.Range.Min.ToInt();
      var maxRangeBack = player.Position - tokenTarget.Range.Max.ToInt();
      foreach (var otherPlayer in _players)
      {
        if (otherPlayer.Position >= maxRangeBack && otherPlayer.Position <= minRangeBack)
        {
          targets.Add(otherPlayer);
        }
      }
    }

    if (targets == null)
    {
      return new List<int>();
    }

    switch (tokenTarget.GetPriority())
    {
      case Priority.Closest:
        targets.Sort((x, y) => Math.Abs(x.Position - player.Position).CompareTo(Math.Abs(y.Position - player.Position)));
        break;
      case Priority.Furthest:
        targets.Sort((x, y) => Math.Abs(y.Position - player.Position).CompareTo(Math.Abs(x.Position - player.Position)));
        break;
      case Priority.PositionAscending:
        targets = targets.OrderBy(p => p.Position).ToList();
        break;
      case Priority.PositionDescending:
        targets = targets.OrderByDescending(p => p.Position).ToList();
        break;
    }

    return targets.Select(p => p.Position).Take(tokenTarget.Amount.ToInt());
  }
}