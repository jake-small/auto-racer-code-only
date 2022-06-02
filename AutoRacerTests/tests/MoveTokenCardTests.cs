using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using NUnit.Framework;

namespace AutoRacerTests.Tests
{
  [TestFixture]
  public class MoveTokenCardTests
  {
    private List<Card> _cards;

    [SetUp]
    public void SetUp()
    {
      // Have to register types again because of a bug between unit tests and MoonSharp
      UserData.RegisterType<RaceScriptData>();
      UserData.RegisterType<MoonSharpPlayer>();
      UserData.RegisterType<MoonSharpMoveTokens>();
      var cardLoader = new CardLoader(@"G:\JakeDoc\Files\Projects\Godot\AutoRacer\auto-racer\configs\cardDataTest.json", new JsonLoader());
      _cards = cardLoader.GetCards();

    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Curse_SameSpace_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(Curse_SameSpace_Success)}({level})");
      var curseCard = _cards.FirstOrDefault(c => c.GetRawName().Equals("curse", StringComparison.InvariantCultureIgnoreCase));
      curseCard.Level = level;
      var player = new Player { Id = 0, Cards = new Dictionary<int, Card>() { { 0, curseCard } }, Position = 0 };
      var playerResults = TestRace(player, 1);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var leveledCard = curseCard.GetLeveledCard();
      foreach (var playerResult in playerResults)
      {
        if (playerResult.Id is 0)
        {
          Assert.That(playerResult.Position, Is.EqualTo(leveledCard.BaseMove));
          continue;
        }
        Assert.That(playerResult.Position, Is.EqualTo(1 + leveledCard.Abilities.MoveTokenAbilities.FirstOrDefault().Value.ToInt()));
      }
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void LingeringCurse_SameSpace_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(LingeringCurse_SameSpace_Success)}({level})");
      var curseCard = _cards.FirstOrDefault(c => c.GetRawName().Equals("lingering curse", StringComparison.InvariantCultureIgnoreCase));
      curseCard.Level = level;
      var player = new Player { Id = 0, Cards = new Dictionary<int, Card>() { { 0, curseCard } }, Position = 0 };
      var playerResults = TestRace(player, 1);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var leveledCard = curseCard.GetLeveledCard();
      var tokenValue = leveledCard.Abilities.MoveTokenAbilities.Select(t => t.Value.ToInt()).Sum();
      var cursedPlayers = playerResults.Where(p => p.Position == 1 + tokenValue);
      Assert.That(cursedPlayers.Count, Is.EqualTo(1));
      Assert.That(cursedPlayers.FirstOrDefault().Id, Is.Not.EqualTo(0));
      Assert.That(playerResults.FirstOrDefault(p => p.Id == 0).Position, Is.EqualTo(leveledCard.BaseMove));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void LingeringCurse_EdgeOfRange_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(LingeringCurse_EdgeOfRange_Success)}({level})");
      var curseCard = _cards.FirstOrDefault(c => c.GetRawName().Equals("lingering curse", StringComparison.InvariantCultureIgnoreCase));
      curseCard.Level = level;
      var player = new Player { Id = 0, Cards = new Dictionary<int, Card>() { { 0, curseCard } }, Position = 0 };
      var leveledCard = curseCard.GetLeveledCard();
      var maxRange = leveledCard.Abilities.MoveTokenAbilities.Select(a => a.Target.Range.Max.ToInt()).Max();
      var playerResults = TestRace(player, 1, maxRange);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var tokenValue = leveledCard.Abilities.MoveTokenAbilities.Select(t => t.Value.ToInt()).Sum();
      var cursedPlayers = playerResults.Where(p => p.Position - maxRange == 1 + tokenValue);
      Assert.That(cursedPlayers.Count, Is.EqualTo(1));
      Assert.That(playerResults.FirstOrDefault(p => p.Id == 0).Position, Is.EqualTo(leveledCard.BaseMove));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void LingeringCurse_OutOfRange_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(LingeringCurse_OutOfRange_Success)}({level})");
      var curseCard = _cards.FirstOrDefault(c => c.GetRawName().Equals("lingering curse", StringComparison.InvariantCultureIgnoreCase));
      curseCard.Level = level;
      var player = new Player { Id = 0, Cards = new Dictionary<int, Card>() { { 0, curseCard } }, Position = 0 };
      var leveledCard = curseCard.GetLeveledCard();
      var maxRange = leveledCard.Abilities.MoveTokenAbilities.Select(a => a.Target.Range.Max.ToInt()).Max();
      var playerResults = TestRace(player, 1, maxRange + 1);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var tokenValue = leveledCard.Abilities.MoveTokenAbilities.Select(t => t.Value.ToInt()).Sum();
      var cursedPlayers = playerResults.Where(p => p.Position - maxRange == 1 + tokenValue);
      Assert.That(cursedPlayers.Count, Is.EqualTo(0));
      Assert.That(playerResults.FirstOrDefault(p => p.Id == 0).Position, Is.EqualTo(leveledCard.BaseMove));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void LingeringCurse_TargetClosestAhead_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(LingeringCurse_TargetClosestBehind_Success)}({level})");
      var curseCard = _cards.FirstOrDefault(c => c.GetRawName().Equals("lingering curse", StringComparison.InvariantCultureIgnoreCase));
      curseCard.Level = level;
      var player0 = new Player { Id = 0, Cards = new Dictionary<int, Card>(), Position = 3 };
      var player1 = new Player { Id = 1, Cards = new Dictionary<int, Card>() { { 0, curseCard } }, Position = 6 };
      var player2 = new Player { Id = 2, Cards = new Dictionary<int, Card>(), Position = 9 };
      var player3 = new Player { Id = 3, Cards = new Dictionary<int, Card>(), Position = 8 };

      var leveledCard = curseCard.GetLeveledCard();
      var playerResults = TestRace(new List<Player> { player0, player1, player2, player3 }, 1);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var tokenValue = leveledCard.Abilities.MoveTokenAbilities.Select(t => t.Value.ToInt()).Sum();
      var cursedPlayers = playerResults.Where(p => p.Id == 3);
      Assert.That(cursedPlayers.Count, Is.EqualTo(1));
      Assert.That(cursedPlayers.FirstOrDefault().Position - 8, Is.EqualTo(1 + tokenValue));
      Assert.That(playerResults.FirstOrDefault(p => p.Id == 1).Position, Is.EqualTo(leveledCard.BaseMove + 6));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void LingeringCurse_TargetClosestBehind_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(LingeringCurse_TargetClosestBehind_Success)}({level})");
      var curseCard = _cards.FirstOrDefault(c => c.GetRawName().Equals("lingering curse", StringComparison.InvariantCultureIgnoreCase));
      curseCard.Level = level;
      var player0 = new Player { Id = 0, Cards = new Dictionary<int, Card>(), Position = 4 };
      var player1 = new Player { Id = 1, Cards = new Dictionary<int, Card>() { { 0, curseCard } }, Position = 6 };
      var player2 = new Player { Id = 2, Cards = new Dictionary<int, Card>(), Position = 5 };
      var player3 = new Player { Id = 3, Cards = new Dictionary<int, Card>(), Position = 8 };

      var leveledCard = curseCard.GetLeveledCard();
      var playerResults = TestRace(new List<Player> { player0, player1, player2, player3 }, 1);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var tokenValue = leveledCard.Abilities.MoveTokenAbilities.Select(t => t.Value.ToInt()).Sum();
      var cursedPlayers = playerResults.Where(p => p.Id == 2);
      Assert.That(cursedPlayers.Count, Is.EqualTo(1));
      Assert.That(cursedPlayers.FirstOrDefault().Position - 5, Is.EqualTo(1 + tokenValue));
      Assert.That(playerResults.FirstOrDefault(p => p.Id == 1).Position, Is.EqualTo(leveledCard.BaseMove + 6));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void EscapeRune_SameSpace_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(EscapeRune_SameSpace_Success)}({level})");
      var escapeCard = _cards.FirstOrDefault(c => c.GetRawName().Equals("escape rune", StringComparison.InvariantCultureIgnoreCase));
      escapeCard.Level = level;
      var player = new Player { Id = 0, Cards = new Dictionary<int, Card>() { { 0, escapeCard } }, Position = 0 };
      var playerResults = TestRace(player, 1);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var leveledCard = escapeCard.GetLeveledCard();
      foreach (var playerResult in playerResults)
      {
        Console.WriteLine($"id: {playerResult.Id} pos: {playerResult.Position}");
        if (playerResult.Id is 0)
        {
          var escapeValue = leveledCard.LevelValues.FirstOrDefault(l => l.Id == level).OutKeys.FirstOrDefault().Value.ToInt();
          Assert.That(playerResult.Position, Is.EqualTo(leveledCard.BaseMove + (3 * escapeValue)));
          continue;
        }
        Assert.That(playerResult.Position, Is.EqualTo(1));
      }
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void StaminaElixir_SameSpace_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(StaminaElixir_SameSpace_Success)}({level})");
      var elixirCard = _cards.FirstOrDefault(c => c.GetRawName().Equals("stamina elixir", StringComparison.InvariantCultureIgnoreCase));
      elixirCard.Level = level;
      var player = new Player { Id = 0, Cards = new Dictionary<int, Card>() { { 0, elixirCard } }, Position = 0 };
      var playerResults = TestRace(player, 8);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var leveledCard = elixirCard.GetLeveledCard();
      foreach (var playerResult in playerResults)
      {
        Console.WriteLine($"id: {playerResult.Id} pos: {playerResult.Position}");
        if (playerResult.Id is 0)
        {
          var elixirValue = leveledCard
            .LevelValues.FirstOrDefault(l => l.Id == level)
            .OutKeys.FirstOrDefault(k => k.Key == "D").Value.ToInt();
          Assert.That(playerResult.Position, Is.EqualTo(leveledCard.BaseMove + elixirValue + 4));
          continue;
        }
        Assert.That(playerResult.Position, Is.EqualTo(5));
      }
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void DragonPlate_NoPositiveTokens_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(DragonPlate_NoPositiveTokens_Success)}({level})");
      var cardInTest = _cards.FirstOrDefault(c => c.GetRawName().Equals("dragon plate", StringComparison.InvariantCultureIgnoreCase));
      cardInTest.Level = level;
      var player = new Player { Id = 0, Cards = new Dictionary<int, Card>() { { 0, cardInTest } }, Position = 0 };
      var playerResults = TestRace(player, 1);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var leveledCard = cardInTest.GetLeveledCard();
      foreach (var playerResult in playerResults)
      {
        Console.WriteLine($"id: {playerResult.Id} pos: {playerResult.Position}");
        if (playerResult.Id is 0)
        {
          var boostValue = leveledCard
            .LevelValues.FirstOrDefault(l => l.Id == level)
            .OutKeys.FirstOrDefault(k => k.Key == "M").Value.ToInt();
          Assert.That(playerResult.Position, Is.EqualTo(leveledCard.BaseMove + boostValue));
          continue;
        }
        Assert.That(playerResult.Position, Is.EqualTo(1));
      }
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void DragonPlate_HasPositiveTokens_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(DragonPlate_HasPositiveTokens_Success)}({level})");
      var cardInTest = _cards.FirstOrDefault(c => c.GetRawName().Equals("dragon plate", StringComparison.InvariantCultureIgnoreCase));
      cardInTest.Level = level;
      var plusOneToken = TestHelperData.GetTestMoveToken();
      var player = new Player
      {
        Id = 0,
        Cards = new Dictionary<int, Card>() { { 0, cardInTest } },
        Position = 0,
        Tokens = new List<Token> { plusOneToken }
      };
      var playerResults = TestRace(player, 1);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var leveledCard = cardInTest.GetLeveledCard();
      foreach (var playerResult in playerResults)
      {
        Console.WriteLine($"id: {playerResult.Id} pos: {playerResult.Position}");
        if (playerResult.Id is 0)
        {
          Assert.That(playerResult.Position, Is.EqualTo(leveledCard.BaseMove + plusOneToken.Value));
          continue;
        }
        Assert.That(playerResult.Position, Is.EqualTo(1));
      }
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void DragonPlate_HasNegativeTokens_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(DragonPlate_HasNegativeTokens_Success)}({level})");
      var cardInTest = _cards.FirstOrDefault(c => c.GetRawName().Equals("dragon plate", StringComparison.InvariantCultureIgnoreCase));
      cardInTest.Level = level;
      var minusOneToken = TestHelperData.GetTestMoveToken(-1);
      var player = new Player
      {
        Id = 0,
        Cards = new Dictionary<int, Card>() { { 0, cardInTest } },
        Position = 0,
        Tokens = new List<Token> { minusOneToken }
      };
      var playerResults = TestRace(player, 1);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var leveledCard = cardInTest.GetLeveledCard();
      foreach (var playerResult in playerResults)
      {
        Console.WriteLine($"id: {playerResult.Id} pos: {playerResult.Position}");
        if (playerResult.Id is 0)
        {
          var boostValue = leveledCard
            .LevelValues.FirstOrDefault(l => l.Id == level)
            .OutKeys.FirstOrDefault(k => k.Key == "M").Value.ToInt();
          Assert.That(playerResult.Position, Is.EqualTo(leveledCard.BaseMove + boostValue + minusOneToken.Value));
          continue;
        }
        Assert.That(playerResult.Position, Is.EqualTo(1));
      }
    }

    [TestCase(1, -1, 2, -1, 2)]
    [TestCase(2, -1, 2, -1, 2)]
    [TestCase(3, -1, 2, -1, 2)]
    [TestCase(1, -2, 2, -1, 2)]
    [TestCase(2, -2, 2, -1, 2)]
    [TestCase(3, -2, 2, -1, 2)]
    [TestCase(1, -1, 4, -2, 1)]
    [TestCase(2, -1, 4, -2, 1)]
    [TestCase(3, -1, 4, -2, 1)]
    public void LastingShield_HasNegativeTokens_Success(int level, int token1Value, int token1Duration, int token2Value, int token2Duration)
    {
      Console.WriteLine($"Starting {nameof(LastingShield_HasNegativeTokens_Success)}({level})");
      var cardInTest = _cards.FirstOrDefault(c => c.GetRawName().Equals("lasting shield", StringComparison.InvariantCultureIgnoreCase));
      cardInTest.Level = level;
      var minusToken1 = TestHelperData.GetTestMoveToken(token1Value, token1Duration);
      var minusToken2 = TestHelperData.GetTestMoveToken(token2Value, token2Duration);
      var player = new Player
      {
        Id = 0,
        Cards = new Dictionary<int, Card>() { { 0, cardInTest } },
        Position = 0,
        Tokens = new List<Token> { minusToken1, minusToken2 }
      };
      var playerResults = TestRace(player, 40);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var leveledCard = cardInTest.GetLeveledCard();
      foreach (var playerResult in playerResults)
      {
        Console.WriteLine($"id: {playerResult.Id} pos: {playerResult.Position}");
        if (playerResult.Id is 0)
        {
          var boostValue = leveledCard
            .LevelValues.FirstOrDefault(l => l.Id == level)
            .OutKeys.FirstOrDefault(k => k.Key == "M").Value.ToInt();
          var newPosition = 4 + leveledCard.BaseMove
            + (boostValue * (token1Duration + token2Duration))
            + ((token1Value * token1Duration) + (token2Value * token2Duration));
          Console.WriteLine($"boostvalue: {boostValue}, newposition: {newPosition}");
          Console.WriteLine($"boost: {(boostValue * (minusToken1.Duration + minusToken2.Duration))}");
          Console.WriteLine($"minus: {((minusToken1.Value * minusToken1.Duration) + (minusToken2.Value * minusToken2.Duration))}");
          Assert.That(playerResult.Position, Is.EqualTo(newPosition));
          continue;
        }
        Assert.That(playerResult.Position, Is.EqualTo(5));
      }
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void FocusRing_HasPositiveTokens_Success(int level)
    {
      Console.WriteLine($"Starting {nameof(FocusRing_HasPositiveTokens_Success)}({level})");
      var cardInTest = _cards.FirstOrDefault(c => c.GetRawName().Equals("focus ring", StringComparison.InvariantCultureIgnoreCase));
      cardInTest.Level = level;
      var plusToken = TestHelperData.GetTestMoveToken(1, 1);
      var player = new Player
      {
        Id = 0,
        Cards = new Dictionary<int, Card>() { { 0, cardInTest } },
        Position = 0,
        Tokens = new List<Token> { plusToken, plusToken }
      };
      var playerResults = TestRace(player, 1);
      Assert.That(playerResults.Count, Is.EqualTo(4));
      var leveledCard = cardInTest.GetLeveledCard();
      foreach (var playerResult in playerResults)
      {
        Console.WriteLine($"id: {playerResult.Id} pos: {playerResult.Position}");
        if (playerResult.Id is 0)
        {
          var boostValue = leveledCard
            .LevelValues.FirstOrDefault(l => l.Id == level)
            .OutKeys.FirstOrDefault(k => k.Key == "A").Value.ToInt();
          var newPosition = boostValue * (leveledCard.BaseMove + plusToken.Value + plusToken.Value);
          Assert.That(playerResult.Position, Is.EqualTo(newPosition));
          continue;
        }
        Assert.That(playerResult.Position, Is.EqualTo(1));
      }
    }



    private List<Player> TestRace(Player player, int numTurns, int defaultPosition = 0, int defaultBasemove = 1)
    {
      return TestRace(new List<Player> { player }, numTurns, defaultPosition, defaultBasemove);
    }
    private List<Player> TestRace(List<Player> players, int numTurns, int defaultPosition = 0, int defaultBasemove = 1)
    {
      for (int i = 0; i < GameData.NumPlayers; i++)
      {
        var player = players.ElementAtOrDefault(i);
        if (player is null)
        {
          player = new Player { Id = i, Cards = new Dictionary<int, Card>(), Position = defaultPosition };
          player.Cards = FillEmptyCardSlots(player.Cards, 1);
          players.Add(player);
        }
        else
        {
          player.Cards = FillEmptyCardSlots(player.Cards, 1);
        }
      }
      Console.WriteLine($"players pos: {string.Join(", ", players.Select(p => p.Position))}");
      var engine = new AutoRaceEngine(players, 5, 5);
      var turn = 0;
      while (turn < numTurns)
      {
        var isRaceOver = engine.AdvanceRace();
        if (isRaceOver)
        {
          break;
        }
        if (engine.GetTurnPhase() is TurnPhases.End)
        {
          turn = turn + 1;
        }
      }
      return engine.GetStandings().SelectMany(s => s.Value).ToList();
    }

    private Dictionary<int, Card> FillEmptyCardSlots(Dictionary<int, Card> cards, int basemove)
    {
      for (int i = 0; i < GameData.PlayerInventorySize; i++)
      {
        if (cards is null || !cards.ContainsKey(i))
        {
          cards.Add(i, new CardEmpty(basemove));
        }
      }
      return cards;
    }

    private void Debug(List<Player> players)
    {
      foreach (var player in players)
      {
        Console.WriteLine($"player {player.Id} : pos {player.Position} : name {player.Name} : skin {player.Skin}");
      }
    }
  }
}
