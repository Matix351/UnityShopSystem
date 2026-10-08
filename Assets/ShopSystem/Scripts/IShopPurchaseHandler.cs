public interface IShopPurchaseHandler
{
    // Validate current funds and capacity, then commit the whole purchase.
    // Return false without changing money or inventory if the purchase cannot complete.
    bool TryPurchase(inventorySlotData[] items, int totalPrice);
}
