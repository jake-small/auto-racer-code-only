
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

  public IEnumerable<MoonSharpPlayer> GetPlayersWithinRange(int min, int max)
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
}

[MoonSharpUserData]
public class MoonSharpPlayer
{
  public int Id { get; set; }
  public int Position { get; set; } = 0;

  public MoonSharpPlayer(Player player)
  {
    Id = player.Id;
    Position = player.Position;
  }
}