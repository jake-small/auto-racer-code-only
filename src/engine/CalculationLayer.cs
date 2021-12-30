using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MoonSharp.Interpreter;
using LuaScript = MoonSharp.Interpreter.Script;

public class CalculationLayer
{
  private LuaScript _luaScript;

  public CalculationLayer()
  {
    // https://www.moonsharp.org/sandbox.html
    _luaScript = new LuaScript(CoreModules.Preset_HardSandbox);
  }
  private MoonSharp.Interpreter.Script _script = new MoonSharp.Interpreter.Script();
  public Card ApplyLevelValues(Card card)
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

  public string ApplyLevelValues(Card card, string text, int levelId)
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

  public Card ApplyFunctionValues(Card card)
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
        var calculatedFunctionValue = RunLuaScript(function.Value);
        calculatedOutKeys.Add(new OutKey { Key = function.Key, Value = calculatedFunctionValue });
      }
      UpdateAllNestedStrings(card, calculatedOutKeys);
    }

    return card;
  }

  private T UpdateAllNestedStrings<T>(T obj, List<OutKey> outKeys)
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
      if (propValue == null)
      {
        continue;
      }
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

  private bool TryGetLevelValue(Card card, out LevelValue levelValue)
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


  private string RunLuaScript(string scriptBody) // , LuaPlayerData playerData)
  {
    string script = $@"
        function script (n)
          {scriptBody}
        end
        return script(5)";

    DynValue res = MoonSharp.Interpreter.Script.RunString(script);
    switch (res.Type)
    {
      case DataType.Number:
        return res.Number.ToString();
      case DataType.Boolean:
        return res.Boolean.ToString();
      case DataType.String:
        return res.String;
      case DataType.Nil:
        GD.Print($"Lua script returned Nil: '{scriptBody}'");
        throw new Exception($"Lua script returned Nil: '{scriptBody}'");
      default:
        GD.Print($"Lua script needs to, but did not, return type Number, Boolean, or String: '{scriptBody}'");
        throw new Exception($"Lua script needs to, but did not, return type Number, Boolean, or String: '{scriptBody}'");
    }
  }
}