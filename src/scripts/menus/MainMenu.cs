using Godot;
using System;

public class MainMenu : Control
{
  private CharacterScript _playerCharacter;
  private int _characterSelectIndex;

  public override void _EnterTree()
  {
    base._EnterTree();
    LoadCharacterSkins();
  }
  public override void _Ready()
  {
    _playerCharacter = GetNode<CharacterScript>("Character");
    _characterSelectIndex = GameManager.CharacterSkins.FindIndex(s => s.Equals(_playerCharacter.CharacterSkin, StringComparison.InvariantCultureIgnoreCase));
    var startButton = GetNode("MarginContainer/VBoxContainer/HBoxContainer/Button_Start") as Button;
    startButton.Connect("pressed", this, nameof(Button_start_pressed));
    var quitButton = GetNode("MarginContainer/VBoxContainer/HBoxContainer/Button_Quit") as Button;
    quitButton.Connect("pressed", this, nameof(Button_quit_pressed));
    var prevSkinButton = GetNode("MarginContainer/VBoxContainer/HBoxContainerCharSelect/Button_Prev_Skin") as Button;
    prevSkinButton.Connect("pressed", this, nameof(Button_previous_skin_pressed));
    var nextSkinButton = GetNode("MarginContainer/VBoxContainer/HBoxContainerCharSelect/Button_Next_Skin") as Button;
    nextSkinButton.Connect("pressed", this, nameof(Button_next_skin_pressed));
  }

  private void Button_previous_skin_pressed()
  {
    Console.WriteLine("Previous Skin button pressed");
    _characterSelectIndex = _characterSelectIndex - 1;
    if (_characterSelectIndex < 0)
    {
      _characterSelectIndex = GameManager.CharacterSkins.Count - 1;
    }
    var prevSkin = GameManager.CharacterSkins[_characterSelectIndex];
    _playerCharacter.CharacterSkin = prevSkin;
  }

  private void Button_next_skin_pressed()
  {
    Console.WriteLine("Next Skin button pressed");
    _characterSelectIndex = _characterSelectIndex + 1;
    if (_characterSelectIndex >= GameManager.CharacterSkins.Count)
    {
      _characterSelectIndex = 0;
    }
    var nextSkin = GameManager.CharacterSkins[_characterSelectIndex];
    _playerCharacter.CharacterSkin = nextSkin;
  }

  private void Button_start_pressed()
  {
    Console.WriteLine("Start Game button pressed");
    GameManager.PlayerCharacterSkin = _playerCharacter.CharacterSkin;
    GetTree().ChangeScene("res://src/scenes/game/Prep.tscn");
  }

  private void Button_quit_pressed()
  {
    Console.WriteLine("Quit button pressed");
    GetTree().Quit();
  }

  private void LoadCharacterSkins()
  {
    var skinDirectory = "res://assets/characters/animation_frames/";
    var directory = new Directory();
    if (directory.Open(skinDirectory) == Error.Ok)
    {
      directory.ListDirBegin();
      var skin = directory.GetNext();
      while (!String.IsNullOrWhiteSpace(skin))
      {
        if (!directory.CurrentIsDir())
        {
          GameManager.CharacterSkins.Add(System.IO.Path.Combine(skinDirectory, skin));
        }
        skin = directory.GetNext();
      }
      if (GameManager.CharacterSkins.Count <= 0)
      {
        GD.Print($"No character skins found at path: {skinDirectory}");
        throw new Exception($"No character skins found at path: {skinDirectory}");
      }
    }
    else
    {
      GD.Print($"Unable open the character skin directory at path: {skinDirectory}");
      throw new Exception($"Unable open the character skin directory at path: {skinDirectory}");
    }
  }
}
