public static class RaceSceneData
{
  public const string BackgroundTileMap1Path = "TileMap1";
  public const string BackgroundTileMap2Path = "TileMap2";
  public const string BackgroundTileMap3Path = "TileMap3";
  public const string ButtonFinishPath = "Control/HBoxContainerRight/Button_Finish";
  public const string ButtonForwardPath = "Control/HBoxContainerLeft/Button_Forward";
  public const string ButtonBackPath = "Control/HBoxContainerLeft/Button_Back";
  public const string ButtonAutoPlay = "Button_AutoPlay";
  public const string RichTextLabel_GameState = "MarginContainer/RichTextLabel_GameState";
  public const string Label_TurnPhase = "CenterContainer/Label_TurnPhase";
  public const string Label_CardP1 = "Control_cards/HBoxContainer/Label_p1/Label_p1_card";
  public const string Label_CardP2 = "Control_cards/HBoxContainer/Label_p2/Label_p2_card";
  public const string Label_CardP3 = "Control_cards/HBoxContainer/Label_p3/Label_p3_card";
  public const string Label_CardP4 = "Control_cards/HBoxContainer/Label_p4/Label_p4_card";
  public const string GroupProjectiles = "Projectile";
  public const string CharacterTokenPositiveLabel = "Label_token_positive";
  public const string CharacterTokenNegativeLabel = "Label_token_negative";
  public const string CharacterScenePath = "res://src/scenes/objects/characters/character.tscn";
  public const string CharacterSpritePath = "AnimatedSprite";
  public const string CharacterSoftLeftBoundPath = "Positions/p3";
  public const string CharacterSoftRightBoundPath = "Positions/p11";
  public const string CharacterHardLeftBoundPath = "Positions/p0";
  public const string CharacterHardRightBoundPath = "Positions/p14";
  public const int CharacterSpawnYOffset = 100;
  public const int SpaceWidth = 128;
  public const float GameSpeed = 600;

  // Offscreen indicator
  public const string OffscreenIndicatorL1 = "Offscreen_Indicators/Offscreen_IndicatorL1";
  public const string OffscreenIndicatorL2 = "Offscreen_Indicators/Offscreen_IndicatorL2";
  public const string OffscreenIndicatorL3 = "Offscreen_Indicators/Offscreen_IndicatorL3";
  public const string OffscreenIndicatorR1 = "Offscreen_Indicators/Offscreen_IndicatorR1";
  public const string OffscreenIndicatorR2 = "Offscreen_Indicators/Offscreen_IndicatorR2";
  public const string OffscreenIndicatorR3 = "Offscreen_Indicators/Offscreen_IndicatorR3";
  public const string OffscreenIndicatorLabelDistance = "Label_distance";
  public const string OffscreenIndicatorCharacterPosition = "Position2D_character";

  // Selected cards
  public const string ContainerSelectedCard = "CardSlots/Container_selected_card_";
  public const string LabelPlayerName0 = "CardSlots/slot_0/Label_PlayerName";
  public const string LabelPlayerName1 = "CardSlots/slot_1/Label_PlayerName";
  public const string LabelPlayerName2 = "CardSlots/slot_2/Label_PlayerName";
  public const string LabelPlayerName3 = "CardSlots/slot_3/Label_PlayerName";
  public const string SelectedSlot0 = "CardSlots/slot_0/Selected";
  public const string SelectedSlot1 = "CardSlots/slot_1/Selected";
  public const string SelectedSlot2 = "CardSlots/slot_2/Selected";
  public const string SelectedSlot3 = "CardSlots/slot_3/Selected";
  public const string LabelSelectedNameRelPath = "/MarginContainer/VBoxContainer/Label_selected_name";
  public const string LabelSelectedDescriptionRelPath = "/MarginContainer/VBoxContainer/Label_selected_description";
  public const string LabelSelectedPhaseRelPath = "/MarginContainer/VBoxContainer2/HSplitContainer/Label_selected_phase";
  public const string LabelSelectedTierRelPath = "/MarginContainer/VBoxContainer2/HSplitContainer/Label_selected_tier";

  // Phase Indicator
  public const string PhaseIndicatorAbilitiesPrefix = "Phase_display/Abilities";
  public const string PhaseIndicatorMovePrefix = "Phase_display/Move";
  public const string PhaseIndicatorRemainingTokens = "Phase_display/RemainingTokens";
}
