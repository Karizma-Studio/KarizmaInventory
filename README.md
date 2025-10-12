# KarizmaInventory

A flexible and generic inventory management system for .NET applications, designed with Clean Architecture principles. Perfect for game backends, e-commerce systems, or any application requiring inventory management.

## 🚀 Features

- **Generic Type System**: Define your own item types using enums
- **Flexible Pricing**: Support for any currency type (int, custom classes) stored as JSONB
- **Equipment System**: Users can equip one item per type simultaneously
- **Free Items**: Items with null price can be equipped without purchase
- **Clean Architecture**: Separated layers (Domain, Infrastructure, Application, SharedClasses)
- **DTO-based API**: Clean data transfer objects for API responses
- **Entity Framework Core**: Full EF Core integration with PostgreSQL
- **Auto-unequip**: Automatically unequip items of same type when equipping new ones

## 📦 Installation

Install the NuGet packages:

```bash
dotnet add package KarizmaInventory.Application
dotnet add package KarizmaInventory.Infrastructure
dotnet add package KarizmaInventory.Domain
dotnet add package KarizmaInventory.SharedClasses
```

## 🔧 Quick Start

### 1. Define Your Item Types

```csharp
public enum ItemType
{
    Skin,
    Avatar,
    Weapon,
    Badge
}

public class GameCurrency
{
    public int Coins { get; set; }
    public int Gems { get; set; }
}
```

### 2. Setup Your Database Context

```csharp
public class GameDbContext : BaseContext, IInventoryDatabase
{
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<UserInventoryItem> UserInventoryItems { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure inventory system with your User entity
        InventoryDatabaseUtilities.ConfigureDatabase<User>(modelBuilder);
    }

    // Implement IInventoryDatabase
    public DbSet<UserInventoryItem> GetUserInventoryItems() => UserInventoryItems;
    public DbSet<InventoryItem> GetInventoryItems() => InventoryItems;
}
```

### 3. Register Services

```csharp
services.AddKarizmaInventory<ItemType, GameCurrency, GameDbContext>();
```

### 4. Populate Your Database

Add items directly to the `inventory_items` table:

```sql
INSERT INTO inventory_items (id, asset_key, type, price, display_order, can_be_purchased)
VALUES 
  (1, 'BlueSkin', 'Skin', '{"Coins": 100, "Gems": 0}', 1, true),
  (2, 'RedSkin', 'Skin', null, 2, true),  -- Free item
  (3, 'CoolAvatar', 'Avatar', '{"Coins": 200, "Gems": 5}', 1, true);
```

### 5. Use the Inventory Processor

```csharp
public class InventoryController : ControllerBase
{
    private readonly IInventoryProcessor<ItemType, GameCurrency> _processor;

    public InventoryController(IInventoryProcessor<ItemType, GameCurrency> processor)
    {
        _processor = processor;
    }

    [HttpGet("items")]
    public async Task<List<InventoryItemDto>> GetAvailableItems(long userId)
    {
        return await _processor.GetAvailableInventoryItems(userId);
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseItem(long userId, long itemId)
    {
        var success = await _processor.AddInventoryItemToUser(userId, itemId);
        return success ? Ok() : BadRequest("Failed to purchase item");
    }

    [HttpPost("equip")]
    public async Task<IActionResult> EquipItem(long userId, long itemId)
    {
        var success = await _processor.EquipInventoryItem(userId, itemId);
        return success ? Ok() : BadRequest("Failed to equip item");
    }

    [HttpGet("equipped")]
    public async Task<List<UserInventoryItemDto>> GetEquippedItems(long userId)
    {
        return await _processor.GetEquippedItems(userId);
    }

    [HttpPost("unequip-type")]
    public async Task<IActionResult> UnequipByType(long userId, ItemType type)
    {
        var success = await _processor.UnequipInventoryItemsByType(userId, type);
        return Ok();
    }
}
```

## 📚 API Methods

### `GetAvailableInventoryItems(userId)`
Returns all inventory items with user-specific context (IsOwned, IsEquipped).

### `AddInventoryItemToUser(userId, itemId)`
Adds an item to user's inventory. Returns false if already owned.

### `EquipInventoryItem(userId, itemId)`
Equips an item. Automatically unequips other items of the same type. Free items (price = null) are auto-added to inventory.

### `UnequipInventoryItemsByType(userId, itemType)`
Unequips all items of a specific type for the user.

### `EquipInventoryItems(userId, itemIds)`
Batch equip multiple items at once.

### `DeleteInventoryItem(userId, itemId)`
Removes an item from user's inventory.

### `GetEquippedItems(userId)`
Returns all currently equipped items with full details.

## 🗃️ Database Schema

### `inventory_items`
| Column | Type | Description |
|--------|------|-------------|
| id | bigint | Primary key |
| asset_key | varchar(100) | Unique identifier for the item |
| type | varchar(50) | Item type (matches your enum) |
| price | jsonb | Price as JSON (nullable for free items) |
| display_order | int | Sort order for display |
| can_be_purchased | bool | Whether item can be purchased |
| created_date | timestamptz | Creation timestamp |
| updated_date | timestamptz | Last update timestamp |
| deleted_date | timestamptz | Soft delete timestamp |

### `user_inventory_items`
| Column | Type | Description |
|--------|------|-------------|
| id | bigint | Primary key |
| user_id | bigint | Foreign key to users |
| inventory_item_id | bigint | Foreign key to inventory_items |
| is_equipped | bool | Whether item is currently equipped |
| created_date | timestamptz | Creation timestamp |
| updated_date | timestamptz | Last update timestamp |
| deleted_date | timestamptz | Soft delete timestamp |

## 🎮 Equipment Rules

- Users can equip **one item per type** simultaneously
- Example: One Skin + One Avatar + One Weapon equipped at the same time
- Equipping a new item of the same type automatically unequips the old one
- Free items (price = null) can be equipped without purchasing

## 🏗️ Architecture

```
KarizmaInventory.SharedClasses    (DTOs, shared models)
         ↓
KarizmaInventory.Domain           (Entities, interfaces)
         ↓
KarizmaInventory.Infrastructure   (Repositories, DB config)
         ↓
KarizmaInventory.Application      (Business logic, processors)
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.

## 🔗 Links

- [GitHub Repository](https://github.com/Karizma-Studio/KarizmaInventory)
- [NuGet Package](https://www.nuget.org/packages/KarizmaInventory.Application)
- [Issues](https://github.com/Karizma-Studio/KarizmaInventory/issues)

## 💡 Support

For questions and support, please open an issue on GitHub.

---

Made with ❤️ by [Karizma Studio](https://github.com/Karizma-Studio)

