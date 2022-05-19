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
    firstPlacesLabel.Text = GameManager.Score.GetResult(0).ToString();
    secondPlacesLabel.Text = GameManager.Score.GetResult(1).ToString();
    thirdPlacesLabel.Text = GameManager.Score.GetResult(2).ToString();
    fourthPlacesLabel.Text = GameManager.Score.GetResult(3).ToString();

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