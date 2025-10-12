namespace KarizmaPlatform.Inventory.SharedClasses.Dtos
{
    public class UserInventoryItemDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long InventoryItemId { get; set; }
        public bool IsEquipped { get; set; }
        public InventoryItemDto? InventoryItem { get; set; }
    }
}

