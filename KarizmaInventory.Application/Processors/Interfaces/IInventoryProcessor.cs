using KarizmaPlatform.Inventory.SharedClasses.Dtos;

namespace KarizmaPlatform.Inventory.Application.Processors.Interfaces;

public interface IInventoryProcessor<TEnum, TPrice> where TEnum : struct, Enum
{
    Task<List<InventoryItemDto<TEnum, TPrice>>> GetAvailableInventoryItems(long? userId);
    Task<List<InventoryItemDto<TEnum, TPrice>>> GetAvailableInventoryItemsByType(long? userId, TEnum itemType);
    Task<bool> AddInventoryItemToUser(long userId, long inventoryItemId);
    Task<bool> EquipInventoryItem(long userId, long inventoryItemId);
    Task<bool> UnequipInventoryItemsByType(long userId, TEnum itemType);
    Task<bool> EquipInventoryItems(long userId, List<long> inventoryItemIds);
    Task<bool> DeleteInventoryItem(long userId, long inventoryItemId);
    Task<List<UserInventoryItemDto<TEnum, TPrice>>> GetEquippedItems(long userId);
    Task<Dictionary<TEnum, UserInventoryItemDto<TEnum, TPrice>>> GetEquippedItemsDictionary(long userId);
}

