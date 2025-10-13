using KarizmaPlatform.Core.Logic;
using KarizmaPlatform.Inventory.Domain.Models;

namespace KarizmaPlatform.Inventory.Infrastructure.Repositories.Interfaces;

public interface IUserInventoryItemRepository : IRepository<UserInventoryItem>
{
    Task<List<UserInventoryItem>> FindUserInventoryItems(long userId, bool tracking = true);
    Task<UserInventoryItem?> FindUserInventoryItem(long userId, long inventoryItemId, bool tracking = true);
    Task<List<UserInventoryItem>> FindEquippedItems(long userId, bool tracking = true);
    Task<int> UnequipItemsByType(long userId, string itemType);
}

