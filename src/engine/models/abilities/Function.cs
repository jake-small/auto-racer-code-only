using System.Collections.Generic;
using System.Linq;

public class Function
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
}