using KarizmaPlatform.Inventory.SharedClasses.Dtos;

namespace KarizmaPlatform.Inventory.Application.Processors.Interfaces;

public interface IInventoryProcessor<TEnum, TPrice>
{
    Task<List<InventoryItemDto>> GetAvailableInventoryItems(long userId);
    Task<bool> AddInventoryItemToUser(long userId, long inventoryItemId);
    Task<bool> EquipInventoryItem(long userId, long inventoryItemId);
    Task<bool> UnequipInventoryItemsByType(long userId, TEnum itemType);
    Task<bool> EquipInventoryItems(long userId, List<long> inventoryItemIds);
    Task<bool> DeleteInventoryItem(long userId, long inventoryItemId);
    Task<List<UserInventoryItemDto>> GetEquippedItems(long userId);
}

