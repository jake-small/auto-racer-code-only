using Godot;
using System.Collections.Generic;
using System.Linq;

public class CardScript : KinematicBody2D
{
  private readonly float _dragSpeed = 40f;
  private float _draggingDistance;
  private bool _dragging = false;
  private Vector2 _direction = new Vector2();
  private Vector2 _dragPosition = new Vector2();
  private bool _mouseIn = false;
  private List<Sprite> _cardSlots = new List<Sprite>();
  private Sprite _backgroundSprite;
  private Sprite _selectedSprite;
  private List<Sprite> _frozenSprites = new List<Sprite>();
  private Node2D _expNode;
  private Label _levelLabel;
  private Sprite _expFull1;
  private Sprite _expFull2;
  private Sprite _expFull3;
  private List<Sprite> _expFullIcons;
  private Sprite _expEmpty3;
  private Label _baseMoveLabel;

  public bool DisplayOnly { get; set; } = false;
  public bool Selected = false;
  public bool Dropped = false;
  public Vector2 DroppedPosition = new Vector2();
  public Vector2 StartingPosition = new Vector2();
  public int Slot;
  public bool MouseInCardActionButton = false;
  public Card Card { get; set; }

  public CardScript() { }
  public CardScript(Card card)
  {
    Card = card;
  }

  public override void _Ready()
  {
    var iconPath = $"res://assets/card_icons/{Card.Icon}"; // ?? "icon023"
    if (ResourceLoader.Exists(iconPath))
    {
      var iconSprite = GetNode<Sprite>(PrepSceneData.SpriteCardIcon);
      iconSprite.Texture = (Texture)GD.Load(iconPath);
    }
    else
    {
      GD.Print($"Unable to load iconSprite with path '{iconPath}'. Using default sprite instead");
    }
    _backgroundSprite = GetNode<Sprite>(PrepSceneData.SpriteCardBackground);
    _expNode = GetNode<Node2D>(PrepSceneData.NodeExp);
    _levelLabel = GetNode<Label>(PrepSceneData.LabelCardLevel);
    _expFull1 = GetNode<Sprite>(PrepSceneData.SpriteExpFull1);
    _expFull2 = GetNode<Sprite>(PrepSceneData.SpriteExpFull2);
    _expFull3 = GetNode<Sprite>(PrepSceneData.SpriteExpFull3);
    _expEmpty3 = GetNode<Sprite>(PrepSceneData.SpriteExpEmpty3);
    _expFullIcons = new List<Sprite>() { _expFull1, _expFull2, _expFull3 };
    _baseMoveLabel = GetNode<Label>(PrepSceneData.LabelCardBaseMove);
    UpdateUi();

    StartingPosition = Position;
    var cardSlotNodes = GetTree().GetNodesInGroup(PrepSceneData.GroupCardSlots);
    foreach (Sprite sprite in cardSlotNodes)
    {
      _cardSlots.Add(sprite);
    }
    _selectedSprite = GetNode(PrepSceneData.PanelSpriteSelected) as Sprite;
    var frozenSprite1 = GetNode(PrepSceneData.PanelSpriteFrozen1) as Sprite;
    var frozenSprite2 = GetNode(PrepSceneData.PanelSpriteFrozen2) as Sprite;
    _frozenSprites.Add(frozenSprite1);
    _frozenSprites.Add(frozenSprite2);
  }

  public override void _Process(float delta)
  {
    if (Selected && _selectedSprite != null)
    {
      _selectedSprite.Visible = true;
    }
    else
    {
      _selectedSprite.Visible = false;
    }

    if (Card.Frozen)
    {
      foreach (var frozenSprite in _frozenSprites)
      {
        frozenSprite.Visible = true;
      }
    }
    else
    {
      foreach (var frozenSprite in _frozenSprites)
      {
        frozenSprite.Visible = false;
      }
    }
  }

  public override void _PhysicsProcess(float delta)
  {
    if (_dragging)
    {
      MoveAndSlide((_dragPosition - Position) * _dragSpeed);
    }
    if (Dropped)
    {
      Position = DroppedPosition;
      Dropped = false;
    }
  }

  public override void _Input(InputEvent @event)
  {
    if (DisplayOnly)
    {
      return;
    }

    if (!Selected || Dropped)
    {
      return;
    }

    var mousePosition = GetViewport().GetMousePosition();

    if ((@event is InputEventMouseMotion eventMouseMotion))
    {
      if (_dragging)
      {
        _dragPosition = mousePosition - (_draggingDistance * _direction);
      }
      return;
    }

    if (!(@event is InputEventMouseButton eventMouseButton))
    {
      return;
    }

    _dragging = false;
    var potentialSlot = CheckIfDroppedInSlot();
    if (potentialSlot == -1)
    {
      GD.Print("not dropped in slot");
    }
    else if (IsInShop() || Slot != potentialSlot)
    {
      emitDroppedInSlotSignal(potentialSlot, DroppedPosition, StartingPosition);
      return;
    }

    if (_mouseIn)
    {
      // mouse released right after picking up card, don't deselect it
      DroppedPosition = StartingPosition;
      Dropped = true;
    }
    else if (MouseInCardActionButton) // Card is dragged over either Freeze or Sell button
    {
      if (!eventMouseButton.IsActionReleased("m1") && !OsFeatures.IsTouchScreen())
      {
        return;
      }

      if (IsInShop())
      {
        GD.Print($"Card {Card.GetName()} dropped on freeze button");
        emitDroppedOnFreezeButtonSignal();
      }
      else
      {
        GD.Print($"Card {Card.GetName()} dropped on sell button");
        emitDroppedOnSellButtonSignal();
      }
      DeselectCard();
    }
    else
    {
      // put card back in shop slot
      DroppedPosition = StartingPosition;
      Dropped = true;
      DeselectCard();
    }
  }

  public void _on_input_event(Node viewport, InputEvent inputEvent, int shape_idx)
  {
    if (inputEvent is InputEventMouseButton eventMouseButton)
    {
      if (inputEvent.IsPressed())
      {
        if (DisplayOnly)
        {
          emitCardDisplaySelectedSignal();
          return;
        }
        GD.Print("Selected card at: ", eventMouseButton.Position);
        Selected = true; // TODO
        emitCardSelectedSignal();
        StartDraggingCard();
      }
    }
  }

  public void _on_mouse_entered()
  {
    _mouseIn = true;
  }

  public void _on_mouse_exited()
  {
    _mouseIn = false;
  }


  public void UpdateUi()
  {
    _baseMoveLabel.Text = Card.BaseMove.ToString();
    if (IsInShop())
    {
      _expNode.Visible = false;
      return;
    }
    _expNode.Visible = true;
    _levelLabel.Text = Card.Level.ToString();
    var exp = Card.Exp;
    var expToLvl = Card.ExpToLvl;
    if (expToLvl == 3)
    {
      _expEmpty3.Visible = false;
    }
    else if (expToLvl > 3)
    {
      _expEmpty3.Visible = true;
    }
    _expFull1.Visible = false;
    _expFull2.Visible = false;
    _expFull3.Visible = false;
    var expVisible = exp - 1;
    for (int i = 0; i < expVisible; i++)
    {
      _expFullIcons[i].Visible = true;
    }
  }

  public Sprite GetBackgroundSprite()
  {
    return _backgroundSprite;
  }

  public bool IsInShop()
  {
    return Card.InventoryType == InventoryType.Shop;
  }

  private void StartDraggingCard()
  {
    var mousePosition = GetViewport().GetMousePosition();
    _draggingDistance = Position.DistanceTo(mousePosition);
    _direction = (mousePosition - Position).Normalized();
    _dragging = true;
    _dragPosition = mousePosition - (_draggingDistance * _direction);
  }

  private int CheckIfDroppedInSlot()
  {
    var mousePosition = GetViewport().GetMousePosition();
    foreach (var cardSlot in _cardSlots)
    {
      var rect = GetRect(cardSlot);
      if (rect.HasPoint(mousePosition))
      {
        // lock in spell
        DroppedPosition = rect.Position + PrepSceneData.CardSlotOffset;
        return GetSlotNumberFromName(cardSlot.Name);
      }
    }
    return -1;
  }

  private void DeselectCard()
  {
    Selected = false;
    emitCardDeselectedSignal();
  }

  private Rect2 GetRect(Sprite sprite)
  {
    var rect = sprite.GetRect();
    rect.Position = sprite.Position;
    rect.Size = rect.Size * sprite.Scale;
    return rect;
  }

  private int GetSlotNumberFromName(string name)
  {
    return name.Split('_').LastOrDefault().ToInt();
  }

  [Signal]
  public delegate void droppedInSlot(CardScript cardScript, int slot, Vector2 droppedPosition, Vector2 originalPosition);
  public void emitDroppedInSlotSignal(int slot, Vector2 droppedPosition, Vector2 originalPosition)
  {
    GD.Print($"Drop signal EMITTED for {Card.GetName()} at slot {Slot} to {slot} at position {droppedPosition}");
    EmitSignal(nameof(droppedInSlot), this, slot, droppedPosition, originalPosition);
  }

  [Signal]
  public delegate void droppedOnFreezeButton(CardScript card);
  public void emitDroppedOnFreezeButtonSignal()
  {
    GD.Print($"DroppedOnFreezeButton signal EMITTED for {Card.GetName()} at slot {Slot}");
    EmitSignal(nameof(droppedOnFreezeButton), this);
  }

  [Signal]
  public delegate void droppedOnSellButton(CardScript card);
  public void emitDroppedOnSellButtonSignal()
  {
    GD.Print($"DroppedOnSellButton signal EMITTED for {Card.GetName()} at slot {Slot}");
    EmitSignal(nameof(droppedOnSellButton), this);
  }

  [Signal]
  public delegate void cardSelected(CardScript card);
  public void emitCardSelectedSignal()
  {
    GD.Print($"CardSelected signal EMITTED for {Card.GetName()} at slot {Slot}");
    EmitSignal(nameof(cardSelected), this);
  }

  [Signal]
  public delegate void cardDeselected(CardScript card);
  public void emitCardDeselectedSignal()
  {
    GD.Print($"CardDeselected signal EMITTED for {Card.GetName()} at slot {Slot}");
    EmitSignal(nameof(cardDeselected), this);
  }

  [Signal]
  public delegate void cardDisplaySelected(CardScript card);
  public void emitCardDisplaySelectedSignal()
  {
    GD.Print($"CardDisplaySelected signal EMITTED for {Card.GetName()} at slot {Slot}");
    EmitSignal(nameof(cardDisplaySelected), this);
  }
}