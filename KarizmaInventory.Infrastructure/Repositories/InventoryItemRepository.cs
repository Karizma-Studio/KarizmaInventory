using KarizmaPlatform.Inventory.Domain.Models;
using KarizmaPlatform.Inventory.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KarizmaPlatform.Inventory.Infrastructure.Repositories;

public class InventoryItemRepository(IInventoryDatabase inventoryDatabase) : IInventoryItemRepository
{
    public async Task<InventoryItem> Add(InventoryItem entity)
    {
        inventoryDatabase.GetInventoryItems().Add(entity);
        await inventoryDatabase.SaveChangesAsync();
        return entity;
    }

    public Task Update(InventoryItem entity)
    {
        inventoryDatabase.GetInventoryItems().Update(entity);
        return inventoryDatabase.SaveChangesAsync();
    }

    public async Task DeleteById(long identifier)
    {
        var byId = await FindById(identifier);
        if (byId is null)
            return;
        inventoryDatabase.GetInventoryItems().Remove(byId);
        await inventoryDatabase.SaveChangesAsync();
    }

    public Task<InventoryItem?> FindById(long identifier)
    {
        return inventoryDatabase.GetInventoryItems().SingleOrDefaultAsync(x => x.Id == identifier);
    }

    public Task<List<InventoryItem>> GetAll()
    {
        return inventoryDatabase.GetInventoryItems().ToListAsync();
    }
}

