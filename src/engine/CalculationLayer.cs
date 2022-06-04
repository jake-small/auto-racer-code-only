using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using LuaScript = MoonSharp.Interpreter.Script;

public class CalculationLayer
{
  private LuaScript _luaScript;

  public CalculationLayer()
  {
    // Automatically register all MoonSharpUserData types
    UserData.RegisterAssembly();
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

  public Card ApplyPrepFunctionValues(Card card)
  {
    if (card.Abilities == null)
    {
      return card;
    }
    var abilities = card.Abilities;
    if (abilities.PrepAbilities == null || !abilities.PrepAbilities.Any())
    {
      return card;
    }
    foreach (var prepAbility in abilities.PrepAbilities)
    {
      var functions = prepAbility.Functions;
      if (functions == null || !functions.Any())
      {
        continue;
      }

      var playerInventory = GameManager.PrepEngine.PlayerInventory.GetCards();
      var shopInventory = GameManager.PrepEngine.ShopInventory.GetCards();
      var coinTotal = GameManager.PrepEngine.Bank.CoinTotal;
      var lifeTotal = GameManager.LifeTotal;
      var raceNumber = GameManager.CurrentRace;
      var scriptData = new PrepScriptData(card, playerInventory, shopInventory, coinTotal, lifeTotal, raceNumber);
      var calculatedOutKeys = CalculateFunctions(functions, scriptData).ToList();
      UpdateAllNestedStrings(card, calculatedOutKeys);
    }

    return card;
  }

  public Card ApplyTokenFunctionValues(Card card, Player player, IEnumerable<Player> players)
  {
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
      if (functions == null || !functions.Any())
      {
        continue;
      }

      var scriptPlayers = players.Select(p => new MoonSharpPlayer(p));
      var scriptData = new RaceScriptData(new MoonSharpPlayer(player), scriptPlayers);
      var calculatedOutKeys = CalculateFunctions(functions, scriptData).ToList();
      UpdateAllNestedStrings(card, calculatedOutKeys);
    }

    return card;
  }

  private IEnumerable<OutKey> CalculateFunctions(IEnumerable<Function> functions, MoonSharpScriptData scriptData)
  {
    var calculatedOutKeys = new List<OutKey>();
    foreach (var function in functions)
    {
      // Calculate Lua function
      var calculatedFunctionValue = RunLuaScript(function.Body, scriptData);
      calculatedOutKeys.Add(new OutKey { Key = function.Key, Value = calculatedFunctionValue });
    }
    return calculatedOutKeys;
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
        if (propValue != null)
        {
          propValue = propValue.Replace(key, outKey.Value);
          stringProp.SetValue(obj, propValue);
        }
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
      Console.WriteLine($"Error: level does not exist: {level}");
      throw new Exception($"Error: level does not exist: {level}");
    }
    return true;
  }


  private string RunLuaScript(string scriptBody, MoonSharpScriptData scriptData)
  {
    var script = $@"
        function script ()
          {scriptBody}
        end
        return script()";

    _luaScript.Globals["scriptData"] = scriptData;
    DynValue res = _luaScript.DoString(script);
    switch (res.Type)
    {
      case DataType.Number:
        return res.Number.ToString();
      case DataType.Boolean:
        return res.Boolean.ToString();
      case DataType.String:
        return res.String;
      case DataType.Nil:
        Console.WriteLine($"Lua script returned Nil: '{scriptBody}'");
        throw new Exception($"Lua script returned Nil: '{scriptBody}'");
      default:
        Console.WriteLine($"Lua script needs to, but did not, return type Number, Boolean, or String: '{scriptBody}'");
        throw new Exception($"Lua script needs to, but did not, return type Number, Boolean, or String: '{scriptBody}'");
    }
  }
}