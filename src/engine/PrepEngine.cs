using System;
using System.Collections.Generic;
using System.Text.Json;
using Godot;

public class PrepEngine
{
  public Bank Bank { get; private set; }
  public PlayerInventory PlayerInventory { get; set; }
  public ShopInventory ShopInventory { get; set; }
  // TODO
  // private string history;

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