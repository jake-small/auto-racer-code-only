using System.Collections.Generic;
using System.Linq;

public class BotBasic : Player
{
  private Bank _bank;
  private ShopService _shopService;
  private PlayerInventory _botInventory = new PlayerInventory(false);
  private ShopInventory _botShopInventory = new ShopInventory(false);

  public BotBasic(int id, int turn)
  {
    Id = id;
    Position = 0;
    Cards = GetBotInventory(turn);
  }

  /*
    Priority:
      1. Fill inventory
      2. Combine cards
      3. Reroll, repeat from 1
  */
  private Dictionary<int, Card> GetBotInventory(int turn)
  {
    var cards = GameManager.PrepEngine.ShopService.GetAvailableCards();
    _shopService = new ShopService(cards);
    _bank = new Bank(GameManager.PrepEngine.BankData, false);

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
    while (_bank.CoinTotal >= GameManager.PrepEngine.BankData.BuyCost)
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
        availableShopCard = _botShopInventory.GetCardsAsList().FirstOrDefault();
      }

      var availableSlot = GetFirstOpenSlot();
      if (availableSlot == -1)
      {
        // no open slots
        var combineResult = CombinePair();
        if (!combineResult)
        {
          // TODO try to combine straight from shop
          // search for a match to availableCard in botInventory
          //   if no match found, change availableShopCard to next card and try again

          break;
        }
        continue;
      }

      var buyResult = _bank.Buy(availableShopCard);
      if (!buyResult.Success)
      {
        break;
      }
      _botInventory.AddCard(availableShopCard, availableSlot);
      _botShopInventory.RemoveCard(availableShopCardSlot);
    }
  }

  private void FillShop()
  {
    var shopCards = _shopService.GetRandomCards(GameData.ShopInventorySize);
    for (int slot = 0; slot < GameData.ShopInventorySize; slot++)
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

  private bool CombinePair()
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
            return false;
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
    // if (targetCardScript.Card.Level >= droppedCardScript.Card.Level || fromShopInventory)
    // {
    //   targetCardScript.Card.AddExp(droppedCardScript.Card.Exp);
    //   targetCardScript.Card.CombineBaseMove(droppedCardScript.Card.BaseMove);
  }

  private (int, Card) GetFirstShopCard()
  {
    var cardDict = _botShopInventory.GetCards();
    if (!cardDict.Any())
    {
      return (-1, null);
    }
    var slottedCard = cardDict.First();
    return (slottedCard.Key, slottedCard.Value);
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