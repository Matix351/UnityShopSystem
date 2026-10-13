public interface IItemTradeable
{
    public int Value { get; }

}

public interface IItemType
{
    public ITEM_TYPE Type { get; }

}


public interface IPurchaseHandler
{
    //invoke this everyTime state(money/slots) is changed
    public event System.Action StateChanged;
    public bool CanPurchaseMoney(int price);
    public bool CanPurchaseInventorySlots(inventorySlotData[] cartItems);
    // Validate current funds and capacity
    // Return false without changing money or inventory if the purchase cannot complete.
    bool TryPurchase(inventorySlotData[] items, int totalPrice);


}
