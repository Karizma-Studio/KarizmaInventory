using KarizmaPlatform.Core.Database;
using KarizmaPlatform.Inventory.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace KarizmaPlatform.Inventory.Infrastructure;

public interface IInventoryDatabase : IBaseContext
{
    DbSet<UserInventoryItem> GetUserInventoryItems();
    DbSet<InventoryItem> GetInventoryItems();
}

