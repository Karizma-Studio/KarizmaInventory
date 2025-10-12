using System;

namespace KarizmaPlatform.Inventory.SharedClasses.Dtos
{
    public class UserInventoryItemDto<TEnum, TPrice> where TEnum : struct, Enum
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long InventoryItemId { get; set; }
        public bool IsEquipped { get; set; }
        public InventoryItemDto<TEnum, TPrice>? InventoryItem { get; set; }
    }
}

