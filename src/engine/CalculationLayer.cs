using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class CalculationLayer
{
  public static Card ApplyLevelValues(Card card)
  {
    var result = TryGetLevelValue(card, out var levelValue);
    if (!result)
    {
      return card;
    }
    // Update all string properties with LevelValues
    // Recursively go through all other classes and do the same
    return UpdateAllNestedStrings(card, levelValue.OutKeys);
  }

  public static string ApplyLevelValues(Card card, string text, int levelId)
  {
    var result = TryGetLevelValue(card, out var levelValue);
    if (!result)
    {
      return text;
    }
    foreach (var param in levelValue.OutKeys)
    {
      var key = "{" + param.Key + "}";
      text = text.Replace(key, param.Value);
    }
    return text;
  }

  public static Card ApplyFunctionValues(Card card)
  {
    // TODO implement
    if (card.Abilities == null)
    {
      return card;
    }
    var abilities = card.Abilities;
    if (abilities.MoveTokenAbilities == null || !abilities.MoveTokenAbilities.Any())
    {
      return card;
    }
    foreach (var moveAbility in abilities.MoveTokenAbilities)
    {
      var functions = moveAbility.Functions;
      if (functions == null || functions.OutKeys == null || !functions.OutKeys.Any())
      {
        continue;
      }

      var calculatedOutKeys = new List<OutKey>();
      foreach (var function in functions.OutKeys)
      {
        // Calculate Lua function

        calculatedOutKeys.Add(new OutKey { Key = function.Key, Value = function.Value });
      }
      UpdateAllNestedStrings(card, calculatedOutKeys);
    }

    return card;
  }

  private static T UpdateAllNestedStrings<T>(T obj, List<OutKey> outKeys)
  {
    var stringProperties = obj.GetType().GetProperties().Where(p => p.PropertyType == typeof(string) && p.CanWrite);
    foreach (var stringProp in stringProperties)
    {
      foreach (var outKey in outKeys)
      {
        var key = "{" + outKey.Key + "}";
        var propValue = stringProp.GetValue(obj) as string;
        propValue = propValue.Replace(key, outKey.Value);
        stringProp.SetValue(obj, propValue);
      }
    }

    var otherProperties = obj.GetType().GetProperties().Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string));
    foreach (var otherProp in otherProperties)
    {
      var propValue = otherProp.GetValue(obj);
      var elems = propValue as IList;
      if (elems != null)
      {
        if (elems.Count < 1)
        {
          continue;
        }
        var elemType = elems[0].GetType();
        var listType = typeof(List<>);
        var constructedListType = listType.MakeGenericType(elemType);
        var updatedElems = (IList)Activator.CreateInstance(constructedListType);
        foreach (var elem in elems)
        {
          updatedElems.Add(UpdateAllNestedStrings(elem, outKeys));
        }
        otherProp.SetValue(obj, updatedElems);
      }
      else
      {
        otherProp.SetValue(obj, UpdateAllNestedStrings(propValue, outKeys));
      }
    }
    // TODO test HEAVILY
    return obj;
  }

  private static bool TryGetLevelValue(Card card, out LevelValue levelValue)
  {
    var levelValues = card.LevelValues;
    if (levelValues == null || !levelValues.Any())
    {
      levelValue = null;
      return false;
    }
    var level = card.Level;
    levelValue = levelValues?.FirstOrDefault(t => t.Id == level);
    if (levelValue == null)
    {
      GD.Print($"Error: level does not exist: {level}");
      throw new Exception($"Error: level does not exist: {level}");
    }
    return true;
  }
}