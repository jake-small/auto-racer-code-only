using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static AutoRaceEngine;

namespace AutoRacerTests.Tests
{
  [TestFixture]
  public class MoveTokenCardTests
  {
    private List<Card> _cards;

    [SetUp]
    public void SetUp()
    {
      var cardLoader = new CardLoader(@"G:\JakeDoc\Files\Projects\Godot\AutoRacer\auto-racer\configs\cardsTest.json");
      _cards = cardLoader.GetCards();
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void Curse_Nearby_Success(int level)
    {
      var curseCard = _cards.FirstOrDefault(c => c.GetRawName().Equals("curse", StringComparison.InvariantCultureIgnoreCase));
      curseCard.Level = level;
      var player = new Player { Id = 0, Cards = new Dictionary<int, Card>() { { 0, curseCard } }, Position = 0 };
      var playerResults = TestRace(player, 1);
      var leveledCurseCard = curseCard.GetLeveledCard();
      foreach (var playerResult in playerResults)
      {
        if (playerResult.Id is 0)
        {
          Assert.That(playerResult.Position, Is.EqualTo(leveledCurseCard.BaseMove));
          continue;
        }
        Assert.That(playerResult.Position, Is.EqualTo(1 + leveledCurseCard.Abilities.MoveTokenAbilities.FirstOrDefault().Value.ToInt()));
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
        var player = players[i];
        if (player is null)
        {
          player = new Player { Id = i, Cards = new Dictionary<int, Card>(), Position = defaultPosition };
        }
        player.Cards = FillEmptyCardSlots(player.Cards, 1);
      }
      var engine = new AutoRaceEngine(players, 5, 5);
      var turn = 0;
      while (turn < numTurns && engine.GetTurnPhase() != TurnPhases.End)
      {
        engine.AdvanceRace();
        turn = turn + 1;
      }
      return engine.GetStandings();
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

  }
}
