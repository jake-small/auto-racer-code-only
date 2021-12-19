public class Move : Ability
{
  public Phase Phase { get; set; }
  public string Value { get; set; }
  public string Duration { get; set; }
  public string Target { get; set; }
  public string BuiltInFunctions { get; set; }
  public string Functions { get; set; }

  public Move(Phase phase)
  {
    Phase = phase;
  }

  public int GetMove() { return 0; }
}