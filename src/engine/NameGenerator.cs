using System;
using System.Text.Json;

public class NameGenerator
{
  private Names _availableNames { get; set; }
  private Random _random = new Random();
  private const string DefaultNameDataPath = @"configs/nameData.tres";

  public NameGenerator(string nameJsonFile)
  {
    var dataLoader = new FileLoader();
    try
    {
      if (System.IO.File.Exists(nameJsonFile))
      {
        Console.WriteLine($"Loading name data '{nameJsonFile}'");
        _availableNames = dataLoader.LoadJsonData<Names>(nameJsonFile);
      }
    }
    catch (System.Exception)
    {
      Console.WriteLine($"Warning: Unable to access filesystem to access name config '{nameJsonFile}', using built-in name data instead");
    }
    finally
    {
      if (_availableNames == null || _availableNames.FirstNames.Count == 0 || _availableNames.Adjectives.Count == 0)
      {
        Console.WriteLine($"Warning: Name json file not found at'{nameJsonFile}', using built-in name data instead");
        if (dataLoader.CanLoadResource())
        {
          _availableNames = dataLoader.LoadResourceData<Names>(DefaultNameDataPath);
        }
        else
        {
          Console.WriteLine($"Warning: Unable to load name resource file");
        }
      }
    }
  }

  public string GetRandomName()
  {
    return $"{GetRandomFirstName()} the {GetRandomAdjective()}";
  }

  public string GetRandomFirstName()
  {
    var firstName = _availableNames.FirstNames[_random.Next(_availableNames.FirstNames.Count)];

    if (string.IsNullOrWhiteSpace(firstName))
    {
      throw new Exception("Unable to get random first name");
    }
    return firstName.Capitalize();
  }

  public string GetRandomAdjective()
  {
    var adjective = _availableNames.Adjectives[_random.Next(_availableNames.Adjectives.Count)];

    if (string.IsNullOrWhiteSpace(adjective))
    {
      throw new Exception("Unable to get random adjective");
    }
    return adjective.Capitalize();
  }
}