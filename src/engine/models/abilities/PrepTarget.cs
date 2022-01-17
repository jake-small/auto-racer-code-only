using System;

public class PrepTarget
{
  /// <summary>
  ///  Possible values: "other", "self", "all"
  /// </summary>
  public string Type { private get; set; }
  /// <summary>
  ///  Possible values: "player", "shop", "all"
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

  public TargetType GetTargetType()
  {
    var result = Enum.TryParse(Type, true, out TargetType type);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Type {Type} to enum TargetType");
      throw new Exception($"Error: unable to parse Type {Type} to enum TargetType");
    }
    return type;
  }

  public InventoryTarget GetInventoryType()
  {
    var result = Enum.TryParse(Inventory, true, out InventoryTarget type);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Inventory {Inventory} to enum InventoryType");
      throw new Exception($"Error: unable to parse Inventory {Inventory} to enum InventoryType");
    }
    return type;
  }

  public Direction GetDirection()
  {
    var result = Enum.TryParse(Direction, true, out Direction direction);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Direction {Direction} to enum Direction");
      throw new Exception($"Error: unable to parse Direction {Direction} to enum Direction");
    }
    return direction;
  }

  public Priority GetPriority()
  {
    var result = Enum.TryParse(Priority, true, out Priority priority);
    if (!result)
    {
      Console.WriteLine($"Error: unable to parse Priority {Priority} to enum Priority");
      throw new Exception($"Error: unable to parse Priority {Priority} to enum Priority");
    }
    return priority;
  }
}

public enum InventoryTarget
{
  Player,
  Shop,
  Any
}