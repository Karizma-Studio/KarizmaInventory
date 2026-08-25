using KarizmaPlatform.Inventory.SharedClasses.Dtos;

namespace KarizmaPlatform.Inventory.Application.Processors.Interfaces;

public interface IInventoryProcessor<TEnum, TPrice, TMetadata> where TEnum : struct, Enum
{
    Task<List<InventoryItemDto<TEnum, TPrice, TMetadata>>> GetAvailableInventoryItems(long? userId);
    Task<List<InventoryItemDto<TEnum, TPrice, TMetadata>>> GetAvailableInventoryItemsByType(long? userId, TEnum itemType);
    Task<bool> AddInventoryItemToUser(long userId, long inventoryItemId);
    Task<bool> EquipInventoryItem(long userId, long inventoryItemId);
    Task<bool> UnequipInventoryItemsByType(long userId, TEnum itemType);
    Task<bool> EquipInventoryItems(long userId, List<long> inventoryItemIds);
    Task<bool> DeleteInventoryItem(long userId, long inventoryItemId);
    Task<List<UserInventoryItemDto<TEnum, TPrice, TMetadata>>> GetEquippedItems(long userId);
    Task<Dictionary<TEnum, UserInventoryItemDto<TEnum, TPrice, TMetadata>>> GetEquippedItemsDictionary(long userId);
}
