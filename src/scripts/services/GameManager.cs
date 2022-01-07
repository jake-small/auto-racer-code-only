public static class GameManager
{
  public static Player Player1 { get; set; }
  public static int RaceNumber { get; set; } = 0;
  public static PrepEngine PrepEngine { get; set; } = new PrepEngine();
}