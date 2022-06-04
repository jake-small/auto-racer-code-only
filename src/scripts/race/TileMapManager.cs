using System;
using System.Linq;

public class TileMapManager
{
  private BackgroundTileMap[] _tileMaps;

  public TileMapManager(BackgroundTileMap[] tileMaps)
  {
    _tileMaps = tileMaps;
  }

  public bool IsScrolling()
  {
    return _tileMaps.Any(tile => tile.IsScrolling);
  }

  public void ScrollRight(float amount)
  {
    foreach (var tileMap in _tileMaps)
    {
      tileMap.ScrollRight(amount);
    }
  }

  public void ScrollLeft(float amount)
  {
    foreach (var tileMap in _tileMaps)
    {
      tileMap.ScrollLeft(amount);
    }
  }
}
