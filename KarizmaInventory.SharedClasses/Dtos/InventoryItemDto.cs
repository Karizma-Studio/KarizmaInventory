namespace KarizmaPlatform.Inventory.SharedClasses.Dtos
{
    public class InventoryItemDto
    {
        public long Id { get; set; }
        public string AssetKey { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Price { get; set; }
        public int DisplayOrder { get; set; }
        public bool CanBePurchased { get; set; }
        public bool IsOwned { get; set; }
        public bool IsEquipped { get; set; }
    }
}

