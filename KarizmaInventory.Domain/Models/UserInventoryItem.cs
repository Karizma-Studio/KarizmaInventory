using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using KarizmaPlatform.Core.Database;

namespace KarizmaPlatform.Inventory.Domain.Models;

[Table("user_inventory_items")]
public class UserInventoryItem : BaseEntity
{
    [Column("user_id"), Required] public long UserId { get; init; }
    [Column("inventory_item_id"), Required] public long InventoryItemId { get; init; }
    [Column("is_equipped")] public bool IsEquipped { get; set; }

    [JsonIgnore] public InventoryItem? InventoryItem { get; init; }
    [JsonIgnore] public IInventoryUser? User { get; init; }
}

