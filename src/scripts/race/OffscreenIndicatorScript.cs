using Godot;
using System;
using static CharacterScript;

public class OffscreenIndicatorScript : Node2D
{
  public int Id { get; set; }
  public int Distance { get; set; } = 0;

  private CharacterScript _characterRef;
  public CharacterScript CharacterRef
  {
    get
    {
      return _characterRef;
    }
    set
    {
      _characterRef = value;
      UpdateSkin(_characterRef.CharacterSkin);
    }
  }

  private CharacterScript _characterIcon;
  private Label _distanceLabel;
  private bool _isLeftIndicator;

  public override void _Ready()
  {
    _distanceLabel = GetNode<Label>(RaceSceneData.OffscreenIndicatorLabelDistance);
    _isLeftIndicator = Position < GetViewport().Size / 2;
    var position = GetNode<Position2D>(RaceSceneData.OffscreenIndicatorCharacterPosition);
    var characterScene = ResourceLoader.Load(RaceSceneData.CharacterScenePath) as PackedScene;
    var characterInstance = (CharacterScript)characterScene.Instance();
    characterInstance.AnimationState = AnimationStates.running;
    characterInstance.Position = position.Position;
    characterInstance.Scale = new Vector2((float)0.5, (float)0.5);
    _characterIcon = characterInstance;
    AddChild(characterInstance);
  }

  public override void _Process(float delta)
  {
    _distanceLabel.Text = Distance.ToString();
    if (CharacterRef != null)
    {
      if (_isLeftIndicator)
      {
        Visible = CharacterRef.Position.x < 0;
      }
      else
      {
        Visible = CharacterRef.Position.x > GetViewport().Size.x;
      }
    }
  }

  public void UpdateSkin(string characterSkin)
  {
    if (_characterIcon != null)
    {
      _characterIcon.CharacterSkin = characterSkin;
    }
  }
}
