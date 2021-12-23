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
    return UpdateAllNestedStrings(card, levelValue);
  }

  public static string ApplyLevelValues(Card card, string text, int levelId)
  {
    var result = TryGetLevelValue(card, out var levelValue);
    if (!result)
    {
      return text;
    }
    foreach (var param in levelValue.Params)
    {
      var key = "{" + param.Key + "}";
      text = text.Replace(key, param.Value);
    }
    return text;
  }

  public static Card ApplyFunctionValues(Card card)
  {
    // TODO implement
    GD.Print("Not implemented: ApplyFunctionValues(Card card)");
    return card;
  }

  private static T UpdateAllNestedStrings<T>(T obj, LevelValue levelValue)
  {
    var stringProperties = obj.GetType().GetProperties().Where(p => p.PropertyType == typeof(string) && p.CanWrite); // TODO double check, instead of typeof(T)
    foreach (var stringProp in stringProperties)
    {
      foreach (var param in levelValue.Params)
      {
        var key = "{" + param.Key + "}";
        var propValue = stringProp.GetValue(obj) as string;
        propValue = propValue.Replace(key, param.Value);
        stringProp.SetValue(obj, propValue);
      }
    }
    var customProperties = obj.GetType().GetProperties().Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string)); // TODO double check, instead of typeof(T)
    // foreach (var customProp in customProperties)
    // {
    //   var propValue = customProp.GetValue(obj);
    //   customProp.SetValue(obj, UpdateAllNestedStrings(propValue, levelValue));
    // }
    // var listProperties = typeof(T).GetProperties().Where(p => p.PropertyType && p.PropertyType != typeof(List));
    // var listProperties = typeof(T).GetProperties().Where(p => p.PropertyType is IList);
    var otherProperties = obj.GetType().GetProperties().Where(p => p.PropertyType.IsClass && p.PropertyType != typeof(string)); // TODO double check
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
          updatedElems.Add(UpdateAllNestedStrings(elem, levelValue));
        }
        otherProp.SetValue(obj, updatedElems);
      }
      else
      {
        otherProp.SetValue(obj, UpdateAllNestedStrings(propValue, levelValue));
      }
    }
    // TODO test HEAVILY
    return obj;
  }


  // private static Ability ApplyLevelValues(Ability ability, LevelValue levelValue)
  // {
  //   var properties = typeof(Ability).GetProperties().Where(p => p.PropertyType == typeof(string));
  //   foreach (var prop in properties)
  //   {
  //     foreach (var param in levelValue.Params)
  //     {
  //       var key = "{" + param.Key + "}";
  //       var propValue = prop.GetValue(ability) as string;
  //       propValue = propValue.Replace(key, param.Value);
  //       prop.SetValue(ability, propValue);
  //     }
  //   }
  //   return ability;
  // }

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