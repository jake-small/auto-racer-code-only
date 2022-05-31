// using System;
// using System.Collections.Generic;
// using System.Text.Json;
// using Godot;

// public class FileLoader : DataLoader
// {
//   public bool CanLoadResource()
//   {
//     return true;
//   }

//   public T LoadJsonData<T>(string jsonFile)
//   {
//     if (!System.IO.File.Exists(jsonFile))
//     {
//       Console.WriteLine($"Error: provided json file '{jsonFile}' does not exist");
//       throw new Exception($"Error: provided json file '{jsonFile}' does not exist");
//     }
//     var jsonStr = System.IO.File.ReadAllLines(jsonFile);
//     var dataJson = String.Join("\n", jsonStr);
//     // TODO: error handling
//     var data = JsonSerializer.Deserialize<T>(dataJson);
//     return data;
//   }

//   public T LoadResourceData<T>(string resourceFile)
//   {
//     if (!ResourceLoader.Exists($"res://{resourceFile}"))
//     {
//       Console.WriteLine($"Error: provided resourceFile '{resourceFile}' does not exist");
//       throw new Exception($"Error: provided resourceFile '{resourceFile}' does not exist");
//     }
//     var file = new File();
//     var result = file.Open($"res://{resourceFile}", File.ModeFlags.Read);
//     if (result != Error.Ok)
//     {
//       Console.WriteLine($"Error: unable to open resourceFile '{resourceFile}'. Error code {result}");
//       throw new Exception($"Error: unable to open resourceFile '{resourceFile}'. Error code {result}");
//     }
//     var dataStr = file.GetAsText();
//     if (string.IsNullOrWhiteSpace(dataStr))
//     {
//       Console.WriteLine($"Error: resourceFile '{resourceFile}' is empty");
//       throw new Exception($"Error: resourceFile '{resourceFile}' is empty");
//     }
//     // TODO: error handling
//     var data = JsonSerializer.Deserialize<T>(dataStr);
//     return data;
//   }
// }