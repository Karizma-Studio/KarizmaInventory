using System;

namespace KarizmaPlatform.Inventory.SharedClasses.Dtos
{
    public class InventoryItemDto<TEnum, TPrice> where TEnum : struct, Enum
    {
        public long Id { get; set; }
        public string AssetKey { get; set; } = string.Empty;
        public TEnum Type { get; set; }
        public TPrice Price { get; set; } = default!;
        public bool IsFree { get; set; }
        public int DisplayOrder { get; set; }
        public bool CanBePurchased { get; set; }
        public int MinLevel { get; set; }
        public bool IsOwned { get; set; }
        public bool IsEquipped { get; set; }
    }
}

