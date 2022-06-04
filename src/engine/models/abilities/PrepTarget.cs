using System;

public class PrepTarget
{
  /// <summary>
  /// The card that triggered the effect. e.g. a Bought card
  ///  Possible values: "true", "false"
  /// </summary>
  public string TriggerCard { private get; set; }
  /// <summary>
  ///  Possible values: "other", "self", "all"
  /// </summary>
  public string Type { private get; set; }
  /// <summary>
  ///  Possible values: "player", "shop", "any"
  /// </summary>
  public string Inventory { private get; set; }
  /// <summary>
  ///  Possible values: either a player or inventory slot number
  /// </summary>
  public string Slot { get; set; }
  /// <summary>
  ///  Possible values: "forward", "backward", "any"
  /// </summary>
  public string Direction { private get; set; }
  /// <summary>
  ///  Possible values: "closest", "furthest", "positionAscending", "positionDescending"
  /// </summary>
  public string Priority { private get; set; }
  /// <summary>
  ///  Possible values: -1 or a positive int
  /// </summary>
  public string Amount { get; set; }

  public bool GetTriggerCard()
  {
    if (string.IsNullOrWhiteSpace(TriggerCard))
    {
      return false;
    }
    var result = bool.TryParse(TriggerCard, out bool useTriggerCard);
    if (result == false)
    {
      Console.WriteLine($"Error: unable to parse TriggerCard '{TriggerCard}' to bool");
      throw new Exception($"Error: unable to parse TriggerCard '{TriggerCard}' to bool");
    }
    return useTriggerCard;
  }

  public TargetType GetTargetType()
  {
    var result = Enum.TryParse(Type, true, out TargetType type);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Type '{Type}' to enum TargetType");
      throw new Exception($"Error: unable to parse Type '{Type}' to enum TargetType");
    }
    return type;
  }

  public InventoryType GetInventoryType()
  {
    var result = Enum.TryParse(Inventory, true, out InventoryType type);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Inventory '{Inventory}' to enum InventoryType");
      throw new Exception($"Error: unable to parse Inventory '{Inventory}' to enum InventoryType");
    }
    return type;
  }

  public Direction GetDirection()
  {
    var result = Enum.TryParse(Direction, true, out Direction direction);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Direction '{Direction}' to enum Direction");
      throw new Exception($"Error: unable to parse Direction '{Direction}' to enum Direction");
    }
    return direction;
  }

  public Priority GetPriority()
  {
    var result = Enum.TryParse(Priority, true, out Priority priority);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Priority '{Priority}' to enum Priority");
      throw new Exception($"Error: unable to parse Priority '{Priority}' to enum Priority");
    }
    return priority;
  }
}