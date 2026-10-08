using UnityEngine;
using UnityEngine.UI;

//public class PlayerDebug : MonoBehaviour, IcanPurchaseInventorySlots, ICanPurchaseMoney
public class PlayerDebug : MonoBehaviour, IShopPurchaseHandler
{
    [SerializeField] private ShopManager _shopManager;
    [SerializeField] private Toggle _hasEnougMoneyToggle;
    [SerializeField] private Toggle _hasEnougInventorySpaceToggle;
    [SerializeField] private bool _hasEnoughInventorySpace;
    [SerializeField] private bool _hasEnougMoney;




    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        _hasEnougMoneyToggle.SetIsOnWithoutNotify(_hasEnougMoney);
        _hasEnougInventorySpaceToggle.SetIsOnWithoutNotify(_hasEnoughInventorySpace);
        _shopManager.checkCanPlayerPurchaseConditions();
    }

    #region debugToolsSetup

    public void setEnoughMoney(bool money)
    {
        _hasEnougMoney = money;
        _shopManager.checkCanPlayerPurchaseConditions();

    }

    public void setEnougInventorySlots(bool inventorySlots)
    {
        _hasEnoughInventorySpace = inventorySlots;
        _shopManager.checkCanPlayerPurchaseConditions();

    }

#endregion


    private void OnEnable()
    {
        _shopManager.RegisterPurchaseHandler(this);
        _shopManager.OnCanPurchaseMoneyEvent += CanPurchaseMoney;
        _shopManager.OnCanPurchaseInventorySlotsEvent += CanPurchaseInventorySlots;
        _shopManager.OnPurchaseCompleted += generatePurchaseLog;

        _hasEnougInventorySpaceToggle.onValueChanged.AddListener(setEnougInventorySlots);
        _hasEnougMoneyToggle.onValueChanged.AddListener(setEnoughMoney);

        _hasEnougMoneyToggle.SetIsOnWithoutNotify(_hasEnougMoney);
        _hasEnougInventorySpaceToggle.SetIsOnWithoutNotify(_hasEnoughInventorySpace);

    }
    private void OnDisable()
    {
        if (_shopManager != null)
        {
            _shopManager.UnregisterPurchaseHandler(this);
            _shopManager.OnCanPurchaseMoneyEvent -= CanPurchaseMoney;
            _shopManager.OnCanPurchaseInventorySlotsEvent -= CanPurchaseInventorySlots;
            _shopManager.OnPurchaseCompleted -= generatePurchaseLog;
        }
        _hasEnougInventorySpaceToggle.onValueChanged.RemoveListener(setEnougInventorySlots);
        _hasEnougMoneyToggle.onValueChanged.RemoveListener(setEnoughMoney);
    }

    public bool TryPurchase(inventorySlotData[] cartItems, int price)
    {
        // Debug simulation only; a real handler must commit money and inventory together.
        return canPurchase();
    }

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

    public bool CanPurchaseMoney(int price)
    {
        return _hasEnougMoney;
    }

    public bool CanPurchaseInventorySlots(inventorySlotData[] cartItems)
    {
        return _hasEnoughInventorySpace;
    }

    private bool canPurchase()
    {
        return _hasEnoughInventorySpace && _hasEnougMoney;
    }

}


//public interface IonPurchase
//{
//    public bool onPurchase(inventorySlotData[] cartItems, int price);
//}

//public interface ICanPurchaseMoney
//{
//    public bool CanPurchaseMoney(int price);

//}

//public interface IcanPurchaseInventorySlots
//{
//    public bool CanPurchaseInventorySlots(inventorySlotData[] cartItems);

//}

