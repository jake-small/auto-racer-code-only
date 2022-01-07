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


  // Events: start turn, end turn, sell, sold, buy, bought, reroll, and freeze
  public void CalculatePrepAbilities(string phase, IEnumerable<Card> inventoryCards, IEnumerable<Card> shopCards, int gold)
  {
  }

  public void CalculateOnSellAbilities()
  {
    var onSellAbilityCards = PlayerInventory.GetCardsAsList()
    .Where(c => c.Card.Abilities != null && c.Card.Abilities.PrepAbilities != null &&
    c.Card.Abilities.PrepAbilities.Any(a => a.Trigger.Equals("Sell", StringComparison.InvariantCultureIgnoreCase)));

    foreach (var cardScript in onSellAbilityCards)
    {
      var leveledCard = _calcLayer.ApplyLevelValues(cardScript.Card);
      var calculatedCard = _calcLayer.ApplyPrepFunctionValues(leveledCard);
      var onSellAbilities = calculatedCard.Abilities.PrepAbilities
      .Where(a => a.Trigger.Equals("Sell", StringComparison.InvariantCultureIgnoreCase));

      foreach (var ability in onSellAbilities)
      {
        ExecuteAbility(ability, cardScript.Slot);
      }
    }
  }

  public void ExecuteAbility(PrepAbility ability, int abilitySlot)
  {
    switch (ability.Effect)
    {
      case "BaseMove":
        BaseMoveEffect(ability, abilitySlot);
        break;
      case "Gold":
        GoldEffect(ability, abilitySlot);
        break;
      case "Exp":
        ExperienceEffect(ability, abilitySlot);
        break;
    }
  }

  private void BaseMoveEffect(PrepAbility ability, int abilitySlot)
  {
    CardScript affectedCard;
    if (ability.Target.Equals("Self", StringComparison.InvariantCultureIgnoreCase))
    {
      affectedCard = PlayerInventory.GetCardInSlot(abilitySlot);
    }
    else if (ability.Target.Equals("ShopCards", StringComparison.InvariantCultureIgnoreCase))
    {
      // TODO
      throw new NotImplementedException();
    }
    else
    {
      var slotTarget = ability.Target.ToInt();
      affectedCard = PlayerInventory.GetCardInSlot(slotTarget);
    }
    if (affectedCard == null)
    {
      // TODO
    }
    affectedCard.Card.BaseMove += ability.Value.ToInt();
  }

  private void ExperienceEffect(PrepAbility ability, int abilitySlot)
  {
    CardScript affectedCard;
    if (ability.Target.Equals("Self", StringComparison.InvariantCulture))
    {
      affectedCard = PlayerInventory.GetCardInSlot(abilitySlot);
    }
    else if (ability.Target.Equals("ShopCards", StringComparison.InvariantCultureIgnoreCase))
    {
      // TODO
      throw new NotImplementedException();
    }
    else
    {
      var slotTarget = ability.Target.ToInt();
      affectedCard = PlayerInventory.GetCardInSlot(slotTarget);
    }
    if (affectedCard == null)
    {
      // TODO
    }
    affectedCard.Card.AddExp(ability.Value.ToInt());
  }

  private void GoldEffect(PrepAbility ability, int abilitySlot)
  {
    Bank.AddCoins(ability.Value.ToInt());
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