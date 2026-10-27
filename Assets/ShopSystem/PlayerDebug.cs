using UnityEngine;

public class PlayerDebug : MonoBehaviour, IPurchaseHandler
{
    [SerializeField] private ShopManager _shopManager;
    [SerializeField] private bool _hasEnoughInventorySpace;
    [SerializeField] private bool _hasEnougMoney;
    public event System.Action StateChanged;

    #region mainLogic


    public bool CanPurchaseMoney(int price)
    {
        return _hasEnougMoney;
    }

    public bool CanPurchaseInventorySlots(inventorySlotData[] cartItems)
    {
        return _hasEnoughInventorySpace;
    }


    public bool TryPurchase(inventorySlotData[] cartItems, int price)
    {
        if(_hasEnoughInventorySpace && _hasEnougMoney)
        {
            //You want to remove money here and add items to player inventory
            generatePurchaseLog(cartItems, price);
            return true;
        }
        else
        {
            return false;
        }
    }


    #endregion

    private void generatePurchaseLog(inventorySlotData[] cartItems, int price)
    {
        string message = "PurchaseList - expand this log for details";
        message = (message + "\n" + "cartItems List length =" + cartItems.Length);

        string orderList = "";
        for (int i = 0; i < cartItems.Length; i++)
        {
            orderList += ("\n" + "[ " + i + " ] = " + cartItems[i].item.Name + " x" + cartItems[i].ammount);
        }
        message = message + "\n" + orderList;
        message = message + "\n" + "total  price = " + price;
        message = message + "\n" + "purchase successful = true\n";
        Debug.Log(message);
    }

    #region debugToolsSetup
    public bool HasEnoughMoney => _hasEnougMoney;
    public bool HasEnoughInventorySpace => _hasEnoughInventorySpace;

    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        StateChanged?.Invoke();
    }

    public void SetEnoughMoney(bool money)
    {
        _hasEnougMoney = money;
        StateChanged?.Invoke();
    }

    public void SetEnougInventorySlots(bool inventorySlots)
    {
        _hasEnoughInventorySpace = inventorySlots;
        StateChanged?.Invoke();
    }
    #endregion
}
