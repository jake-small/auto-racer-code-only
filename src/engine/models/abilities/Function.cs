using System;
using System.Collections.Generic;

public class Function : ICloneable
{
  private IEnumerable<string> _bodyMultiline;
  public string Key { get; set; }
  public IEnumerable<string> BodyMultiline
  {
    get
    {
      return _bodyMultiline;
    }
    set
    {
      _bodyMultiline = value;
      Body = string.Join("\n", value);
    }
  }
  public string Body { get; set; }

  public object Clone()
  {
    return new Function
    {
      Key = Key,
      BodyMultiline = BodyMultiline,
      Body = Body
    };
  }
}