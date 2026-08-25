using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using KarizmaPlatform.Core.Database;
using KarizmaPlatform.Inventory.Domain.Utilities;

namespace KarizmaPlatform.Inventory.Domain.Models;

[Table("inventory_items")]
public class InventoryItem : BaseEntity
{
    [Column("type"), Required, MaxLength(50)] public required string Type { get; set; }
    [Column("asset_key"), Required, MaxLength(100)] public required string AssetKey { get; init; }
    [Column("name"), Required, MaxLength(150)] public required string Name { get; set; }
    [Column("price", TypeName = "jsonb")] public string? Price { get; set; }
    [Column("metadata", TypeName = "jsonb")] public string? Metadata { get; set; }
    [Column("display_order")] public int DisplayOrder { get; set; }
    [Column("can_be_purchased")] public bool CanBePurchased { get; set; }
    [Column("min_level")] public int MinLevel { get; set; } = 0;


    public TEnum GetTypeEnum<TEnum>() where TEnum : struct, Enum
    {
        return Enum.Parse<TEnum>(Type);
    }

    public TMetadata? GetMetadata<TMetadata>(JsonSerializerOptions? options = null)
    {
        return JsonUtilities.Deserialize<TMetadata>(Metadata, options);
    }
}
