using Godot;

public static class PrepSceneData
{
  public static readonly Vector2 CardSlotOffset = new Vector2(6, 4);

  public const string LabelPlayerName = "Label_player_name";
  public const string CharacterPath = "Character";
  public const string TutorialPath1 = "Container_tutorial";
  public const string TutorialPath2 = "Container_tutorial2";
  public const string CardDataRelativePath = @"configs\cardData.json";
  public const string BankDataConfigRelativePath = @"configs\bankData.json";
  public const string NameDataRelativePath = @"configs\nameData.json";
  public const string ButtonRerollPath = "Control/HBoxContainerLeft/Button_Reroll";
  public const string ButtonFreezePath = "Control/HBoxContainerLeft/Button_Freeze";
  public const string ButtonSellPath = "Control/HBoxContainerLeft/Button_Sell";
  public const string ButtonGoPath = "Control/HBoxContainerRight/Button_Go";
  public const string AnimationPlayerGoldPath = "Container_info/panel_runeBlue2/AnimationPlayer";
  public const string CardScenePath = "res://src/scenes/objects/cards/card.tscn";
  public const string GroupCardSlots = "CardSlots";
  public const string GroupCard = "Card";
  public const string GroupProjectiles = "Projectile";
  public const string PanelSpriteSelected = "Panel_selected";
  public const string PanelSpriteFrozen1 = "Panel_frozen";
  public const string PanelSpriteFrozen2 = "Panel_frozen2";
  public const string SpriteCardBackground = "Panel_blue";
  public const string SpriteCardIcon = "Icon";
  public const string NodeExp = "Exp";
  public const string LabelCardLevel = "Exp/Label_level";
  public const string SpriteExpFull1 = "Exp/Sprite_exp_1_full";
  public const string SpriteExpFull2 = "Exp/Sprite_exp_2_full";
  public const string SpriteExpFull3 = "Exp/Sprite_exp_3_full";
  public const string SpriteExpEmpty1 = "Exp/Sprite_exp_1_empty";
  public const string SpriteExpEmpty2 = "Exp/Sprite_exp_2_empty";
  public const string SpriteExpEmpty3 = "Exp/Sprite_exp_3_empty";
  public const string LabelCardBaseMove = "Base_move/Label_basemove";
  public const string LabelNumFirstPlaces = "Container_win_record/Label_firstPlaceTotal";
  public const string LabelNumSecondPlaces = "Container_win_record/Label_secondPlaceTotal";
  public const string LabelNumThirdPlaces = "Container_win_record/Label_thirdPlaceTotal";
  public const string LabelNumFourthPlaces = "Container_win_record/Label_fourthPlaceTotal";
  public const string LabelRaceTotalPath = "Container_Race_Count/Label_raceTotal";
  public const string LabelHeartsPath = "Container_info/Label_hearts";
  public const string LabelCoinsPath = "Container_info/Label_coins";
  public const string SelectedCardInfo = "Selected_card_info";
  public const string ShopSlotPrefix = "shop_slot_";
  public const string ContainerCardCostPrefix = "Shop_cost_";
  public const string PlaceIconFirst = "Container_win_record/Icon_first";
  public const string PlaceIconSecond = "Container_win_record/Icon_second";
  public const string PlaceIconThird = "Container_win_record/Icon_third";
  public const string PlaceIconFourth = "Container_win_record/Icon_fourth";
  public const string LabelDebugInventory = "Debug/Label_inventory";
}
