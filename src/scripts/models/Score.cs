using System.Collections.Generic;

public class Score
{
  private Dictionary<int, int> _placements = new Dictionary<int, int>();

  public Score(int numPlayers)
  {
    for (int i = 0; i < numPlayers; i++)
    {
      _placements[i] = 0;
    }
  }

  public int GetResult(int place)
  {
    return _placements[place];
  }

  public int AddResult(int place)
  {
    _placements[place] = _placements[place] + 1;
    return _placements[place];
  }
}