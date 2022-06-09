using System.Collections.Generic;
using Godot;

public static class OsFeatures
{
  public static void PrintOsFeatures()
  {
    var features = new List<string> { "Android", "HTML5", "JavaScript", "mobile", "web", "Windows", "debug", "release", "pc", "iOS", "OSX", "X11" };
    GD.Print($"OS has feature:");
    foreach (var feature in features)
    {
      GD.Print($"    {feature} {OS.HasFeature(feature)}");
    }
  }

  public static bool IsMobileHtml5()
  {
    return IsMobile() && IsHtml5();
  }

  public static bool IsMobile()
  {
    return OS.HasFeature("Android") || OS.HasFeature("iOS") || OS.HasFeature("mobile");
  }

  public static bool IsHtml5()
  {
    return OS.HasFeature("HTML5") || OS.HasFeature("JavaScript") || OS.HasFeature("web");
  }

  public static bool IsTouchScreen()
  {
    return OS.HasTouchscreenUiHint();
  }
}