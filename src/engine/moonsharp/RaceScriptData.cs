
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class RaceScriptData : MoonSharpScriptData
{
  public MoonSharpPlayer Player { get; set; }
  public List<MoonSharpPlayer> AllPlayers { get; private set; }

  public RaceScriptData(MoonSharpPlayer player, IEnumerable<MoonSharpPlayer> allPlayers)
  {
    Player = player;
    AllPlayers = allPlayers.ToList();
  }

  public List<MoonSharpPlayer> GetOtherPlayers()
  {
    return AllPlayers.Where(p => p.Id != Player.Id).ToList();
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
    return playersInRange;
  }

  public List<MoonSharpMoveTokens> GetAllTokens(int id = -1)
  {
    if (id == -1)
    {
      return Player.MoveTokens;
    }
    return GetPlayer(id).MoveTokens;
  }

  public List<MoonSharpMoveTokens> GetPositiveTokens(int id = -1)
  {
    if (id == -1)
    {
      return Player.MoveTokens.Where(t => t.Value > 0).ToList();
    }
    return GetPlayer(id).MoveTokens.Where(t => t.Value > 0).ToList();
  }

  public List<MoonSharpMoveTokens> GetNegativeTokens(int id = -1)
  {
    if (id == -1)
    {
      return Player.MoveTokens.Where(t => t.Value < 0).ToList();
    }
    return GetPlayer(id).MoveTokens.Where(t => t.Value < 0).ToList();
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