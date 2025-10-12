using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KarizmaPlatform.Core.Database;

namespace KarizmaPlatform.Inventory.Domain.Models;

[Table("inventory_items")]
public class InventoryItem : BaseEntity
{
    [Column("type"), Required, MaxLength(50)] public required string Type { get; set; }
    [Column("asset_key"), Required, MaxLength(100)] public required string AssetKey { get; init; }
    [Column("price", TypeName = "jsonb")] public string? Price { get; set; }
    [Column("display_order")] public int DisplayOrder { get; set; }
    [Column("can_be_purchased")] public bool CanBePurchased { get; set; }


    public TEnum GetTypeEnum<TEnum>() where TEnum : struct, Enum
    {
        return Enum.Parse<TEnum>(Type);
    }
}

