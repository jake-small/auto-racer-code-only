public static class TestHelperData
{
  public static MoveToken GetTestMoveToken(int value = 1, int duration = 1, int target = 0, int createdBy = 0, MoveTokenType type = MoveTokenType.Additive)
  {
    return new MoveToken
    {
      Value = value,
      Duration = duration,
      Target = target,
      CreatedBy = createdBy,
      Type = type
    };
  }
}