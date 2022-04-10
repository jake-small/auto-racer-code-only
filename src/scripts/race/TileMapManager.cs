using System;

public class TileMapManager
{
  private BackgroundTileMap[] _tileMaps;

  public TileMapManager(BackgroundTileMap[] tileMaps)
  {
    _tileMaps = tileMaps;
  }

  public void ScrollRight(int numSpaces)
  {
    foreach (var tileMap in _tileMaps)
    {
      tileMap.ScrollRight(numSpaces);
    }
  }
}
