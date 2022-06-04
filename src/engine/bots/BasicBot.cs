using System;
using System.Collections.Generic;
using System.Linq;

public class BotBasic : Player
{
  private Bank _bank;
  private ShopService _shopService;
  private PlayerInventory _botInventory = new PlayerInventory(false);
  private ShopInventory _botShopInventory = new ShopInventory(false);

  public BotBasic(int id, int turn, ShopService shopService, string name = null)
  {
    _shopService = shopService;
    Id = id;
    Name = name ?? $"Player {id}";
    Position = 0;
    Cards = GetBotInventory(turn);
    Skin = GetRandomSkin();
  }

  private string GetRandomSkin()
  {
    var random = new Random();
    var index = random.Next(GameManager.CharacterSkins.Count);
    return GameManager.CharacterSkins[index];
  }

  /*
    Priority:
      1. Fill inventory
      2. Combine cards
      3. Reroll, repeat from 1
  */
  private Dictionary<int, Card> GetBotInventory(int turn)
  {
    _bank = new Bank(GameManager.PrepEngine.Bank.BankData, false);

    for (int i = 0; i < turn; i++)
    {
      _bank.SetStartingCoins();
      FillShop();
      BuyCards();
    }
    return _botInventory.GetCards();
  }

  private void BuyCards()
  {
    while (_bank.CoinTotal >= GameManager.PrepEngine.Bank.BuyCost)
    {
      // Buy a card
      var (availableShopCardSlot, availableShopCard) = GetFirstShopCard();
      if (availableShopCard is null)
      {
        var rerollResult = Reroll();
        if (!rerollResult)
        {
          break;
        }
        continue;
      }

      var availableSlot = GetFirstOpenSlot();
      if (availableSlot == -1)
      {
        // no open slots
        var combineResult = CombinePairFromInventory();
        if (!combineResult)
        {
          var combineFromShopResult = BuyAndCombineFromShop();
          if (!combineFromShopResult)
          {
            break;
          }
          continue;
        }
        else
        {
          continue;
        }
      }

      var buyResult = BuyCard(availableSlot, availableShopCard, availableShopCardSlot);
      if (!buyResult)
      {
        break;
      }
    }
  }

  private void FillShop()
  {
    var shopCards = _shopService.GetRandomCards(GameManager.ShopSize, GameManager.CurrentRace - 1);
    for (int slot = 0; slot < GameManager.ShopSize; slot++)
    {
      var card = shopCards[slot];
      _botShopInventory.AddCard(card, slot);
    }
  }

  private bool Reroll()
  {
    var rerollResult = _bank.Reroll();
    if (!rerollResult.Success)
    {
      return false;
    }
    _botShopInventory.Clear();
    FillShop();
    return true;
  }

  private bool BuyCard(int playerSlot, Card shopCard, int shopCardSlot)
  {
    var buyResult = _bank.Buy(shopCard);
    if (!buyResult.Success)
    {
      return false;
    }
    _botInventory.AddCard(shopCard, playerSlot);
    _botShopInventory.RemoveCard(shopCardSlot);
    return true;
  }

  private bool CombinePairFromInventory()
  {
    var cardDict = _botInventory.GetCards();
    foreach (var (slotA, cardA) in cardDict.Select(d => (d.Key, d.Value)))
    {
      foreach (var (slotB, cardB) in cardDict.Select(d => (d.Key, d.Value)))
      {
        if (slotA == slotB)
        {
          continue;
        }

        if (cardA.GetRawName() == cardB.GetRawName())
        {
          // TODO: this logic is partially duplicated here and in PrepMain.cs
          if (cardA.IsMaxLevel() || cardB.IsMaxLevel())
          {
            // return false;
            continue;
          }

          if (cardA.Level >= cardB.Level)
          {
            cardA.AddExp(cardB.Exp);
            cardA.CombineBaseMove(cardB.BaseMove);
            _botInventory.RemoveCard(slotB);
          }
          else
          {
            cardB.AddExp(cardA.Exp);
            cardB.CombineBaseMove(cardA.BaseMove);
            _botInventory.RemoveCard(slotA);
            _botInventory.MoveCard(cardB, slotB, slotA);
          }
          return true;
        }
      }
    }
    return false;
  }

  private bool BuyAndCombineFromShop()
  {
    var shopCardDict = _botShopInventory.GetCards();
    var botCardDict = _botInventory.GetCards();
    foreach (var (shopSlot, shopCard) in shopCardDict.Select(d => (d.Key, d.Value)))
    {
      foreach (var (botSlot, botCard) in botCardDict.Select(d => (d.Key, d.Value)))
      {
        if (shopCard.GetRawName() == botCard.GetRawName())
        {
          if (botCard.IsMaxLevel())
          {
            continue;
          }
          var buyResult = BuyCard(botSlot, shopCard, shopSlot);
          if (!buyResult)
          {
            continue;
          }
          botCard.AddExp(shopCard.Exp);
          botCard.CombineBaseMove(shopCard.BaseMove);
          _botShopInventory.RemoveCard(botSlot);
          return true;
        }
      }
    }
    return false;
  }

  private (int, Card) GetFirstShopCard()
  {
    var cardDict = _botShopInventory.GetCards();
    if (!cardDict.Any())
    {
      return (-1, null);
    }
    // Don't buy cards with effects a bot won't make use of (sell, sold, freeze)
    var slottedCardNoSellAbility = cardDict
      .Where(kv => !kv.Value.Abilities.PrepAbilities
        .Any(a => a.GetTrigger() == Trigger.Sell || a.GetTrigger() == Trigger.Sold || a.GetTrigger() == Trigger.Freeze
          || a.Functions.Any(f => f.Body.Contains(".IsFrozen"))))
      .FirstOrDefault();
    if (slottedCardNoSellAbility.Value == null)
    {
      return (-1, null);
    }
    return (slottedCardNoSellAbility.Key, slottedCardNoSellAbility.Value);
  }

  private int GetFirstOpenSlot()
  {
    var cardDict = _botInventory.GetCards();
    foreach (var (slot, card) in cardDict.Select(d => (d.Key, d.Value)))
    {
      if (card is CardEmpty)
      {
        return slot;
      }
    }
    return -1;
  }
}