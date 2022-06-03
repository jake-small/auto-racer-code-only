using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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

  public void CalculateStartTurnAbilities()
  {
    var startTurnAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Startturn));
    CalculateAbilities(startTurnAbilityCards, Trigger.Startturn);
  }

  public void CalculateEndTurnAbilities()
  {
    var endTurnAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Endturn));
    CalculateAbilities(endTurnAbilityCards, Trigger.Endturn);
  }

  public void CalculateOnSoldAbilities(Card card)
  {
    if (card.Abilities != null && card.Abilities.PrepAbilities != null
      && card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Sold))
    {
      CalculateAbilities(new List<Card> { card }, Trigger.Sold, card);
    }
  }

  public void CalculateOnBoughtAbilities(Card card)
  {
    if (card.Abilities != null && card.Abilities.PrepAbilities != null
      && card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Bought))
    {
      CalculateAbilities(new List<Card> { card }, Trigger.Bought, card);
    }
  }

  public void CalculateOnSellAbilities()
  {
    var onSellAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Sell));
    CalculateAbilities(onSellAbilityCards, Trigger.Sell);
  }

  public void CalculateOnBuyAbilities(Card boughtCard)
  {
    var onBuyAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Buy));
    CalculateAbilities(onBuyAbilityCards, Trigger.Buy, boughtCard);
  }

  public void CalculateOnRerollAbilities()
  {
    var onRerollAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Reroll));
    CalculateAbilities(onRerollAbilityCards, Trigger.Reroll);
  }

  private void CalculateAbilities(IEnumerable<Card> cards, Trigger trigger, Card triggerCard = null)
  {
    foreach (var card in cards)
    {
      var leveledCard = card.GetLeveledCard();
      var calculatedCard = leveledCard.ApplyPrepFunctionValues();
      var onTriggerAbilities = calculatedCard.Abilities.PrepAbilities
        .Where(a => a.GetTrigger() == trigger);

      foreach (var ability in onTriggerAbilities)
      {
        ExecuteAbility(ability, card, triggerCard);
      }
    }
  }

  private void ExecuteAbility(PrepAbility ability, Card card, Card triggerCard = null)
  {
    switch (ability.GetEffect())
    {
      case Effect.Basemove:
        BaseMoveEffect(ability, card, triggerCard);
        break;
      case Effect.Exp:
        ExperienceEffect(ability, card, triggerCard);
        break;
      case Effect.Gold:
        GoldEffect(ability);
        break;
    }
  }

  private void BaseMoveEffect(PrepAbility ability, Card card, Card triggerCard = null)
  {
    var targets = GetTargets(ability, card, triggerCard);
    foreach (var target in targets)
    {
      target.BaseMove += ability.Value.ToInt();
    }
  }

  private void ExperienceEffect(PrepAbility ability, Card card, Card triggerCard = null)
  {
    var targets = GetTargets(ability, card, triggerCard);
    foreach (var target in targets)
    {
      target.AddExp(ability.Value.ToInt());
    }
  }

  private void GoldEffect(PrepAbility ability)
  {
    Bank.AddCoins(ability.Value.ToInt());
  }

  private IEnumerable<Card> GetTargets(PrepAbility ability, Card card, Card triggerCard = null)
  {
    var target = ability.Target;
    if (target.GetTriggerCard() && triggerCard != null)
    {
      return new List<Card> { triggerCard };
    }

    var targets = new List<Card>();
    var targetInventoryType = target.GetInventoryType();
    var cardSlot = PlayerInventory.GetSlotOfCard(card);
    switch (target.GetTargetType())
    {
      case TargetType.Self:
        if (targetInventoryType == InventoryType.Any || targetInventoryType == card.InventoryType)
        {
          targets.Add(card);
        }
        break;
      case TargetType.Others:
        if (targetInventoryType == InventoryType.Any)
        {
          targets.AddRange(PlayerInventory.GetCardsAsList().Where(c => c != card));
          targets.AddRange(ShopInventory.GetCardsAsList().Where(c => c != card));
        }
        else if (targetInventoryType == InventoryType.Player)
        {
          var otherPlayerCardsDict = PlayerInventory.GetCards();
          if (card.InventoryType == targetInventoryType && target.GetPriority() == Priority.Closest)
          {
            var otherPlayerCards = otherPlayerCardsDict.OrderBy(e => Math.Abs(e.Key - cardSlot)).Where(c => c.Value != card).Select(d => d.Value);
            targets.AddRange(otherPlayerCards);
          }
          else if (card.InventoryType == targetInventoryType && target.GetPriority() == Priority.Furthest)
          {
            var otherPlayerCards = otherPlayerCardsDict.OrderByDescending(e => Math.Abs(e.Key - cardSlot)).Where(c => c.Value != card).Select(d => d.Value);
            targets.AddRange(otherPlayerCards);
          }
          else if (target.GetPriority() == Priority.PositionAscending)
          {
            var otherPlayerCards = otherPlayerCardsDict.OrderBy(d => d.Key).Where(c => c.Value != card).Select(d => d.Value);
            targets.AddRange(otherPlayerCards);
          }
          else if (target.GetPriority() == Priority.PositionDescending)
          {
            var otherPlayerCards = otherPlayerCardsDict.OrderByDescending(d => d.Key).Where(c => c.Value != card).Select(d => d.Value);
            targets.AddRange(otherPlayerCards);
          }
          else
          {
            var otherPlayerCards = PlayerInventory.GetCardsAsList().Where(c => c != card).ToList();
            targets.AddRange(otherPlayerCards);
          }
        }
        else if (target.GetInventoryType() == InventoryType.Shop)
        {
          var otherShopCardsDict = ShopInventory.GetCards();
          if (card.InventoryType == targetInventoryType && target.GetPriority() == Priority.Closest)
          {
            var otherShopCards = otherShopCardsDict.OrderBy(e => Math.Abs(e.Key - cardSlot)).Where(c => c.Value != card).Select(d => d.Value);
            targets.AddRange(otherShopCards);
          }
          else if (card.InventoryType == targetInventoryType && target.GetPriority() == Priority.Furthest)
          {
            var otherShopCards = otherShopCardsDict.OrderByDescending(e => Math.Abs(e.Key - cardSlot)).Where(c => c.Value != card).Select(d => d.Value);
            targets.AddRange(otherShopCards);
          }
          else if (target.GetPriority() == Priority.PositionAscending)
          {
            var otherShopCards = otherShopCardsDict.OrderBy(d => d.Key).Where(c => c.Value != card).Select(d => d.Value);
            targets.AddRange(otherShopCards);
          }
          else if (target.GetPriority() == Priority.PositionDescending)
          {
            var otherShopCards = otherShopCardsDict.OrderByDescending(d => d.Key).Where(c => c.Value != card).Select(d => d.Value);
            targets.AddRange(otherShopCards);
          }
          else
          {
            var otherShopCards = ShopInventory.GetCardsAsList().Where(c => c != card).ToList();
            targets.AddRange(otherShopCards);
          }
        }
        break;
      case TargetType.All:
        if (targetInventoryType == InventoryType.Any)
        {
          targets.AddRange(PlayerInventory.GetCardsAsList());
          targets.AddRange(ShopInventory.GetCardsAsList());
        }
        else if (targetInventoryType == InventoryType.Player)
        {
          var otherPlayerCardsDict = PlayerInventory.GetCards();
          if (card.InventoryType == targetInventoryType && target.GetPriority() == Priority.Closest)
          {
            var otherPlayerCards = otherPlayerCardsDict.OrderBy(e => Math.Abs(e.Key - cardSlot)).Select(d => d.Value);
            targets.AddRange(otherPlayerCards);
          }
          else if (card.InventoryType == targetInventoryType && target.GetPriority() == Priority.Furthest)
          {
            var otherPlayerCards = otherPlayerCardsDict.OrderByDescending(e => Math.Abs(e.Key - cardSlot)).Select(d => d.Value);
            targets.AddRange(otherPlayerCards);
          }
          else if (target.GetPriority() == Priority.PositionAscending)
          {
            var otherPlayerCards = otherPlayerCardsDict.OrderBy(d => d.Key).Select(d => d.Value);
            targets.AddRange(otherPlayerCards);
          }
          else if (target.GetPriority() == Priority.PositionDescending)
          {
            var otherPlayerCards = otherPlayerCardsDict.OrderByDescending(d => d.Key).Select(d => d.Value);
            targets.AddRange(otherPlayerCards);
          }
          else
          {
            var otherPlayerCards = PlayerInventory.GetCardsAsList().ToList();
            targets.AddRange(otherPlayerCards);
          }
        }
        else if (target.GetInventoryType() == InventoryType.Shop)
        {
          var otherShopCardsDict = ShopInventory.GetCards();
          if (card.InventoryType == targetInventoryType && target.GetPriority() == Priority.Closest)
          {
            var otherShopCards = otherShopCardsDict.OrderBy(e => Math.Abs(e.Key - cardSlot)).Select(d => d.Value);
            targets.AddRange(otherShopCards);
          }
          else if (card.InventoryType == targetInventoryType && target.GetPriority() == Priority.Furthest)
          {
            var otherShopCards = otherShopCardsDict.OrderByDescending(e => Math.Abs(e.Key - cardSlot)).Select(d => d.Value);
            targets.AddRange(otherShopCards);
          }
          else if (target.GetPriority() == Priority.PositionAscending)
          {
            var otherShopCards = otherShopCardsDict.OrderBy(d => d.Key).Select(d => d.Value);
            targets.AddRange(otherShopCards);
          }
          else if (target.GetPriority() == Priority.PositionDescending)
          {
            var otherShopCards = otherShopCardsDict.OrderByDescending(d => d.Key).Select(d => d.Value);
            targets.AddRange(otherShopCards);
          }
          else
          {
            var otherShopCards = ShopInventory.GetCardsAsList().ToList();
            targets.AddRange(otherShopCards);
          }
        }
        break;
    }

    if (target.GetPriority() == Priority.Random)
    {
      targets = targets.Shuffle().ToList();
    }

    if (target.Amount.ToInt() > targets.Count)
    {
      return targets;
    }
    return targets.Take(target.Amount.ToInt());
  }
}