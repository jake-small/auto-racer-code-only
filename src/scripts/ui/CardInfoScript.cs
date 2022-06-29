using System.Collections.Generic;
using Godot;

public class CardInfoScript : Node2D
{
  public Card Card { get; private set; }

  private Label _selectedCardNameLabel;
  private RichTextLabel _selectedCardDescriptionLabel;
  private Label _selectedCardPhaseLabel;
  private Label _selectedCardTierLabel;
  private const string LabelSelectedNamePath = "MarginContainer/VBoxContainer/Label_selected_name";
  private const string LabelSelectedDescriptionPath = "MarginContainer/VBoxContainer/Label_selected_description";
  private const string LabelSelectedPhasePath = "MarginContainer/VBoxContainer2/HSplitContainer/Label_selected_phase";
  private const string LabelSelectedTierPath = "MarginContainer/VBoxContainer2/HSplitContainer/Label_selected_tier";

  public override void _Ready()
  {
    _selectedCardNameLabel = GetNode<Label>(LabelSelectedNamePath);
    _selectedCardDescriptionLabel = GetNode<RichTextLabel>(LabelSelectedDescriptionPath);
    _selectedCardPhaseLabel = GetNode<Label>(LabelSelectedPhasePath);
    _selectedCardTierLabel = GetNode<Label>(LabelSelectedTierPath);
  }

  public void SetCard(Card card)
  {
    if (card == null)
    {
      Visible = false;
      _selectedCardNameLabel.Text = "";
      _selectedCardDescriptionLabel.Text = "";
      _selectedCardPhaseLabel.Text = "";
      _selectedCardTierLabel.Text = "";
      Card = null;
      return;
    }

    Visible = true;
    _selectedCardNameLabel.Text = card.GetName();
    var cardDescription = card.GetDescription();
    var tags = new Dictionary<string, string>{
      {"[positive]", "res://assets/effects/icon102_c.png"},
      {"[negative]", "res://assets/effects/icon105_c.png"},
    };
    ReplaceTagsInRichText(cardDescription, _selectedCardDescriptionLabel, tags);
    _selectedCardPhaseLabel.Text = card.GetAbilityPhase();
    _selectedCardTierLabel.Text = $"Tier {card.Tier}";
  }

  public void Clear()
  {
    SetCard(null);
  }

  private void ReplaceTagsInRichText(string description, RichTextLabel richLabel, Dictionary<string, string> tags)
  {
    richLabel.Text = "";
    while (description.Length > 0)
    {
      var tagStartIndex = description.IndexOf("[");
      if (tagStartIndex == -1)
      {
        richLabel.AddText(description);
        return;
      }
      var preTagString = description.Substring(0, tagStartIndex);
      richLabel.AddText(preTagString);
      description = description.Substring(tagStartIndex);

      var tagEndIndex = description.IndexOf("]");
      if (tagEndIndex == -1)
      {
        richLabel.AddText(description);
        return;
      }
      var tagString = description.Substring(0, tagEndIndex + 1);
      richLabel.AddImage((Texture)GD.Load(tags[tagString]));
      description = description.Substring(tagEndIndex + 1);
    }
  }
}