using System;
using System.Text.Json;

public class NameGenerator : FileLoader
{
  private Names _availableNames { get; set; }
  private Random _random = new Random();

  public NameGenerator(string nameJsonFile)
  {
    _availableNames = LoadJsonData<Names>(nameJsonFile);
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