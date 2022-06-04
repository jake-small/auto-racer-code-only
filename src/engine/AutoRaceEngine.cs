using System;
using System.Collections.Generic;
using System.Linq;

public class AutoRaceEngine
{
  private TurnManager _turnManager;
  private IEnumerable<Player> _players;
  private int _raceLength;
  private int _turn;
  private int _currentSlot;
  private readonly int _slotCount;
  private TurnPhases _phase;

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

  public IEnumerable<Player> GetPlayers() => _players;
  public int GetTurn() => _turn;
  public TurnPhases GetTurnPhase() => _phase;

  public Dictionary<int, List<Player>> GetStandings()
  {
    var sortedPlayers = _players.OrderByDescending(p => p.Position).ToList();
    var standings = new Dictionary<int, List<Player>>();
    var placement = 0;
    foreach (var player in sortedPlayers)
    {
      var previousPlacement = placement - 1;
      if (standings.ContainsKey(previousPlacement) && standings[previousPlacement].FirstOrDefault().Position == player.Position)
      {
        standings[previousPlacement].Add(player);
      }
      else
      {
        standings[placement] = new List<Player> { player };
        placement = placement + 1;
      }
    }
    return standings;
  }

  public IEnumerable<PlayerTurnResult> GetTurnResults()
  {
    return _turnManager.PlayerTurns;
  }

  public void Clear()
  {
    _turnManager.ClearPlayerTurns();
    foreach (var player in _players)
    {
      if (GameManager.LocalPlayer.Id != player.Id)
      {
        player.Cards.Clear();
      }
      _players = null;
    }
  }

  public bool AdvanceRace()
  {
    _phase = NextTurnPhase(_phase);
    Console.WriteLine($"engine phase: {_phase}");
    switch (_phase)
    {
      case TurnPhases.Start:
        StartTurnPhase();
        break;
      case TurnPhases.Abilities1:
        Abilities1Phase();
        break;
      case TurnPhases.Abilities2:
        Abilities2Phase();
        break;
      case TurnPhases.Move:
        MovePhase();
        break;
      case TurnPhases.End:
        var noMoreCardSlots = EndTurnPhase();
        if (noMoreCardSlots)
        {
          _phase = TurnPhases.HandleRemainingTokens;
        }
        break;
      case TurnPhases.HandleRemainingTokens:
        var isRaceOver = CalculateRemainingTokens();
        return isRaceOver;
    }
    return false;
  }

  private void StartTurnPhase()
  {
    Console.WriteLine("StartTurnPhase");
    NextTurn();
    Console.WriteLine($"engine turn: {_turn}");
  }

  private void Abilities1Phase()
  {
    Console.WriteLine("Abilities1Phase");
    foreach (var player in _players)
    {
      _turnManager.AddPlayerTurn(CalculatePlayerTurn(player, TurnPhases.Abilities1));
    }
    _turnManager.DistributeTokens();
  }

  private void Abilities2Phase()
  {
    Console.WriteLine("Abilities2Phase");
    _turnManager.ClearPlayerTurns();
    foreach (var player in _players)
    {
      _turnManager.AddPlayerTurn(CalculatePlayerTurn(player, TurnPhases.Abilities2));
    }
    _turnManager.DistributeTokens();
  }

  private void MovePhase()
  {
    Console.WriteLine("MovePhase");
    _turnManager.ApplyTokens();
    _turnManager.UpdatePositions();
  }

  private bool EndTurnPhase()
  {
    Console.WriteLine("EndTurnPhase");
    _turnManager.ClearPlayerTurns();
    if (_turn >= _raceLength)
    {
      Console.WriteLine("No more card slots");
      return true;
    }
    return false;
  }

  private bool CalculateRemainingTokens()
  {
    Console.WriteLine("Calculating Remaining Tokens");
    _turn = _turn + 1;
    _turnManager.ClearPlayerTurns();
    foreach (var player in _players)
    {
      _turnManager.AddPlayerTurn(new PlayerTurnResult(player));
    }
    _turnManager.DistributeTokens();
    var noRemainingTokens = _turnManager.ApplyTokens();
    _turnManager.UpdatePositions();
    return noRemainingTokens;
  }

  private TurnPhases NextTurnPhase(TurnPhases phase)
  {
    if (phase == TurnPhases.HandleRemainingTokens)
    {
      return TurnPhases.HandleRemainingTokens;
    }
    var nextPhaseInt = (int)phase + 1;
    if (nextPhaseInt > (int)TurnPhases.End)
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

  private PlayerTurnResult CalculatePlayerTurn(Player player, TurnPhases currentPhase)
  {
    var result = new PlayerTurnResult(player);
    result.Phase = currentPhase;
    if (player.Cards.TryGetValue(_currentSlot, out var card))
    {
      result.Movement = card.BaseMove;
    }
    result.TokensGiven = CalculateTokensGiven(card, player, currentPhase);
    return result;
  }

  private Dictionary<int, List<Token>> CalculateTokensGiven(Card card, Player player, TurnPhases currentPhase)
  {
    var leveledCard = card.GetLeveledCard();
    var calculatedCard = leveledCard.ApplyTokenFunctionValues(player, _players);
    var tokenAbilities = calculatedCard.Abilities?.MoveTokenAbilities
      .Where(a => a.Phase.Equals(currentPhase.ToString(), StringComparison.InvariantCultureIgnoreCase)).ToList() ?? new List<MoveTokenAbility>();
    var tokensGiven = CalculateTokens(tokenAbilities, player);
    return tokensGiven;
  }

  private Dictionary<int, List<Token>> CalculateTokens(List<MoveTokenAbility> tokenAbilities, Player player)
  {
    var shuffledPlayers = _players.ToList().Shuffle().ToList();
    var tokensGiven = new Dictionary<int, List<Token>>();
    foreach (var tokenAbility in tokenAbilities)
    {
      if (tokenAbility is MoveTokenAbility)
      {
        var moveTokensGiven = GetMoveToken(tokenAbility, player, shuffledPlayers);
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

  private Dictionary<int, List<Token>> GetMoveToken(TokenAbility tokenAbility, Player player, List<Player> shuffledPlayers)
  {
    if (!(tokenAbility is MoveTokenAbility moveTokenAbililty))
    {
      Console.WriteLine("Error: Can't get Move Token from tokenAbility as it's not a MoveTokenAbility");
      return new Dictionary<int, List<Token>>();
    }

    var targets = GetTargets(moveTokenAbililty, player, shuffledPlayers);
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
        CreatedBy = player.Id,
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

  private IEnumerable<int> GetTargets(TokenAbility tokenAbility, Player player, List<Player> shuffledPlayers)
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

    var targets = new HashSet<Player>();
    if (checkForward)
    {
      var minRangeForward = player.Position + tokenTarget.Range.Min.ToInt();
      var maxRangeForward = player.Position + tokenTarget.Range.Max.ToInt();
      foreach (var eachPlayer in shuffledPlayers)
      {
        if (tokenTarget.GetTargetType() == TargetType.Others && eachPlayer.Id == player.Id)
        {
          continue;
        }
        if (eachPlayer.Position >= minRangeForward && eachPlayer.Position <= maxRangeForward)
        {
          targets.Add(eachPlayer);
        }
      }
    }
    if (checkBack)
    {
      var minRangeBack = player.Position - tokenTarget.Range.Min.ToInt();
      var maxRangeBack = player.Position - tokenTarget.Range.Max.ToInt();
      foreach (var eachPlayer in shuffledPlayers)
      {
        if (tokenTarget.GetTargetType() == TargetType.Others && eachPlayer.Id == player.Id)
        {
          continue;
        }
        if (eachPlayer.Position >= maxRangeBack && eachPlayer.Position <= minRangeBack)
        {
          targets.Add(eachPlayer);
        }
      }
    }

    if (targets == null || targets.Count == 0)
    {
      return new List<int>();
    }

    var sortedTargets = targets.ToList();
    switch (tokenTarget.GetPriority())
    {
      case Priority.Closest:
        sortedTargets.Sort((x, y) => Math.Abs(x.Position - player.Position).CompareTo(Math.Abs(y.Position - player.Position)));
        break;
      case Priority.Furthest:
        sortedTargets.Sort((x, y) => Math.Abs(y.Position - player.Position).CompareTo(Math.Abs(x.Position - player.Position)));
        break;
      case Priority.PositionAscending:
        sortedTargets = sortedTargets.OrderBy(p => p.Position).ToList();
        break;
      case Priority.PositionDescending:
        sortedTargets = sortedTargets.OrderByDescending(p => p.Position).ToList();
        break;
    }
    return sortedTargets.Select(p => p.Id).Take(tokenTarget.Amount.ToInt());
  }
}

public enum TurnPhases
{
  PreRace = -1,
  Start = 0,
  Abilities1 = 1,
  AbilitiesP0 = 2,
  AbilitiesP1 = 3,
  AbilitiesP2 = 4,
  AbilitiesP3 = 5,
  Abilities2 = 6,
  Abilities2P0 = 7,
  Abilities2P1 = 8,
  Abilities2P2 = 9,
  Abilities2P3 = 10,
  Move = 11,
  End = 12,
  HandleRemainingTokens = -2
}