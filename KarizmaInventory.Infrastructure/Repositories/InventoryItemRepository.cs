using KarizmaPlatform.Core.Database;
using KarizmaPlatform.Core.Logic;
using KarizmaPlatform.Inventory.Domain.Models;
using KarizmaPlatform.Inventory.Infrastructure.Repositories.Interfaces;

namespace KarizmaPlatform.Inventory.Infrastructure.Repositories;

public class InventoryItemRepository(BaseContext baseContext) : BaseRepository<InventoryItem>(baseContext), IInventoryItemRepository
{
}

