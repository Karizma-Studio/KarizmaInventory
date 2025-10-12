using KarizmaPlatform.Inventory.Application.Processors.Interfaces;
using KarizmaPlatform.Inventory.Domain.Models;
using KarizmaPlatform.Inventory.Infrastructure.Repositories.Interfaces;
using KarizmaPlatform.Inventory.SharedClasses.Dtos;

namespace KarizmaPlatform.Inventory.Application.Processors;

public class InventoryProcessor<TEnum, TPrice>(
    IUserInventoryItemRepository userInventoryItemRepository,
    IInventoryItemRepository inventoryItemRepository) : IInventoryProcessor<TEnum, TPrice> where TEnum : struct, Enum
{
    public async Task<List<InventoryItemDto>> GetAvailableInventoryItems(long userId)
    {
        var allItems = await inventoryItemRepository.GetAll();
        var userItems = await userInventoryItemRepository.FindUserInventoryItems(userId, false);
        var userItemIds = userItems.Select(ui => ui.InventoryItemId).ToHashSet();
        var equippedItemIds = userItems.Where(ui => ui.IsEquipped).Select(ui => ui.InventoryItemId).ToHashSet();

        return allItems.Select(item => new InventoryItemDto
        {
            Id = item.Id,
            AssetKey = item.AssetKey,
            Type = item.Type,
            Price = item.Price,
            DisplayOrder = item.DisplayOrder,
            CanBePurchased = item.CanBePurchased,
            IsOwned = userItemIds.Contains(item.Id),
            IsEquipped = equippedItemIds.Contains(item.Id)
        }).ToList();
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
            var isFreeItem = string.IsNullOrEmpty(inventoryItem.Price) || inventoryItem.Price == "null";

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
            var userItems = await userInventoryItemRepository.FindUserInventoryItems(userId);
            var itemsOfSameType = userItems.Where(ui => 
                ui.InventoryItem != null && 
                ui.InventoryItem.Type == itemType && 
                ui.IsEquipped).ToList();

            foreach (var item in itemsOfSameType)
            {
                item.IsEquipped = false;
                await userInventoryItemRepository.Update(item);
            }

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
            var userItems = await userInventoryItemRepository.FindUserInventoryItems(userId);
            var itemsOfType = userItems.Where(ui => 
                ui.InventoryItem != null && 
                ui.InventoryItem.Type == itemTypeString && 
                ui.IsEquipped).ToList();

            foreach (var item in itemsOfType)
            {
                item.IsEquipped = false;
                await userInventoryItemRepository.Update(item);
            }

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

    public async Task<List<UserInventoryItemDto>> GetEquippedItems(long userId)
    {
        var equippedItems = await userInventoryItemRepository.FindEquippedItems(userId, false);
        
        return equippedItems.Select(ui => new UserInventoryItemDto
        {
            Id = ui.Id,
            UserId = ui.UserId,
            InventoryItemId = ui.InventoryItemId,
            IsEquipped = ui.IsEquipped,
            InventoryItem = ui.InventoryItem == null ? null : new InventoryItemDto
            {
                Id = ui.InventoryItem.Id,
                AssetKey = ui.InventoryItem.AssetKey,
                Type = ui.InventoryItem.Type,
                Price = ui.InventoryItem.Price,
                DisplayOrder = ui.InventoryItem.DisplayOrder,
                CanBePurchased = ui.InventoryItem.CanBePurchased,
                IsOwned = true,
                IsEquipped = ui.IsEquipped
            }
        }).ToList();
    }
}


