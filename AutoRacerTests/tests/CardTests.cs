using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static AutoRaceEngine;

namespace AutoRacerTests.Tests
{
  [TestFixture]
  public class CardTests
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
      var cards = GetEmptyCards(1);
      cards[0] = curseCard;

      var player = new Player { Id = 0, Cards = cards, Position = 0 };
      var players = new List<Player> { player };
      players.AddRange(GetTestPlayers());
      var engine = new AutoRaceEngine(players, 5, 5);

      while (engine.GetTurnPhase() != TurnPhases.End)
      {
        engine.AdvanceRace();
      }

      var playerResults = engine.GetStandings();
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

    private List<Player> GetTestPlayers(int basemove = 1)
    {
      var ids = new int[] { 1, 2, 3 };
      var positions = new int[] { 0, 0, 0 };
      return GetTestPlayers(ids, positions, basemove);
    }
    private List<Player> GetTestPlayers(int[] ids, int[] positions, int basemove = 1)
    {
      if (ids.Count() != positions.Count())
      {
        throw new ArgumentException("Different number of Ids and Positions in CardTests.GetTestPlayers()");
      }
      var players = new List<Player>();
      for (int i = 0; i < ids.Count(); i++)
      {
        players.Add(GetTestPlayer(ids[i], positions[i], basemove));
      }
      return players;
    }

    private Player GetTestPlayer(int id, int position, int basemove)
    {
      return new Player { Id = id, Cards = GetEmptyCards(basemove), Position = position };
    }

    private Dictionary<int, Card> GetEmptyCards(int basemove, int amount = 5)
    {
      var cardDict = new Dictionary<int, Card>();
      for (int i = 0; i < amount; i++)
      {
        cardDict.Add(i, new CardEmpty(basemove));
      }
      return cardDict;
    }
  }

}
