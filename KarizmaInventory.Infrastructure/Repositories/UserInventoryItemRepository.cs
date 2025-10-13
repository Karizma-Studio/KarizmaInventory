using KarizmaPlatform.Inventory.Domain.Models;
using KarizmaPlatform.Inventory.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KarizmaPlatform.Inventory.Infrastructure.Repositories;

public class UserInventoryItemRepository(IInventoryDatabase inventoryDatabase) : IUserInventoryItemRepository
{
    public async Task<UserInventoryItem> Add(UserInventoryItem entity)
    {
        inventoryDatabase.GetUserInventoryItems().Add(entity);
        await inventoryDatabase.SaveChangesAsync();
        return entity;
    }

    public Task Update(UserInventoryItem entity)
    {
        inventoryDatabase.GetUserInventoryItems().Update(entity);
        return inventoryDatabase.SaveChangesAsync();
    }

    public async Task DeleteById(long identifier)
    {
        var byId = await FindById(identifier);
        if (byId is null)
            return;
        inventoryDatabase.GetUserInventoryItems().Remove(byId);
        await inventoryDatabase.SaveChangesAsync();
    }

    public Task<UserInventoryItem?> FindById(long identifier)
    {
        return inventoryDatabase.GetUserInventoryItems().SingleOrDefaultAsync(x => x.Id == identifier);
    }

    public Task<List<UserInventoryItem>> GetAll()
    {
        return inventoryDatabase.GetUserInventoryItems().ToListAsync();
    }


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

    public Task<int> UnequipItemsByType(long userId, string itemType)
    {
        return inventoryDatabase.GetUserInventoryItems()
            .Where(ui => ui.UserId == userId && ui.IsEquipped && ui.InventoryItem != null && ui.InventoryItem.Type == itemType)
            .ExecuteUpdateAsync(setters => setters.SetProperty(ui => ui.IsEquipped, false));
    }
}


