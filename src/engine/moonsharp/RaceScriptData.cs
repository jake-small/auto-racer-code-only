
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class RaceScriptData : MoonSharpScriptData
{
  public MoonSharpPlayer Player { get; set; }
  public IEnumerable<MoonSharpPlayer> AllPlayers { get; private set; }

  public RaceScriptData(MoonSharpPlayer player, IEnumerable<MoonSharpPlayer> allPlayers)
  {
    Player = player;
    AllPlayers = allPlayers;
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

  public List<MoonSharpMoveTokens> GetPositiveTokens()
  {
    return Player.MoveTokens.Where(t => t.Value > 0).ToList();
  }

  public List<MoonSharpMoveTokens> GetNegativeTokens()
  {
    return Player.MoveTokens.Where(t => t.Value < 0).ToList();
  }
}

[MoonSharpUserData]
public class MoonSharpPlayer
{
  public int Id { get; set; }
  public int Position { get; set; } = 0;
  public IEnumerable<MoonSharpMoveTokens> MoveTokens { get; set; }

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
      });
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