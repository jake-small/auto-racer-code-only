using Godot;

public static class PrepSceneData
{
  public static readonly Vector2 CardSlotOffset = new Vector2(6, 4);

  public const string LabelPlayerName = "Label_player_name";
  public const string CharacterPath = "Character";
  public const string CardDataRelativePath = @"configs\cardsTest.json";
  public const string BankDataConfigRelativePath = @"configs\bank.config.json";
  public const string NameDataRelativePath = @"configs\nameData.json";
  public const string ButtonRerollPath = "Control/HBoxContainerLeft/Button_Reroll";
  public const string ButtonFreezePath = "Control/HBoxContainerLeft/Button_Freeze";
  public const string ButtonSellPath = "Control/HBoxContainerLeft/Button_Sell";
  public const string ButtonGoPath = "Control/HBoxContainerRight/Button_Go";
  public const string CardScenePath = "res://src/scenes/objects/cards/card.tscn";
  public const string GroupCardSlots = "CardSlots";
  public const string GroupCard = "Card";
  public const string PanelSpriteSelected = "Panel_selected";
  public const string PanelSpriteFrozen1 = "Panel_frozen";
  public const string PanelSpriteFrozen2 = "Panel_frozen2";
  public const string SpriteCardIcon = "Icon";
  public const string LabelCardLevel = "Label_level";
  public const string LabelCardBaseMove = "Label_basemove";
  public const string LabelNumFirstPlaces = "Container_win_record/Label_firstPlaceTotal";
  public const string LabelNumSecondPlaces = "Container_win_record/Label_secondPlaceTotal";
  public const string LabelNumThirdPlaces = "Container_win_record/Label_thirdPlaceTotal";
  public const string LabelNumFourthPlaces = "Container_win_record/Label_fourthPlaceTotal";
  public const string LabelRaceTotalPath = "Container_info/Label_raceTotal";
  public const string LabelHeartsPath = "Container_info/Label_hearts";
  public const string LabelCoinsPath = "Container_info/Label_coins";
  public const string ContainerSelectedCard = "Container_selected_card";
  public const string LabelSelectedNamePath = "Container_selected_card/MarginContainer/VBoxContainer/Label_selected_name";
  public const string LabelSelectedDescriptionPath = "Container_selected_card/MarginContainer/VBoxContainer/Label_selected_description";
  public const string LabelSelectedSellsForPath = "Container_selected_card/MarginContainer/VBoxContainer2/HSplitContainer/HSplitContainer/Label_selected_sellsForValue";
  public const string LabelSelectedBaseMovePath = "Container_selected_card/MarginContainer/VBoxContainer2/HSplitContainer/Label_selected_basemove";
  public const string ContainerCardCostPrefix = "Shop_cost_panel_";
  public const string LabelDebugInventory = "Debug/Label_inventory";
}
