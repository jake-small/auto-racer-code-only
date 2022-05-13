using System;

public class MoveToken : Token

{
  public int CreatedBy { get; set; }
  public int Duration { get; set; }
  public MoveTokenType Type { get; set; }
  public int Target { get; set; }
  public int Value { get; set; }

  public int Calculate(int position)
  {
    switch (Type)
    {
      case MoveTokenType.Additive:
        return position + Value;
      case MoveTokenType.Multiplicative:
        return position * Value;
      case MoveTokenType.Exponential:
        return position ^ Value;
    }
    throw new Exception("Type of move token is not set");
  }
}

public enum MoveTokenType
{
  Additive,
  Multiplicative,
  Exponential
}