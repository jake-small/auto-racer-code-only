using Godot;
using System;

public class PrepProjectile : Sprite
{
  public Vector2 Target { get; set; }
  public Vector2 TargetSize { get; set; }
  public Vector2 TargetCenter { get; set; }
  public Card TargetCard { get; set; }
  public float? DelayedTakeoffAmount { get; set; }
  public bool SelfBuff { get; set; }
  public Action<Card> EffectEvent { get; set; }

  private const float StartingSpeed = 600;
  private float _speed = StartingSpeed;
  private Vector2 _spawn;
  private float _time = 0;
  private const float TowardsStrength = 0.5f;
  private const float PerpendicularStrength = 0.5f;
  private bool _transparent = true;
  private float _transparentValue = 0.5f;
  private bool _isDespawning = false;
  private float _despawnTimer = 0;

  public override void _Ready()
  {
    SpawnPosition();
    AddToGroup(PrepSceneData.GroupProjectiles);
    Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, _transparentValue);
    if (TargetCenter == null || TargetCenter == default(Vector2))
    {
      var centerX = Target.x + (TargetSize.x / 2);
      var centerY = Target.y + (TargetSize.y / 2);
      TargetCenter = new Vector2(centerX, centerY);
    }
    _spawn = GlobalPosition;
  }

  public override void _Process(float delta)
  {
    if (TargetCenter == null || TargetCenter == default(Vector2) || DelayedTakeoffAmount == null)
    {
      return;
    }

    _time += delta;
    if (_time <= DelayedTakeoffAmount)
    {
      return;
    }

    if (_transparent)
    {
      Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 1f);
      _transparent = false;
    }

    if (_isDespawning)
    {
      _despawnTimer += delta;
      Despawn();
      return;
    }

    if (TargetCenter != Position)
    {
      if (_time > 5 + DelayedTakeoffAmount)
      {
        Despawn();
        return;
      }
      var distanceToTarget = GlobalPosition.DistanceTo(TargetCenter);
      var distanceFromSpawn = GlobalPosition.DistanceTo(_spawn);
      if (200 > distanceToTarget && _speed > StartingSpeed && distanceFromSpawn > 200)
      {
        _speed = _speed - (_speed * delta) - 1;
      }
      else if (distanceFromSpawn > 40)
      {
        _speed = _speed + (_speed * delta) + 1;
      }
      var towardsTarget = (TargetCenter - Position).Normalized();
      var perpendicular = new Vector2(towardsTarget.y, -towardsTarget.x);
      Position += (TowardsStrength * towardsTarget + PerpendicularStrength * perpendicular * (float)Math.Sin(_time)) * _speed * delta;
      if (Math.Abs(TargetCenter.x - Position.x) < 20 && Math.Abs(TargetCenter.y - Position.y) < 20)
      {
        Despawn();
        return;
      }
    }
    else
    {
      Despawn();
      return;
    }
  }

  private void SpawnPosition()
  {
    var random = new Random();
    var radius = SelfBuff ? random.Next(128, 160) : random.Next(8, 16);
    var min = 0;
    var max = Math.PI * 2;
    var angle = random.NextDouble() * (max - min) + min;
    var offsetX = (float)Math.Sin(angle) * radius;
    var offsetY = (float)Math.Cos(angle) * radius;
    Position = new Vector2(Position.x + offsetX, Position.y + offsetY);
  }

  private void Despawn()
  {
    if (TargetCenter == null)
    {
      return;
    }

    if (!_isDespawning)
    {
      _isDespawning = true;
      Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 0);

      if (EffectEvent != null)
      {
        EffectEvent(TargetCard);
      }
    }
    QueueFree();
  }
}