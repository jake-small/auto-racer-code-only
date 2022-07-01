using System.Linq;
using Godot;

public class GameEndMain : Node2D
{
  public override void _Ready()
  {
    var endRaceButton = GetNode<TextureButton>(GameEndSceneData.ButtonFinishPath);
    endRaceButton.Connect("pressed", this, nameof(Button_finish_pressed));

    var firstPlacesLabel = GetNode<Label>(GameEndSceneData.LabelNumFirstPlaces);
    var secondPlacesLabel = GetNode<Label>(GameEndSceneData.LabelNumSecondPlaces);
    var thirdPlacesLabel = GetNode<Label>(GameEndSceneData.LabelNumThirdPlaces);
    var fourthPlacesLabel = GetNode<Label>(GameEndSceneData.LabelNumFourthPlaces);
    if (GameManager.NumPlayers > 0)
    {
      firstPlacesLabel.Text = GameManager.Score.GetResult(0).ToString();
    }
    else
    {
      GetNode<Sprite>("Container_win_record/trophy").Visible = false;
      firstPlacesLabel.Visible = false;
    }
    if (GameManager.NumPlayers > 1)
    {
      secondPlacesLabel.Text = GameManager.Score.GetResult(1).ToString();
    }
    else
    {
      GetNode<Sprite>("Container_win_record/medal2").Visible = false;
      secondPlacesLabel.Visible = false;
    }
    if (GameManager.NumPlayers > 2)
    {
      thirdPlacesLabel.Text = GameManager.Score.GetResult(2).ToString();
    }
    else
    {
      GetNode<Sprite>("Container_win_record/medal1").Visible = false;
      thirdPlacesLabel.Visible = false;
    }
    if (GameManager.NumPlayers > 3)
    {
      fourthPlacesLabel.Text = GameManager.Score.GetResult(3).ToString();
    }
    else
    {
      GetNode<Sprite>("Container_win_record/star").Visible = false;
      fourthPlacesLabel.Visible = false;
    }

    if (GameManager.NumPlayers == 2)
    {
      firstPlacesLabel.RectPosition = new Vector2(firstPlacesLabel.RectPosition.x + 55, firstPlacesLabel.RectPosition.y);
      var firstPlaceIcon = GetNode<Sprite>("Container_win_record/trophy");
      firstPlaceIcon.Position = new Vector2(firstPlaceIcon.Position.x + 55, firstPlaceIcon.Position.y);
      secondPlacesLabel.RectPosition = new Vector2(secondPlacesLabel.RectPosition.x + 55, secondPlacesLabel.RectPosition.y);
      var secondPlaceIcon = GetNode<Sprite>("Container_win_record/medal2");
      secondPlaceIcon.Position = new Vector2(secondPlaceIcon.Position.x + 55, secondPlaceIcon.Position.y);
    }
    PlayerInventoryFill();
  }

  private void PlayerInventoryFill()
  {
    var cardDict = GameManager.LocalPlayer?.Cards;
    foreach (var (slot, card) in cardDict.Select(d => (d.Key, d.Value)))
    {
      if (card is CardEmpty)
      {
        continue;
      }
      CreateCardScript(card, slot);
      var addResult = GameManager.PrepEngine.PlayerInventory.AddCard(card, slot);
      if (!addResult)
      {
        GD.Print($"Error adding card to shop inventory {card.GetRawName()} at slot {slot}");
      }
    }
  }

  private void CreateCardScript(Card card, int slot)
  {
    var cardScene = ResourceLoader.Load(PrepSceneData.CardScenePath) as PackedScene;
    var cardInstance = (CardScript)cardScene.Instance();
    var spriteName = "slot_";
    var containerPosition = GetNode<Sprite>($"{spriteName}{slot}").Position;
    var position = containerPosition + PrepSceneData.CardSlotOffset;
    cardInstance.DisplayOnly = true;
    cardInstance.Card = card;
    cardInstance.Slot = slot;
    cardInstance.Position = position;
    cardInstance.Card.InventoryType = InventoryType.Player;
    AddChild(cardInstance);
  }

  private void Button_finish_pressed()
  {
    GD.Print("Game over button pressed");
    GetTree().ChangeScene("res://src/scenes/menus/MainMenu.tscn");
  }
}