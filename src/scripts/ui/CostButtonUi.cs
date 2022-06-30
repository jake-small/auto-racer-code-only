using Godot;
using System;

public class CostButtonUi : ButtonUi
{
  private Vector2 _startingPosition;
  private float _moveDownAmount = 4;

  private bool _disabled;
  public new bool Disabled
  {
    get
    {
      return _disabled;
    }
    set
    {
      DisableCostButton(value);
      _disabled = value;
    }
  }

  private bool _costVisible = true;
  public bool CostVisible
  {
    get
    {
      return _costVisible;
    }
    set
    {
      UpdateCostVisibility(value);
      _costVisible = value;
    }
  }

  private int? _cost;
  public int? Cost
  {
    get
    {
      return _cost;
    }
    set
    {
      UpdateCostLabel(value);
      _cost = value;
    }
  }

  private Control _costContainer;
  private Label _costLabel;
  private AnimationPlayer _animationPlayer;

  public override void _Ready()
  {
    _startingPosition = this.RectGlobalPosition;
    _costContainer = GetNode<Control>("Control_cost");
    _costLabel = GetNode<Label>("Control_cost/Label_cost");
    _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    UpdateCostVisibility(CostVisible);
    UpdateCostLabel(Cost);
  }

  public override void _Process(float delta)
  {
    if (_startingPosition != default(Vector2)
      && RectGlobalPosition != _startingPosition
      && Disabled)
    {
      SetGlobalPosition(_startingPosition);
    }
  }

  public new void _on_TextureButton_down()
  {
    var newPosition = RectGlobalPosition + new Vector2(0, _moveDownAmount);
    SetGlobalPosition(newPosition);
  }

  public new void _on_TextureButton_up()
  {
    SetGlobalPosition(_startingPosition);
  }

  public void OnHitEffect()
  {
    _animationPlayer.Stop(true);
    _animationPlayer.Play("OnHit");
  }

  private void DisableCostButton(bool disabled)
  {
    base.Disabled = disabled;
    _costContainer.Modulate = disabled ? new Color(Modulate.r, Modulate.g, Modulate.b, 0.2f) : new Color(Modulate.r, Modulate.g, Modulate.b, 1f);
  }

  private void UpdateCostVisibility(bool visible)
  {
    if (_costContainer == null)
    {
      return;
    }
    _costContainer.Visible = visible;
  }

  private void UpdateCostLabel(int? cost)
  {
    if (cost == null && _costLabel != null)
    {
      return;
    }
    _costLabel.Text = cost.ToString();
  }
}