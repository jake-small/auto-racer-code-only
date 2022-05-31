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
      CalculateAbilities(new List<Card> { card }, Trigger.Sold);
    }
  }

  public void CalculateOnBoughtAbilities(Card card)
  {
    if (card.Abilities != null && card.Abilities.PrepAbilities != null
      && card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Bought))
    {
      CalculateAbilities(new List<Card> { card }, Trigger.Bought);
    }
  }

  public void CalculateOnSellAbilities()
  {
    var onSellAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Sell));
    CalculateAbilities(onSellAbilityCards, Trigger.Sell);
  }

  public void CalculateOnBuyAbilities()
  {
    var onBuyAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Buy));
    CalculateAbilities(onBuyAbilityCards, Trigger.Buy);
  }

  public void CalculateOnRerollAbilities()
  {
    var onRerollAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Abilities != null && c.Abilities.PrepAbilities != null &&
        c.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Reroll));
    CalculateAbilities(onRerollAbilityCards, Trigger.Reroll);
  }

  private void CalculateAbilities(IEnumerable<Card> cards, Trigger trigger)
  {
    foreach (var card in cards)
    {
      var leveledCard = card.GetLeveledCard();
      var calculatedCard = leveledCard.ApplyPrepFunctionValues();
      var onTriggerAbilities = calculatedCard.Abilities.PrepAbilities
        .Where(a => a.GetTrigger() == trigger);

      foreach (var ability in onTriggerAbilities)
      {
        ExecuteAbility(ability, card);
      }
    }
  }

  private void ExecuteAbility(PrepAbility ability, Card cardScript)
  {
    switch (ability.GetEffect())
    {
      case Effect.Basemove:
        BaseMoveEffect(ability, cardScript);
        break;
      case Effect.Exp:
        ExperienceEffect(ability, cardScript);
        break;
      case Effect.Gold:
        GoldEffect(ability);
        break;
    }
  }

  private void BaseMoveEffect(PrepAbility ability, Card card)
  {
    var targets = GetTargets(ability, card);
    foreach (var target in targets)
    {
      target.BaseMove += ability.Value.ToInt();
    }
  }

  private void ExperienceEffect(PrepAbility ability, Card card)
  {
    var targets = GetTargets(ability, card);
    foreach (var target in targets)
    {
      target.AddExp(ability.Value.ToInt());
    }
  }

  private void GoldEffect(PrepAbility ability)
  {
    Bank.AddCoins(ability.Value.ToInt());
  }

  private IEnumerable<Card> GetTargets(PrepAbility ability, Card card)
  {
    var targets = new List<Card>();
    var target = ability.Target;
    switch (target.GetTargetType())
    {
      case TargetType.Self:
        if (target.GetInventoryType() == InventoryType.Any || target.GetInventoryType() == card.InventoryType)
        {
          targets.Add(card);
        }
        break;
      case TargetType.Others:
        if (target.GetInventoryType() == InventoryType.Any)
        {
          targets.AddRange(PlayerInventory.GetCardsAsList().Where(c => c != card));
          targets.AddRange(ShopInventory.GetCardsAsList().Where(c => c != card));
        }
        else if (target.GetInventoryType() == InventoryType.Player)
        {
          targets.AddRange(PlayerInventory.GetCardsAsList().Where(c => c != card));
        }
        else if (target.GetInventoryType() == InventoryType.Shop)
        {
          targets.AddRange(ShopInventory.GetCardsAsList().Where(c => c != card));
        }
        break;
      case TargetType.All:
        if (target.GetInventoryType() == InventoryType.Any)
        {
          targets.AddRange(PlayerInventory.GetCardsAsList());
          targets.AddRange(ShopInventory.GetCardsAsList());
        }
        else if (target.GetInventoryType() == InventoryType.Player)
        {
          targets.AddRange(PlayerInventory.GetCardsAsList());
        }
        else if (target.GetInventoryType() == InventoryType.Shop)
        {
          targets.AddRange(ShopInventory.GetCardsAsList());
        }
        break;
    }
    return targets;
  }
}