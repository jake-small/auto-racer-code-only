// using Godot;
// using System;
// using System.Collections.Generic;
// using System.Linq;

// public class ItemScriptBackup : KinematicBody2D
// {
//   private readonly float DragSpeed = 40f;

//   private float _draggingDistance;
//   private bool _dragging = false;
//   private Vector2 _direction = new Vector2();
//   private bool _dropped = false;
//   private bool _bought = false;
//   private Vector2 _startingPosition = new Vector2();
//   private Vector2 _dragPosition = new Vector2();
//   private Vector2 _droppedPosition = new Vector2();
//   private bool _mouseIn = false;
//   private bool _hovered = false;
//   private bool _selected = false;
//   private readonly Vector2 ItemSlotOffset = new Vector2(6, 4);
//   private List<Sprite> _itemSlots = new List<Sprite>();
//   private Sprite _currentItemSlot = null;
//   private Sprite _selectedSprite = new Sprite();

//   public Card _item { get; set; }

//   public override void _Ready()
//   {
//     var headerLabel = GetNode<Label>("Label_header");
//     var bodyLabel = GetNode<Label>("Label_body");
//     headerLabel.Text = _item.Name;
//     bodyLabel.Text = _item.Body;

//     _startingPosition = Position;
//     var itemSlotNodes = GetTree().GetNodesInGroup("ItemSlots");
//     foreach (Sprite sprite in itemSlotNodes)
//     {
//       _itemSlots.Add(sprite);
//     }
//     _selectedSprite = GetNode("Panel_selected") as Sprite;
//   }

//   public override void _Process(float delta)
//   {
//     if (_selected)
//     {
//       _selectedSprite.Visible = true;
//     }
//     else
//     {
//       _selectedSprite.Visible = false;
//     }
//   }

//   public override void _PhysicsProcess(float delta)
//   {
//     if (_dragging)
//     {
//       MoveAndSlide((_dragPosition - Position) * DragSpeed);
//     }
//     else if (_dropped)
//     {
//       Position = _droppedPosition;
//       _dropped = false;
//     }
//   }

//   public override void _Input(InputEvent @event)
//   {
//     if (@event is InputEventMouseButton eventMouseButton)
//     {
//       if (_hovered && eventMouseButton.IsPressed() && _mouseIn)
//       {
//         // _dropped = false;
//         GD.Print("Selecting item at: ", eventMouseButton.Position);
//         var mousePosition = GetViewport().GetMousePosition();
//         _draggingDistance = Position.DistanceTo(mousePosition);
//         _direction = (mousePosition - Position).Normalized();
//         _dragging = true;
//         _dragPosition = mousePosition - (_draggingDistance * _direction);
//       }
//       // item in shop slot
//       else if (_selected && !_bought)
//       {
//         GD.Print("Hovered & NOT bought");
//         _dragging = false;
//         _hovered = false;
//         var mousePosition = GetViewport().GetMousePosition();
//         foreach (var itemSlot in _itemSlots)
//         {
//           var rect = itemSlot.GetRect();
//           rect.Position = itemSlot.Position;
//           rect.Size = rect.Size * itemSlot.Scale;
//           if (rect.HasPoint(mousePosition))
//           {
//             // lock in spell
//             _droppedPosition = rect.Position + ItemSlotOffset;
//             _selected = false;
//             _currentItemSlot = itemSlot;
//             // _item.Slot = GetSlotNumberFromName(_currentItemSlot.Name);
//             var slot = GetSlotNumberFromName(_currentItemSlot.Name);
//             emitDroppedInSlotSignal(this, slot, _droppedPosition, _startingPosition);
//             // TODO
//             // _dropped = true;
//             //_bought = true;
//             // _startingPosition = _droppedPosition;
//           }
//         }
//         if (!_bought && _mouseIn)
//         {
//           // put item back in player slot
//           _droppedPosition = _startingPosition;
//           _dropped = true;
//         }
//         else if (!_bought)
//         {
//           // put item back in shop slot
//           _droppedPosition = _startingPosition;
//           _dropped = true;
//           _selected = false;
//         }
//       }
//       // item in player slot
//       else if (_selected && _bought)
//       {
//         GD.Print("Hovered & bought");
//         _dragging = false;
//         _hovered = false;
//         var mousePosition = GetViewport().GetMousePosition();
//         var movedSlots = false;
//         foreach (var itemSlot in _itemSlots)
//         {
//           if (_currentItemSlot == itemSlot)
//           {
//             continue;
//           }
//           var rect = itemSlot.GetRect();
//           rect.Position = itemSlot.Position;
//           rect.Size = rect.Size * itemSlot.Scale;
//           if (rect.HasPoint(mousePosition))
//           {
//             // lock in spell
//             GD.Print("Dropped in slot at: ", mousePosition);
//             _droppedPosition = rect.Position + ItemSlotOffset;
//             // _startingPosition = _droppedPosition;
//             // _dropped = true;
//             movedSlots = true;
//             _selected = false;
//             _currentItemSlot = itemSlot;
//             // _item.Slot = GetSlotNumberFromName(_currentItemSlot.Name);
//             var slot = GetSlotNumberFromName(_currentItemSlot.Name);
//             emitDroppedInSlotSignal(this, slot, _droppedPosition, _startingPosition);
//           }
//         }
//         if (!movedSlots && _mouseIn)
//         {
//           // put item back in player slot
//           _droppedPosition = _startingPosition;
//           _dropped = true;
//         }
//         else if (!movedSlots)
//         {
//           // put item back in player slot
//           _droppedPosition = _startingPosition;
//           _dropped = true;
//           _selected = false;
//         }
//       }
//       else
//       {
//         _dragging = false;
//         _hovered = false;
//       }
//     }
//     else if (@event is InputEventMouseMotion eventMouseMotion)
//     {
//       if (_dragging)
//       {
//         var mousePosition = GetViewport().GetMousePosition();
//         _dragPosition = mousePosition - (_draggingDistance * _direction);
//       }
//     }
//   }

//   public void _on_input_event(Node viewport, InputEvent inputEvent, int shape_idx)
//   {
//     if (inputEvent.IsPressed())
//     {
//       GD.Print("Mouse in pressed");
//       _selected = true;
//     }
//     else
//     {
//       _hovered = true;
//     }
//   }

//   public void _on_mouse_entered()
//   {
//     _mouseIn = true;
//   }

//   public void _on_mouse_exited()
//   {
//     _mouseIn = false;
//   }

//   [Signal]
//   public delegate void droppedInSlot(Card item, KinematicBody2D itemNode, int slot, Vector2 droppedPosition, Vector2 originalPosition);
//   public void emitDroppedInSlotSignal(KinematicBody2D itemNode, int slot, Vector2 droppedPosition, Vector2 originalPosition)
//   {
//     GD.Print($"Drop signal emitted for {_item.Name} at slot {_item.Slot} to {slot} at position {droppedPosition}");
//     EmitSignal(nameof(droppedInSlot), _item, itemNode, slot, droppedPosition, originalPosition);
//   }

//   private int GetSlotNumberFromName(string name)
//   {
//     return name.Split('_').LastOrDefault().ToInt();
//   }
// }