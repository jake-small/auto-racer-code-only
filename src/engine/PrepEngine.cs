using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

public class PrepEngine
{
  public Bank Bank { get; private set; }
  public PlayerInventory PlayerInventory { get; set; }
  public ShopInventory ShopInventory { get; set; }
  // TODO
  // private string history;
  private CalculationLayer _calcLayer = new CalculationLayer();

  public PrepEngine()
  {
    var bankData = LoadBankDataJson();
    GD.Print($"json bank data Starting Coins: {bankData.StartingCoins}");
    Bank = new Bank(bankData);
    PlayerInventory = new PlayerInventory();
    ShopInventory = new ShopInventory();
  }

  public void CalculateStartTurnAbilities()
  {
    var startTurnAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Card.Abilities != null && c.Card.Abilities.PrepAbilities != null &&
        c.Card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Startturn));
    CalculateAbilities(startTurnAbilityCards, Trigger.Startturn);
  }

  public void CalculateEndTurnAbilities()
  {
    var endTurnAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Card.Abilities != null && c.Card.Abilities.PrepAbilities != null &&
        c.Card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Endturn));
    CalculateAbilities(endTurnAbilityCards, Trigger.Endturn);
  }

  public void CalculateOnSoldAbilities(CardScript cardScript)
  {
    if (cardScript.Card.Abilities != null && cardScript.Card.Abilities.PrepAbilities != null
      && cardScript.Card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Sold))
    {
      CalculateAbilities(new List<CardScript> { cardScript }, Trigger.Sold);
    }
  }

  public void CalculateOnBoughtAbilities(CardScript cardScript)
  {
    if (cardScript.Card.Abilities != null && cardScript.Card.Abilities.PrepAbilities != null
      && cardScript.Card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Bought))
    {
      CalculateAbilities(new List<CardScript> { cardScript }, Trigger.Bought);
    }
  }

  public void CalculateOnSellAbilities()
  {
    var onSellAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Card.Abilities != null && c.Card.Abilities.PrepAbilities != null &&
        c.Card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Sell));
    CalculateAbilities(onSellAbilityCards, Trigger.Sell);
  }

  public void CalculateOnBuyAbilities()
  {
    var onBuyAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Card.Abilities != null && c.Card.Abilities.PrepAbilities != null &&
        c.Card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Buy));
    CalculateAbilities(onBuyAbilityCards, Trigger.Buy);
  }

  public void CalculateOnRerollAbilities()
  {
    var onRerollAbilityCards = PlayerInventory.GetCardsAsList()
      .Where(c => c.Card.Abilities != null && c.Card.Abilities.PrepAbilities != null &&
        c.Card.Abilities.PrepAbilities.Any(a => a.GetTrigger() == Trigger.Reroll));
    CalculateAbilities(onRerollAbilityCards, Trigger.Reroll);
  }

  private void CalculateAbilities(IEnumerable<CardScript> cardScripts, Trigger trigger)
  {
    foreach (var cardScript in cardScripts)
    {
      var leveledCard = _calcLayer.ApplyLevelValues(cardScript.Card);
      var calculatedCard = _calcLayer.ApplyPrepFunctionValues(leveledCard);
      var onTriggerAbilities = calculatedCard.Abilities.PrepAbilities
        .Where(a => a.GetTrigger() == trigger);

      foreach (var ability in onTriggerAbilities)
      {
        ExecuteAbility(ability, cardScript);
      }
    }
  }

  private void ExecuteAbility(PrepAbility ability, CardScript cardScript)
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

  private void BaseMoveEffect(PrepAbility ability, CardScript cardScript)
  {
    var targets = GetTargets(ability, cardScript);
    foreach (var target in targets)
    {
      target.Card.BaseMove += ability.Value.ToInt();
      target.UpdateUi();
    }
  }

  private void ExperienceEffect(PrepAbility ability, CardScript cardScript)
  {
    var targets = GetTargets(ability, cardScript);
    foreach (var target in targets)
    {
      target.Card.AddExp(ability.Value.ToInt());
      target.UpdateUi();
    }
  }

  private void GoldEffect(PrepAbility ability)
  {
    Bank.AddCoins(ability.Value.ToInt());
  }

  private IEnumerable<CardScript> GetTargets(PrepAbility ability, CardScript card)
  {
    var targets = new List<CardScript>();
    var target = ability.Target;
    switch (target.GetTargetType())
    {
      case TargetType.Self:
        if (target.GetInventoryType() == InventoryType.Any || target.GetInventoryType() == card.InventoryLocation)
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

  private BankData LoadBankDataJson()
  {
    var bankConfigFile = PrepSceneData.BankDataConfigRelativePath;
    if (!System.IO.File.Exists(bankConfigFile))
    {
      GD.Print($"Error: provided bankConfigFile '{bankConfigFile}' does not exist");
      throw new Exception($"Error: provided bankConfigFile '{bankConfigFile}' does not exist");
    }
    var bankDataConfigArr = System.IO.File.ReadAllLines(bankConfigFile);
    var bankDataConfig = String.Join("\n", bankDataConfigArr);
    GD.Print($"Bank Data:\n{bankDataConfig}");
    // TODO: error handling
    return JsonSerializer.Deserialize<BankData>(bankDataConfig);
  }
}