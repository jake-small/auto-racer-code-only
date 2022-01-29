using System.Collections.Generic;
using System.Linq;

public class BotBasic : Player
{
  private Bank _bank;
  private ShopService _shopService;
  private PlayerInventory _botInventory = new PlayerInventory();
  private ShopInventory _botShopInventory = new ShopInventory();

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
    _bank = new Bank(GameManager.PrepEngine.BankData);

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
      var availableCard = _botShopInventory.GetCardsAsList().FirstOrDefault();
      if (availableCard is null)
      {
        var rerollResult = Reroll();
        if (!rerollResult)
        {
          break;
        }
        availableCard = _botShopInventory.GetCardsAsList().FirstOrDefault();
      }

      var buyResult = _bank.Buy(availableCard);
      if (!buyResult.Success)
      {
        break;
      }
      var availableSlot = GetFirstOpenSlot();
      if (availableSlot == -1)
      {
        // no open slots
        // TODO combine cards
        break;
      }
      _botInventory.AddCard(availableCard, availableSlot);
      _botShopInventory.RemoveCard(availableSlot);
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