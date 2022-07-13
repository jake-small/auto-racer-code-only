
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class RaceScriptData : MoonSharpScriptData
{
  public MoonSharpPlayer Player { get; set; }
  public List<MoonSharpPlayer> AllPlayers { get; private set; }
  public int Turn { get; set; }

  public RaceScriptData(MoonSharpPlayer player, IEnumerable<MoonSharpPlayer> allPlayers, int turn)
  {
    Player = player;
    AllPlayers = allPlayers.ToList();
    Turn = turn;
  }

  public List<MoonSharpPlayer> GetOtherPlayers()
  {
    // Shuffle with a seed, that way multiple abilities can get lists in the same order, but random
    return AllPlayers.Shuffle(Turn + Player.Id).Where(p => p.Id != Player.Id).ToList();
  }

  public MoonSharpPlayer GetPlayer(int id)
  {
    return AllPlayers.FirstOrDefault(p => p.Id == id);
  }

  public List<MoonSharpPlayer> GetPlayersWithinRange(int min, int max)
  {
    var playersInRange = new List<MoonSharpPlayer>();
    foreach (var player in AllPlayers.Where(p => p.Id != Player.Id))
    {
      if (player.Position >= min && player.Position <= max)
      {
        playersInRange.Add(player);
      }
    }
    // Shuffle with a seed, that way multiple abilities can get lists in the same order, but random
    return playersInRange.Shuffle(Turn + Player.Id).ToList();
  }

  public List<MoonSharpMoveTokens> GetAllTokens(int forId = -1, int fromId = -1)
  {
    if (forId == -1 && fromId == -1)
    {
      return Player.MoveTokens;
    }
    else if (forId == -1 && fromId != -1)
    {
      return Player.MoveTokens.Where(t => t.CreatedBy == fromId).ToList();
    }
    else if (forId != -1 && fromId == -1)
    {
      return GetPlayer(forId).MoveTokens.ToList();
    }
    return GetPlayer(forId).MoveTokens.Where(t => t.CreatedBy == fromId).ToList();
  }

  public List<MoonSharpMoveTokens> GetPositiveTokens(int forId = -1, int fromId = -1)
  {
    if (forId == -1 && fromId == -1)
    {
      return Player.MoveTokens.Where(t => t.Value > 0).ToList();
    }
    else if (forId == -1 && fromId != -1)
    {
      return Player.MoveTokens.Where(t => t.Value > 0 && t.CreatedBy == fromId).ToList();
    }
    else if (forId != -1 && fromId == -1)
    {
      return GetPlayer(forId).MoveTokens.Where(t => t.Value > 0).ToList();
    }
    return GetPlayer(forId).MoveTokens.Where(t => t.Value > 0 && t.CreatedBy == fromId).ToList();
  }

  public List<MoonSharpMoveTokens> GetNegativeTokens(int forId = -1, int fromId = -1)
  {
    if (forId == -1 && fromId == -1)
    {
      return Player.MoveTokens.Where(t => t.Value < 0).ToList();
    }
    else if (forId == -1 && fromId != -1)
    {
      return Player.MoveTokens.Where(t => t.Value < 0 && t.CreatedBy == fromId).ToList();
    }
    else if (forId != -1 && fromId == -1)
    {
      return GetPlayer(forId).MoveTokens.Where(t => t.Value < 0).ToList();
    }
    return GetPlayer(forId).MoveTokens.Where(t => t.Value < 0 && t.CreatedBy == fromId).ToList();
  }
}

[MoonSharpUserData]
public class MoonSharpPlayer
{
  public int Id { get; set; }
  public int Position { get; set; } = 0;
  public List<MoonSharpMoveTokens> MoveTokens { get; set; }

  public MoonSharpPlayer(Player player)
  {
    Id = player.Id;
    Position = player.Position;
    MoveTokens = player.Tokens.OfType<MoveToken>().Select(
      t => new MoonSharpMoveTokens
      {
        CreatedBy = t.CreatedBy,
        Duration = t.Duration,
        Type = t.Type,
        Target = t.Target,
        Value = t.Value
      }).ToList();
  }
}

[MoonSharpUserData]
public class MoonSharpMoveTokens
{
  public int CreatedBy { get; set; }
  public int Duration { get; set; }
  public MoveTokenType Type { get; set; }
  public int Target { get; set; }
  public int Value { get; set; }
}