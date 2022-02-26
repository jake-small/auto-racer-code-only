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
  private Sprite _selectedSprite = new Sprite();
  private List<Sprite> _frozenSprites = new List<Sprite>();
  private Label _levelLabel;
  private Label _baseMoveLabel;

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
    var iconPath = $"res://assets/card_icons/{Card.Icon ?? "null"}";
    var directory = new Directory();
    if (directory.FileExists(iconPath))
    {
      var iconSprite = GetNode<Sprite>(PrepSceneData.SpriteCardIcon);
      iconSprite.Texture = (Texture)GD.Load(iconPath);
    }

    _levelLabel = GetNode<Label>(PrepSceneData.LabelCardLevel);
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
    if (Selected)
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
    else if (Card.InventoryType == InventoryType.Shop || Slot != potentialSlot)
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
      if (IsInShop())
      {
        GD.Print($"Card {Card.GetName()} dropped on freeze button");
        DroppedPosition = StartingPosition;
        Dropped = true;
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
    _levelLabel.Text = Card.Level.ToString() + " exp" + Card.Exp.ToString() + "/" + Card.ExpToLvl.ToString();
    _baseMoveLabel.Text = Card.BaseMove + "m";
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
}