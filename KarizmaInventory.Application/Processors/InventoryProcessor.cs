using KarizmaPlatform.Inventory.Application.Processors.Interfaces;
using KarizmaPlatform.Inventory.Domain.Models;
using KarizmaPlatform.Inventory.Domain.Utilities;
using KarizmaPlatform.Inventory.Infrastructure.Repositories.Interfaces;
using KarizmaPlatform.Inventory.SharedClasses.Dtos;

namespace KarizmaPlatform.Inventory.Application.Processors;

public class InventoryProcessor<TEnum, TPrice, TMetadata>(
    IUserInventoryItemRepository userInventoryItemRepository,
    IInventoryItemRepository inventoryItemRepository) : IInventoryProcessor<TEnum, TPrice, TMetadata>
    where TEnum : struct, Enum
{
    public async Task<List<InventoryItemDto<TEnum, TPrice, TMetadata>>> GetAvailableInventoryItems(long? userId)
    {
        var allItems = await inventoryItemRepository.GetAll();
        if (userId == null)
        {
            // Free items are always owned, others are not
            return allItems
                .Select(item => MapToDto(item, isOwned: IsFree(item), isEquipped: false))
                .OrderBy(item => item.DisplayOrder)
                .ToList();
        }

        // When userId is provided, include user-specific data
        var userItems = await userInventoryItemRepository.FindUserInventoryItems(userId.Value, false);
        var userItemIds = userItems.Select(ui => ui.InventoryItemId).ToHashSet();
        var equippedItemIds = userItems.Where(ui => ui.IsEquipped).Select(ui => ui.InventoryItemId).ToHashSet();

        return allItems
            .Select(item => MapToDto(
                item,
                isOwned: IsFree(item) || userItemIds.Contains(item.Id), // Free items are always owned
                isEquipped: equippedItemIds.Contains(item.Id)))
            .OrderBy(item => item.DisplayOrder)
            .ToList();
    }

    public async Task<List<InventoryItemDto<TEnum, TPrice, TMetadata>>> GetAvailableInventoryItemsByType(long? userId,
        TEnum itemType)
    {
        var allItems = await GetAvailableInventoryItems(userId);
        return allItems.Where(item => item.Type.Equals(itemType)).ToList();
    }

    private static bool IsFree(InventoryItem item)
    {
        return JsonUtilities.IsNullOrEmptyJson(item.Price);
    }

    private static InventoryItemDto<TEnum, TPrice, TMetadata> MapToDto(InventoryItem item, bool isOwned,
        bool isEquipped)
    {
        var isFree = IsFree(item);

        return new InventoryItemDto<TEnum, TPrice, TMetadata>
        {
            Id = item.Id,
            Name = item.Name,
            AssetKey = item.AssetKey,
            Type = Enum.Parse<TEnum>(item.Type),
            Price = isFree ? default! : JsonUtilities.Deserialize<TPrice>(item.Price) ?? default!,
            Metadata = JsonUtilities.Deserialize<TMetadata>(item.Metadata) ?? default!,
            IsFree = isFree,
            DisplayOrder = item.DisplayOrder,
            CanBePurchased = item.CanBePurchased,
            MinLevel = item.MinLevel,
            IsOwned = isOwned,
            IsEquipped = isEquipped
        };
    }

    public async Task<bool> AddInventoryItemToUser(long userId, long inventoryItemId)
    {
        try
        {
            var inventoryItem = await inventoryItemRepository.FindById(inventoryItemId);
            if (inventoryItem == null)
                return false; // Item doesn't exist

            // Check if user already has this item
            var existingItem = await userInventoryItemRepository.FindUserInventoryItem(userId, inventoryItemId);
            if (existingItem != null)
                return false; // Already owns the item

            await userInventoryItemRepository.Add(new UserInventoryItem
            {
                UserId = userId,
                InventoryItemId = inventoryItemId,
                IsEquipped = false
            });

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"AddInventoryItemToUser Error: {e.Message}");
            return false;
        }
    }

    public async Task<bool> EquipInventoryItem(long userId, long inventoryItemId)
    {
        try
        {
            var inventoryItem = await inventoryItemRepository.FindById(inventoryItemId);
            if (inventoryItem == null)
                return false; // Item doesn't exist

            var itemType = inventoryItem.Type;

            // Check if item is free (price is null)
            var isFreeItem = IsFree(inventoryItem);

            UserInventoryItem? userInventoryItem;

            if (isFreeItem)
            {
                // For free items, user can equip without owning
                userInventoryItem = await userInventoryItemRepository.FindUserInventoryItem(userId, inventoryItemId);
                if (userInventoryItem == null)
                {
                    // Auto-add the free item
                    userInventoryItem = await userInventoryItemRepository.Add(new UserInventoryItem
                    {
                        UserId = userId,
                        InventoryItemId = inventoryItemId,
                        IsEquipped = false
                    });
                }
            }
            else
            {
                // For paid items, check ownership
                userInventoryItem = await userInventoryItemRepository.FindUserInventoryItem(userId, inventoryItemId);
                if (userInventoryItem == null)
                    return false; // User doesn't own this item
            }

            // Unequip all items of the same type
            await userInventoryItemRepository.UnequipItemsByType(userId, itemType);

            // Equip the selected item
            userInventoryItem.IsEquipped = true;
            await userInventoryItemRepository.Update(userInventoryItem);

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"EquipInventoryItem Error: {e.Message}");
            return false;
        }
    }

    public async Task<bool> UnequipInventoryItemsByType(long userId, TEnum itemType)
    {
        try
        {
            var itemTypeString = itemType.ToString();
            await userInventoryItemRepository.UnequipItemsByType(userId, itemTypeString);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"UnequipInventoryItemsByType Error: {e.Message}");
            return false;
        }
    }

    public async Task<bool> EquipInventoryItems(long userId, List<long> inventoryItemIds)
    {
        try
        {
            foreach (var inventoryItemId in inventoryItemIds)
            {
                var result = await EquipInventoryItem(userId, inventoryItemId);
                if (!result)
                    return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"EquipInventoryItems Error: {e.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteInventoryItem(long userId, long inventoryItemId)
    {
        try
        {
            var userInventoryItem = await userInventoryItemRepository.FindUserInventoryItem(userId, inventoryItemId);

            if (userInventoryItem == null)
                return false;

            await userInventoryItemRepository.DeleteById(userInventoryItem.Id);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"DeleteInventoryItem Error: {e.Message}");
            return false;
        }
    }

    public async Task<List<UserInventoryItemDto<TEnum, TPrice, TMetadata>>> GetEquippedItems(long userId)
    {
        var equippedItems = await userInventoryItemRepository.FindEquippedItems(userId, false);

        // Group by type and take only the first equipped item per type (in case of manual DB edits)
        var uniqueEquippedItems = equippedItems
            .Where(ui => ui.InventoryItem != null)
            .GroupBy(ui => ui.InventoryItem!.Type)
            .Select(g => g.First())
            .ToList();

        return uniqueEquippedItems.Select(ui => new UserInventoryItemDto<TEnum, TPrice, TMetadata>
            {
                Id = ui.Id,
                UserId = ui.UserId,
                InventoryItemId = ui.InventoryItemId,
                IsEquipped = ui.IsEquipped,
                InventoryItem = MapToDto(ui.InventoryItem!, isOwned: true, isEquipped: ui.IsEquipped)
            })
            .OrderBy(ui => ui.InventoryItem!.DisplayOrder)
            .ToList();
    }

    public async Task<Dictionary<TEnum, UserInventoryItemDto<TEnum, TPrice, TMetadata>>> GetEquippedItemsDictionary(
        long userId)
    {
        var equippedItems = await GetEquippedItems(userId);

        return equippedItems
            .Where(ui => ui.InventoryItem != null)
            .ToDictionary(
                ui => ui.InventoryItem!.Type,
                ui => ui
            );
    }
}
