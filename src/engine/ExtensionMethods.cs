using System;
using System.Collections.Generic;

public static class ExtensionMethods
{
  private static readonly Random rnd = new Random();

  public static void MergeDictionaries<OBJ1, OBJ2>(this IDictionary<OBJ1, List<OBJ2>> dict1, IDictionary<OBJ1, List<OBJ2>> dict2)
  {
    foreach (var kvp2 in dict2)
    {
      // If the dictionary already contains the key then merge them
      if (dict1.ContainsKey(kvp2.Key))
      {
        dict1[kvp2.Key].AddRange(kvp2.Value);
        continue;
      }
      dict1.Add(kvp2);
    }
  }

  public static int ToInt(this string strValue)
  {
    if (!Int32.TryParse(strValue, out var intValue))
    {
      Console.WriteLine($"Error: unable to parse string to int: {strValue}");
      throw new Exception($"Error: unable to parse string to int: {strValue}");
    }
    return intValue;
  }


  public static string Capitalize(this string input)
  {
    switch (input)
    {
      case null: throw new ArgumentNullException(nameof(input));
      case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
      default: return input[0].ToString().ToUpper() + input.Substring(1);
    }
  }

  public static void Shuffle<T>(this IList<T> list)
  {
    var n = list.Count;
    while (n > 1)
    {
      var k = (rnd.Next(0, n) % n);
      n--;
      T value = list[k];
      list[k] = list[n];
      list[n] = value;
    }
  }
}