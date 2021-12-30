using System;
using System.Collections.Generic;
using Godot;

public static class ExtensionMethods
{
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
      GD.Print($"Error: unable to parse string to int: {strValue}");
      throw new Exception($"Error: unable to parse string to int: {strValue}");
    }
    return intValue;
  }
}