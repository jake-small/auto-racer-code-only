using System;
using System.Collections.Generic;
using System.Linq;

public class PrepEngine
{
  public Bank Bank { get; private set; }
  public PlayerInventory PlayerInventory { get; set; }
  public ShopInventory ShopInventory { get; set; }
  public ShopService ShopService { get; set; }
  // TODO
  // private string history;

  private DataLoader _dataLoader;

  public PrepEngine()
  {
    _dataLoader = new FileLoader();
    Bank = new Bank(PrepSceneData.BankDataConfigRelativePath, _dataLoader);
    PlayerInventory = new PlayerInventory();
    ShopInventory = new ShopInventory();
    ShopService = new ShopService(_dataLoader);
  }

  public IEnumerable<PrepAbilityResult> CalculateStartTurnAbilities()
  {
    var startTurnAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Startturn)).ToList();
    startTurnAbilityCards.AddRange(ShopInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Startturn)));
    return CalculateAbilities(startTurnAbilityCards, Trigger.Startturn);
  }

  public IEnumerable<PrepAbilityResult> CalculateEndTurnAbilities()
  {
    var endTurnAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Endturn)).ToList();
    endTurnAbilityCards.AddRange(ShopInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Endturn)));
    return CalculateAbilities(endTurnAbilityCards, Trigger.Endturn);
  }

  public IEnumerable<PrepAbilityResult> CalculateOnSoldAbilities(Card card)
  {
    if (card.Abilities != null && card.Abilities.PrepAbilities != null
      && card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Sold))
    {
      return CalculateAbilities(new List<Card> { card }, Trigger.Sold, card);
    }
    return new List<PrepAbilityResult>();
  }

  public IEnumerable<PrepAbilityResult> CalculateOnBoughtAbilities(Card card)
  {
    if (card.Abilities != null && card.Abilities.PrepAbilities != null
      && card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Bought))
    {
      return CalculateAbilities(new List<Card> { card }, Trigger.Bought, card);
    }
    return new List<PrepAbilityResult>();
  }

  public IEnumerable<PrepAbilityResult> CalculateOnSellAbilities(Card soldCard)
  {
    var onSellAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Sell));
    return CalculateAbilities(onSellAbilityCards, Trigger.Sell, soldCard);
  }

  public IEnumerable<PrepAbilityResult> CalculateOnBuyAbilities(Card boughtCard)
  {
    var onBuyAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Buy));
    return CalculateAbilities(onBuyAbilityCards, Trigger.Buy, boughtCard);
  }

  public IEnumerable<PrepAbilityResult> CalculateOnRerollAbilities()
  {
    var onRerollAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Reroll));
    return CalculateAbilities(onRerollAbilityCards, Trigger.Reroll);
  }

  private IEnumerable<PrepAbilityResult> CalculateAbilities(IEnumerable<Card> cards, Trigger trigger, Card triggerCard = null)
  {
    var prepAbilityResponses = new List<PrepAbilityResult>();
    foreach (var card in cards)
    {
      var leveledCard = card.GetLeveledCard();
      var calculatedCard = leveledCard.ApplyPrepFunctionValues();
      var onTriggerAbilities = calculatedCard.Abilities.PrepAbilities
        .Where(a => a.GetTrigger() == trigger);

      foreach (var ability in onTriggerAbilities)
      {
        var response = ExecuteAbility(ability, card, triggerCard);
        if (response != null)
        {
          prepAbilityResponses.Add(response);
        }
      }
    }
    return prepAbilityResponses;
  }

  private PrepAbilityResult ExecuteAbility(PrepAbility ability, Card card, Card triggerCard = null)
  {
    var prepAbilityResult = new PrepAbilityResult();
    switch (ability.GetEffect())
    {
      case Effect.Basemove:
        prepAbilityResult = BaseMoveEffect(ability, card, triggerCard);
        break;
      case Effect.Exp:
        prepAbilityResult = ExperienceEffect(ability, card, triggerCard);
        break;
      case Effect.Gold:
        prepAbilityResult = GoldEffect(ability, card);
        break;
      case Effect.Reroll:
        prepAbilityResult = new PrepAbilityResult(card, Effect.Reroll);
        break;
    }
    return prepAbilityResult;
  }

  private PrepAbilityResult BaseMoveEffect(PrepAbility ability, Card card, Card triggerCard = null)
  {
    var targets = GetTargets(ability, card, triggerCard);
    return new PrepAbilityResult(card, Effect.Basemove, ability.Value.ToInt(), targets);
  }

  private PrepAbilityResult ExperienceEffect(PrepAbility ability, Card card, Card triggerCard = null)
  {
    var targets = GetTargets(ability, card, triggerCard);
    return new PrepAbilityResult(card, Effect.Exp, ability.Value.ToInt(), targets);
  }

  private PrepAbilityResult GoldEffect(PrepAbility ability, Card card)
  {
    if (ability.Target == null || ability.Target.GetInventoryType() == InventoryType.Any)
    {
      return new PrepAbilityResult(card, Effect.Gold, ability.Value.ToInt());
    }
    else if (ability.Target.GetInventoryType() == card.InventoryType)
    {
      return new PrepAbilityResult(card, Effect.Gold, ability.Value.ToInt());
    }
    return null;
  }

  private IEnumerable<Card> GetTargets(PrepAbility ability, Card card, Card triggerCard = null)
  {
    var target = ability.Target;
    if (target.GetTriggerCard())
    {
      return triggerCard != null ? new List<Card> { triggerCard } : new List<Card>();
    }
    if (target.GetTargetType() == TargetType.Self)
    {
      if (target.GetInventoryType() == InventoryType.Any || target.GetInventoryType() == card.InventoryType)
      {
        return new List<Card> { card };
      }
      return new List<Card>();
    }
    if (!string.IsNullOrWhiteSpace(target.Slot))
    {
      if (target.GetInventoryType() == InventoryType.Player)
      {
        var targetCard = PlayerInventory.GetCardInSlot(target.Slot.ToInt());
        return targetCard != null ? new List<Card> { targetCard } : new List<Card>();
      }
      else if (target.GetInventoryType() == InventoryType.Shop)
      {
        var targetCard = ShopInventory.GetCardInSlot(target.Slot.ToInt());
        return targetCard != null ? new List<Card> { targetCard } : new List<Card>();
      }
      else
      {
        return new List<Card>();
      }
    }
    if (target.Amount == null || target.Amount.ToInt() == 0)
    {
      return new List<Card>();
    }

    if (target.GetInventoryType() != InventoryType.Any && target.GetInventoryType() != card.InventoryType)
    {
      return new List<Card>();
    }

    var checkForward = false;
    var checkBack = false;
    switch (target.GetDirection())
    {
      case Direction.Forward:
        checkForward = true;
        break;
      case Direction.Backward:
        checkBack = true;
        break;
      case Direction.Any:
        checkForward = true;
        checkBack = true;
        break;
    }

    var targets = new List<Card>();
    var targetsDict = new Dictionary<int, Card>();
    var cardSlot = target.GetInventoryType() == InventoryType.Player ? PlayerInventory.GetSlotOfCard(card) : ShopInventory.GetSlotOfCard(card);
    if (target.GetInventoryType() == InventoryType.Any)
    {
      if (target.GetTargetType() == TargetType.All)
      {
        targets.AddRange(PlayerInventory.GetCardsAsList());
        targets.AddRange(ShopInventory.GetCardsAsList());
      }
      else if (target.GetTargetType() == TargetType.Others)
      {
        targets.AddRange(PlayerInventory.GetCardsAsList().Where(c => c != card));
        targets.AddRange(ShopInventory.GetCardsAsList().Where(c => c != card));
      }
      else
      {
        return new List<Card>();
      }
    }
    else
    {
      var availableTargets = target.GetInventoryType() == InventoryType.Player ? PlayerInventory.GetCards() : ShopInventory.GetCards();
      if (checkForward)
      {
        foreach (var kv in availableTargets)
        {
          if (kv.Key > cardSlot)
          {
            targetsDict.Add(kv.Key, kv.Value);
          }
        }
      }
      if (checkBack)
      {
        foreach (var kv in availableTargets)
        {
          if (kv.Key < cardSlot)
          {
            targetsDict.Add(kv.Key, kv.Value);
          }
        }
      }
      if (target.GetTargetType() == TargetType.All)
      {
        targetsDict.Add(cardSlot, card);
      }
    }

    if (!targetsDict.Any() && target.GetPriority() == Priority.Random)
    {
      targets = targets.Shuffle().ToList();
    }
    else if (targetsDict.Any())
    {
      switch (target.GetPriority())
      {
        case Priority.Random:
          targets.AddRange(targetsDict.Select(kv => kv.Value).ToList().Shuffle());
          break;
        case Priority.Closest:
          var closestCards = targetsDict.OrderBy(e => Math.Abs(e.Key - cardSlot)).Select(d => d.Value);
          targets.AddRange(closestCards);
          break;
        case Priority.Furthest:
          var furthestCards = targetsDict.OrderByDescending(e => Math.Abs(e.Key - cardSlot)).Select(d => d.Value);
          targets.AddRange(furthestCards);
          break;
        case Priority.PositionAscending:
          var ascendingCards = targetsDict.OrderBy(d => d.Key).Select(d => d.Value);
          targets.AddRange(ascendingCards);
          break;
        case Priority.PositionDescending:
          var descendingCards = targetsDict.OrderByDescending(d => d.Key).Where(c => c.Value != card).Select(d => d.Value);
          targets.AddRange(descendingCards);
          break;
      }
    }

    targets = targets.Where(t => !(t is CardEmpty)).ToList();
    if (ability.GetTrigger() == Trigger.Sell)
    {
      targets = targets.Where(c => c != triggerCard).ToList();
    }

    if (target.Amount.ToInt() > targets.Count)
    {
      return targets;
    }
    return targets.Take(target.Amount.ToInt());
  }
}