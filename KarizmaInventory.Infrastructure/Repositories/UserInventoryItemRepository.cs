using KarizmaPlatform.Core.Database;
using KarizmaPlatform.Core.Logic;
using KarizmaPlatform.Inventory.Domain.Models;
using KarizmaPlatform.Inventory.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KarizmaPlatform.Inventory.Infrastructure.Repositories;

public class UserInventoryItemRepository(IInventoryDatabase inventoryDatabase, BaseContext baseContext) 
    : BaseRepository<UserInventoryItem>(baseContext), IUserInventoryItemRepository
{
    public Task<List<UserInventoryItem>> FindUserInventoryItems(long userId, bool tracking = true)
    {
        return (tracking
                ? inventoryDatabase.GetUserInventoryItems()
                : inventoryDatabase.GetUserInventoryItems().AsNoTracking())
            .Include(ui => ui.InventoryItem)
            .Where(ui => ui.UserId == userId)
            .ToListAsync();
    }

    public Task<UserInventoryItem?> FindUserInventoryItem(long userId, long inventoryItemId, bool tracking = true)
    {
        return (tracking
                ? inventoryDatabase.GetUserInventoryItems()
                : inventoryDatabase.GetUserInventoryItems().AsNoTracking())
            .Include(ui => ui.InventoryItem)
            .SingleOrDefaultAsync(ui => ui.UserId == userId && ui.InventoryItemId == inventoryItemId);
    }

    public Task<List<UserInventoryItem>> FindEquippedItems(long userId, bool tracking = true)
    {
        return (tracking
                ? inventoryDatabase.GetUserInventoryItems()
                : inventoryDatabase.GetUserInventoryItems().AsNoTracking())
            .Include(ui => ui.InventoryItem)
            .Where(ui => ui.UserId == userId && ui.IsEquipped)
            .ToListAsync();
    }
}


