using System;
using System.Collections.Generic;
using Godot;

public interface TokenAbility : Ability
{
  string Phase { get; set; }
  string Duration { get; set; }
  Target Target { get; set; }
}