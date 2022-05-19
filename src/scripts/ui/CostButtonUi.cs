using Godot;
using System;

public class CostButtonUi : ButtonUi
{
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

  public override void _Ready()
  {
    _costContainer = GetNode<Control>("Control_cost");
    _costLabel = GetNode<Label>("Control_cost/Label_cost");
    UpdateCostVisibility(CostVisible);
    UpdateCostLabel(Cost);
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