public class Range
{
  public string Min { get; set; }
  public string Max { get; set; }

  public object Clone()
  {
    return new Range
    {
      Min = Min,
      Max = Max
    };
  }
}