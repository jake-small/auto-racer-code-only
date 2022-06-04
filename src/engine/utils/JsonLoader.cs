using System;
using System.Text.Json;

public class JsonLoader : DataLoader
{
  public bool CanLoadResource()
  {
    return false;
  }

  public T LoadJsonData<T>(string jsonFile)
  {
    if (!System.IO.File.Exists(jsonFile))
    {
      Console.WriteLine($"Error: provided json file '{jsonFile}' does not exist");
      throw new Exception($"Error: provided json file '{jsonFile}' does not exist");
    }
    var jsonStr = System.IO.File.ReadAllLines(jsonFile);
    var dataJson = String.Join("\n", jsonStr);
    // TODO: error handling
    var data = JsonSerializer.Deserialize<T>(dataJson);
    return data;
  }

  public T LoadResourceData<T>(string resourceFile)
  {
    throw new NotImplementedException();
  }
}