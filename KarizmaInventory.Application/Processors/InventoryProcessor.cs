using System.Text.Json;
using KarizmaPlatform.Inventory.Application.Processors.Interfaces;
using KarizmaPlatform.Inventory.Domain.Models;
using KarizmaPlatform.Inventory.Infrastructure.Repositories.Interfaces;
using KarizmaPlatform.Inventory.SharedClasses.Dtos;

namespace KarizmaPlatform.Inventory.Application.Processors;

public class InventoryProcessor<TEnum, TPrice>(
    IUserInventoryItemRepository userInventoryItemRepository,
    IInventoryItemRepository inventoryItemRepository) : IInventoryProcessor<TEnum, TPrice> where TEnum : struct, Enum
{
    public async Task<List<InventoryItemDto<TEnum, TPrice>>> GetAvailableInventoryItems(long? userId)
    {
        var allItems = await inventoryItemRepository.GetAll();
        if (userId == null)
        {
            return allItems.Select(item =>
            {
                var isFree = string.IsNullOrEmpty(item.Price) || item.Price == "null";
                var price = isFree ? default(TPrice)! : (DeserializePrice(item.Price) ?? default(TPrice)!);
                
                return new InventoryItemDto<TEnum, TPrice>
                {
                    Id = item.Id,
                    Name = item.Name,
                    AssetKey = item.AssetKey,
                    Type = Enum.Parse<TEnum>(item.Type),
                    Price = price,
                    IsFree = isFree,
                    DisplayOrder = item.DisplayOrder,
                    CanBePurchased = item.CanBePurchased,
                    MinLevel = item.MinLevel,
                    IsOwned = isFree, // Free items are always owned, others are not
                    IsEquipped = false
                };
            }).OrderBy(item => item.DisplayOrder).ToList();
        }
        
        // When userId is provided, include user-specific data
        var userItems = await userInventoryItemRepository.FindUserInventoryItems(userId.Value, false);
        var userItemIds = userItems.Select(ui => ui.InventoryItemId).ToHashSet();
        var equippedItemIds = userItems.Where(ui => ui.IsEquipped).Select(ui => ui.InventoryItemId).ToHashSet();

        return allItems.Select(item =>
        {
            var isFree = string.IsNullOrEmpty(item.Price) || item.Price == "null";
            var price = isFree ? default(TPrice)! : (DeserializePrice(item.Price) ?? default(TPrice)!);
            
            return new InventoryItemDto<TEnum, TPrice>
            {
                Id = item.Id,
                Name = item.Name,
                AssetKey = item.AssetKey,
                Type = Enum.Parse<TEnum>(item.Type),
                Price = price,
                IsFree = isFree,
                DisplayOrder = item.DisplayOrder,
                CanBePurchased = item.CanBePurchased,
                MinLevel = item.MinLevel,
                IsOwned = isFree || userItemIds.Contains(item.Id), // Free items are always owned
                IsEquipped = equippedItemIds.Contains(item.Id)
            };
        }).OrderBy(item => item.DisplayOrder).ToList();
    }

    public async Task<List<InventoryItemDto<TEnum, TPrice>>> GetAvailableInventoryItemsByType(long? userId, TEnum itemType)
    {
        var allItems = await GetAvailableInventoryItems(userId);
        return allItems.Where(item => item.Type.Equals(itemType)).ToList();
    }

    private TPrice? DeserializePrice(string? priceJson)
    {
        if (string.IsNullOrEmpty(priceJson) || priceJson == "null")
            return default;

        try
        {
            return JsonSerializer.Deserialize<TPrice>(priceJson);
        }
        catch
        {
            return default;
        }
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

    public async Task<List<UserInventoryItemDto<TEnum, TPrice>>> GetEquippedItems(long userId)
    {
        var equippedItems = await userInventoryItemRepository.FindEquippedItems(userId, false);
        
        // Group by type and take only the first equipped item per type (in case of manual DB edits)
        var uniqueEquippedItems = equippedItems
            .Where(ui => ui.InventoryItem != null)
            .GroupBy(ui => ui.InventoryItem!.Type)
            .Select(g => g.First())
            .ToList();
        
        return uniqueEquippedItems.Select(ui =>
        {
            var isFree = string.IsNullOrEmpty(ui.InventoryItem!.Price) || ui.InventoryItem.Price == "null";
            var price = isFree ? default(TPrice)! : (DeserializePrice(ui.InventoryItem.Price) ?? default(TPrice)!);
            
            return new UserInventoryItemDto<TEnum, TPrice>
            {
                Id = ui.Id,
                UserId = ui.UserId,
                InventoryItemId = ui.InventoryItemId,
                IsEquipped = ui.IsEquipped,
                InventoryItem = new InventoryItemDto<TEnum, TPrice>
                {
                    Id = ui.InventoryItem.Id,
                    AssetKey = ui.InventoryItem.AssetKey,
                    Type = Enum.Parse<TEnum>(ui.InventoryItem.Type),
                    Price = price,
                    IsFree = isFree,
                    DisplayOrder = ui.InventoryItem.DisplayOrder,
                    CanBePurchased = ui.InventoryItem.CanBePurchased,
                    MinLevel = ui.InventoryItem.MinLevel,
                    IsOwned = true,
                    IsEquipped = ui.IsEquipped
                }
            };
        }).OrderBy(ui => ui.InventoryItem!.DisplayOrder).ToList();
    }

    public async Task<Dictionary<TEnum, UserInventoryItemDto<TEnum, TPrice>>> GetEquippedItemsDictionary(long userId)
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


