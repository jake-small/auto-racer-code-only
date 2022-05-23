using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

public class FileLoader
{
  public T LoadJsonData<T>(string dataFile)
  {
    if (!ResourceLoader.Exists($"res://{dataFile}"))
    {
      Console.WriteLine($"Error: provided dataFile '{dataFile}' does not exist");
      throw new Exception($"Error: provided dataFile '{dataFile}' does not exist");
    }
    var file = new File();
    var result = file.Open($"res://{dataFile}", File.ModeFlags.Read);
    if (result != Error.Ok)
    {
      Console.WriteLine($"Error: unable to open dataFile '{dataFile}'. Error code {result}");
      throw new Exception($"Error: unable to open dataFile '{dataFile}'. Error code {result}");
    }
    var dataStr = file.GetAsText();
    if (string.IsNullOrWhiteSpace(dataStr))
    {
      Console.WriteLine($"Error: dataFile '{dataFile}' is empty");
      throw new Exception($"Error: dataFile '{dataFile}' is empty");
    }
    // TODO: error handling
    var data = JsonSerializer.Deserialize<T>(dataStr);
    return data;
  }
}