using Godot;
using System;
using static CharacterScript;

public class OffscreenIndicatorScript : Node2D
{
  public int Id { get; set; }
  public int Distance { get; set; } = 0;
  private string _characterSkin;
  public string CharacterSkin
  {
    get
    {
      return _characterSkin;
    }
    set
    {
      _characterSkin = value;
      UpdateSkin();
    }
  }

  private CharacterScript _character;
  private Label _distanceLabel;

  public override void _Ready()
  {
    _distanceLabel = GetNode<Label>(RaceSceneData.OffscreenIndicatorLabelDistance);
    var position = GetNode<Position2D>(RaceSceneData.OffscreenIndicatorCharacterPosition);
    var characterScene = ResourceLoader.Load(RaceSceneData.CharacterScenePath) as PackedScene;
    var characterInstance = (CharacterScript)characterScene.Instance();
    characterInstance.AnimationState = AnimationStates.running;
    characterInstance.Position = position.Position;
    characterInstance.Scale = new Vector2((float)0.5, (float)0.5);
    _character = characterInstance;
    AddChild(characterInstance);
  }

  public override void _Process(float delta)
  {
    _distanceLabel.Text = Distance.ToString();
  }

  public void UpdateSkin()
  {
    if (_character != null)
    {
      _character.CharacterSkin = CharacterSkin;
    }
  }
}
