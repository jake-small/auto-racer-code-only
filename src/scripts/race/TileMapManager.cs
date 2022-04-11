using System;

public class TileMapManager
{
  private BackgroundTileMap[] _tileMaps;

  public TileMapManager(BackgroundTileMap[] tileMaps)
  {
    _tileMaps = tileMaps;
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
