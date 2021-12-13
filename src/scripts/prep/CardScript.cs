using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CardScript : KinematicBody2D
{
  private readonly float DragSpeed = 40f;

  private float _draggingDistance;
  private bool _dragging = false;
  private Vector2 _direction = new Vector2();
  private bool _dropped = false;
  private Vector2 _startingPosition = new Vector2();
  private Vector2 _dragPosition = new Vector2();
  private Vector2 _droppedPosition = new Vector2();
  private bool _mouseIn = false;
  private bool _selected = false;
  private readonly Vector2 CardSlotOffset = new Vector2(6, 4);
  private List<Sprite> _cardSlots = new List<Sprite>();
  private int _currentCardSlot = -1;
  private Sprite _selectedSprite = new Sprite();

  public Card _card { get; set; }

  public override void _Ready()
  {
    var headerLabel = GetNode<Label>(PrepSceneData.CardLabelHeader);
    var bodyLabel = GetNode<Label>(PrepSceneData.CardLabelBody);
    headerLabel.Text = _card.Name;
    bodyLabel.Text = _card.Body;

    _startingPosition = Position;
    var cardSlotNodes = GetTree().GetNodesInGroup(PrepSceneData.GroupCardSlots);
    foreach (Sprite sprite in cardSlotNodes)
    {
      _cardSlots.Add(sprite);
    }
    _selectedSprite = GetNode(PrepSceneData.SelectedPanelSprite) as Sprite;
  }

  public override void _Process(float delta)
  {
    if (_selected)
    {
      _selectedSprite.Visible = true;
    }
    else
    {
      _selectedSprite.Visible = false;
    }
  }

  public override void _PhysicsProcess(float delta)
  {
    if (_dragging)
    {
      MoveAndSlide((_dragPosition - Position) * DragSpeed);
    }
    if (_dropped)
    {
      Position = _droppedPosition;
      _dropped = false;
    }
  }

  /*public void _InputBACKUP(InputEvent @event)
  {
    if (@event is InputEventMouseButton eventMouseButton)
    {
      if (_hovered && eventMouseButton.IsPressed() && _mouseIn)
      {
        // _dropped = false;
        GD.Print("Selecting item at: ", eventMouseButton.Position);
        var mousePosition = GetViewport().GetMousePosition();
        _draggingDistance = Position.DistanceTo(mousePosition);
        _direction = (mousePosition - Position).Normalized();
        _dragging = true;
        _dragPosition = mousePosition - (_draggingDistance * _direction);
      }
      // item in shop slot
      else if (_selected && !_bought)
      {
        GD.Print("Hovered & NOT bought");
        _dragging = false;
        _hovered = false;
        var mousePosition = GetViewport().GetMousePosition();
        foreach (var itemSlot in _itemSlots)
        {
          var rect = itemSlot.GetRect();
          rect.Position = itemSlot.Position;
          rect.Size = rect.Size * itemSlot.Scale;
          if (rect.HasPoint(mousePosition))
          {
            // lock in spell
            _droppedPosition = rect.Position + ItemSlotOffset;
            _selected = false;
            _currentItemSlot = itemSlot;
            // _item.Slot = GetSlotNumberFromName(_currentItemSlot.Name);
            var slot = GetSlotNumberFromName(_currentItemSlot.Name);
            emitDroppedInSlotSignal(this, slot, _droppedPosition, _startingPosition);
            // TODO
            // _dropped = true;
            //_bought = true;
            // _startingPosition = _droppedPosition;
          }
        }
        if (!_bought && _mouseIn)
        {
          // put item back in player slot
          _droppedPosition = _startingPosition;
          _dropped = true;
        }
        else if (!_bought)
        {
          // put item back in shop slot
          _droppedPosition = _startingPosition;
          _dropped = true;
          _selected = false;
        }
      }
      // item in player slot
      else if (_selected && _bought)
      {
        GD.Print("Hovered & bought");
        _dragging = false;
        _hovered = false;
        var mousePosition = GetViewport().GetMousePosition();
        var movedSlots = false;
        foreach (var itemSlot in _itemSlots)
        {
          if (_currentItemSlot == itemSlot)
          {
            continue;
          }
          var rect = itemSlot.GetRect();
          rect.Position = itemSlot.Position;
          rect.Size = rect.Size * itemSlot.Scale;
          if (rect.HasPoint(mousePosition))
          {
            // lock in spell
            GD.Print("Dropped in slot at: ", mousePosition);
            _droppedPosition = rect.Position + ItemSlotOffset;
            // _startingPosition = _droppedPosition;
            // _dropped = true;
            movedSlots = true;
            _selected = false;
            _currentItemSlot = itemSlot;
            // _item.Slot = GetSlotNumberFromName(_currentItemSlot.Name);
            var slot = GetSlotNumberFromName(_currentItemSlot.Name);
            emitDroppedInSlotSignal(this, slot, _droppedPosition, _startingPosition);
          }
        }
        if (!movedSlots && _mouseIn)
        {
          // put item back in player slot
          _droppedPosition = _startingPosition;
          _dropped = true;
        }
        else if (!movedSlots)
        {
          // put item back in player slot
          _droppedPosition = _startingPosition;
          _dropped = true;
          _selected = false;
        }
      }
      else
      {
        _dragging = false;
        _hovered = false;
      }
    }
    else if (@event is InputEventMouseMotion eventMouseMotion)
    {
      if (_dragging)
      {
        var mousePosition = GetViewport().GetMousePosition();
        _dragPosition = mousePosition - (_draggingDistance * _direction);
      }
    }
  }
  */

  public override void _Input(InputEvent @event)
  {
    if (!_selected || _dropped)
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
    else if (_currentCardSlot != potentialSlot)
    {
      emitDroppedInSlotSignal(potentialSlot, _droppedPosition, _startingPosition);
      return;
    }

    if (_mouseIn)
    {
      // mouse released right after picking up card, don't deselect it
      _droppedPosition = _startingPosition;
      _dropped = true;
    }
    else
    {
      // put card back in shop slot
      _droppedPosition = _startingPosition;
      _dropped = true;
      _selected = false;
    }
  }

  public void _on_input_event(Node viewport, InputEvent inputEvent, int shape_idx)
  {
    if (inputEvent is InputEventMouseButton eventMouseButton)
    {
      if (inputEvent.IsPressed())
      {
        GD.Print("Selected card at: ", eventMouseButton.Position);
        _selected = true; // TODO

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
        _droppedPosition = rect.Position + CardSlotOffset;
        return GetSlotNumberFromName(cardSlot.Name);
      }
    }
    return -1;
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
  public delegate void droppedInSlot(Card card, int slot, Vector2 droppedPosition, Vector2 originalPosition);
  public void emitDroppedInSlotSignal(int slot, Vector2 droppedPosition, Vector2 originalPosition)
  {
    GD.Print($"Drop signal EMITTED for {_card.Name} at slot {_card.Slot} to {slot} at position {droppedPosition}");
    _card.CardNode = this;
    EmitSignal(nameof(droppedInSlot), _card, slot, droppedPosition, originalPosition);
  }
}