using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class ShopManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Min(1)]
    [Tooltip("Maximum quantity of each item in the cart, independent of inventory stack size.")]
    private int _maxQuantityPerItem = 999;
    public int GetMaxQuantityPerItem => _maxQuantityPerItem;
    [Header("SetUp")]

    [SerializeField] private ShopItemSlot _shopItemSlotPrefab;
    [SerializeField] private Transform _itemsSlotViewContent;
    
    [SerializeField] private ShopTagButton _tagButtonPrefab;
    [SerializeField] private Transform _tagViewContent;


    private ShopItemSlot[] _itemSlots;
    //cart
    [SerializeField] private ShopCartItemSlot _cartItemSlotPrefab;
    [SerializeField] private Transform _cartItemSlotViewContent;
    private List<ShopCartItemSlot> _cartItemsSlots = new List<ShopCartItemSlot>();
    [SerializeField] private TextMeshProUGUI _cartPriceTV;
    [SerializeField] private TextMeshProUGUI _cartErrorBuyTV;

    private IShopPurchaseHandler _purchaseHandler;
    private bool _purchaseInProgress;
    public event Action<inventorySlotData[], int> OnPurchaseCompleted;

    public void RegisterPurchaseHandler(IShopPurchaseHandler handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));
        if (_purchaseHandler != null && !ReferenceEquals(_purchaseHandler, handler)
            && !(_purchaseHandler is UnityEngine.Object owner && owner == null))
            throw new InvalidOperationException("The shop already has a purchase handler.");
        _purchaseHandler = handler;
    }

    public void UnregisterPurchaseHandler(IShopPurchaseHandler handler)
    {
        if (ReferenceEquals(_purchaseHandler, handler))
            _purchaseHandler = null;
    }
    //canPurchaseEvent
    public delegate bool CanPurchaseMoneyDelegate(int price);
    public event CanPurchaseMoneyDelegate OnCanPurchaseMoneyEvent;
    public delegate bool CanPurchaseInventorySlotsDelegate(inventorySlotData[] cartItems);
    public event CanPurchaseInventorySlotsDelegate OnCanPurchaseInventorySlotsEvent;
    //openShopEvent
    //public delegate void shopOpenEvent(bool isOpen);
    //public event shopOpenEvent OnShopOpenEvent;

    UnityAction _onShopCloseAction = null;
    private bool _canPlayerPurchase = false;
    private int _cartPrice;


    public void openShop(SOItemData[] items, UnityAction onShopClosedAction = null)
    {
        Debug.Log("MATI: ShopManager go");
        for(int i=0; i<items.Length; i++)
        {
            if (items[i] == null || !(items[i] is IItemTradeable tradeable) )
            {
                Debug.LogError("Shop items must be non-null and tradeable", this);
                return;
            }

        }
        Debug.Log("MATI: ShopManager AfterCheck");
        _onShopCloseAction = onShopClosedAction;
        clearCart();

        //CLEAR_SHOP_ITEM_VIEW
        foreach (Transform child in _itemsSlotViewContent)
        {
            GameObject.Destroy(child.gameObject);
        }
        //CLEAR_CART_ITEM_VIEW
        foreach (Transform child in _cartItemSlotViewContent)
        {
            GameObject.Destroy(child.gameObject);
        }


        _itemSlots = new ShopItemSlot[items.Length];
        HashSet<ITEM_TYPE> tags = new HashSet<ITEM_TYPE>();

        for(int i =0; i< items.Length; i++)
        {
            ShopItemSlot itemSlot = Instantiate(_shopItemSlotPrefab, _itemsSlotViewContent);
            itemSlot.init(items[i], this);

            tags.Add(itemSlot.ItemData.Type);
            
            _itemSlots[i] = itemSlot;
        }

        //TAG_VIEW
        foreach (Transform child in _tagViewContent)
        {
            GameObject.Destroy(child.gameObject);
        }

        ShopTagButton allTagView = Instantiate(_tagButtonPrefab, _tagViewContent);
        allTagView.initAllTypesTag(this);

        foreach (ITEM_TYPE tag in tags)
        {
            ShopTagButton tagView = Instantiate(_tagButtonPrefab, _tagViewContent);
            tagView.init(tag, this);
        }




        _cartErrorBuyTV.gameObject.SetActive(false);
        Debug.Log("MATI: ShopManager setActiveTreu");
        this.gameObject.SetActive(true);
        Debug.Log("MATI: ShopManager active = " + this.gameObject.activeSelf);

        //OnShopOpenEvent?.Invoke(true);

    }

    //TODO: Clear the cart and shop items, and reset all state so the shop is empty.
    public void closeShop()
    {
        Debug.Log("MATI: ShopManager CloseShop");
        clearCart();
        this.gameObject.SetActive(false);
        //OnShopOpenEvent?.Invoke(false);

        UnityAction onShopClosed = _onShopCloseAction;
        _onShopCloseAction = null;
        onShopClosed?.Invoke();

    }


    public void OnTagClick(ITEM_TYPE type, bool all = false)
    {
        foreach (ShopTagButton tag in _tagViewContent.GetComponentsInChildren<ShopTagButton>())
            tag.UpdateSelection(type, all);
        var itemScroll = _itemsSlotViewContent.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        if (itemScroll != null) itemScroll.verticalNormalizedPosition = 1;
        if(all)
        {
            foreach (ShopItemSlot slot in _itemSlots)
            {
                slot.gameObject.SetActive(true);
            }

            return;
        }

        foreach (ShopItemSlot slot in _itemSlots)
        {
            if(slot.ItemData.Type == type)
            {
                slot.gameObject.SetActive(true);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }

    public void addToCart(ShopItem item)
    {
        if (item == null || item.Count <= 0)
            return;

        foreach(ShopCartItemSlot slot in _cartItemsSlots)
        {
            if(slot.Item.ItemSO == item.ItemSO)
            {
                int newQuantity = item.Count + slot.Item.Count;
                if(newQuantity >  _maxQuantityPerItem)
                    newQuantity = _maxQuantityPerItem;

                slot.updateData(newQuantity);
                return;
            }
        }



        ShopItem cartItem = new ShopItem(item);
        if(item.Count > _maxQuantityPerItem)
            cartItem.changeCount(_maxQuantityPerItem);
        else
            cartItem.changeCount(item.Count);

        ShopCartItemSlot itemSlot = Instantiate(_cartItemSlotPrefab, _cartItemSlotViewContent);
        _cartItemsSlots.Add(itemSlot);
        itemSlot.init(cartItem, this);
    }


    public void OnCartItemDataChange(ShopCartItemSlot cartItemSlot)
    {
        if (cartItemSlot.Item.Count <= 0)
        {
            _cartItemsSlots.Remove(cartItemSlot);
            Destroy(cartItemSlot.gameObject);
        }

        _cartPrice = 0;
        foreach (ShopCartItemSlot slot in _cartItemsSlots)
        {
            _cartPrice += slot.Item.PriceMultiple;

        }
        checkCanPlayerPurchaseConditions();
        updateCartTVs();

    }

    //TODO: TEST
    //NOTTESTED
    public void updatePlayerPurchaseConditions()
    {
        checkCanPlayerPurchaseConditions();
    }

    public bool checkCanPlayerPurchaseConditions()
    {
        if (_cartItemsSlots.Count == 0)
        {
            _canPlayerPurchase = false;
            _cartErrorBuyTV.gameObject.SetActive(false);
            return false;
        }

        bool enougMoney = true;
        bool enoughInventorySlots = true;

        if (OnCanPurchaseMoneyEvent != null)
        {
            foreach (CanPurchaseMoneyDelegate handler in OnCanPurchaseMoneyEvent.GetInvocationList())
            {
                enougMoney &= handler(_cartPrice); // all subscribers must agree
            }
        }

        inventorySlotData[] cartItems = new inventorySlotData[_cartItemsSlots.Count];
        for (int i = 0; i < cartItems.Length; i++)
        {
            cartItems[i] = new inventorySlotData(_cartItemsSlots[i].Item.ItemSO, _cartItemsSlots[i].Item.Count);
        }
        if (OnCanPurchaseInventorySlotsEvent != null)
        {
            foreach (CanPurchaseInventorySlotsDelegate handler in OnCanPurchaseInventorySlotsEvent.GetInvocationList())
            {
                enoughInventorySlots &= handler(cartItems); // all subscribers must agree
            }
        }
        _canPlayerPurchase = (enoughInventorySlots && enougMoney);

        if (!_canPlayerPurchase)
        {
            string errorMsg = "";
            if (!enougMoney) 
            {
                errorMsg += "not enough Money";
            }
            if (!enoughInventorySlots) 
            {
                errorMsg += "\n not enoug Inventory Space";
            }
            _cartErrorBuyTV.SetText(errorMsg);
            _cartErrorBuyTV.gameObject.SetActive(true);
        }
        else
        {
            _cartErrorBuyTV.gameObject.SetActive(false);

        }

        return _canPlayerPurchase;


    }

    public void onPurchaseButtonClick()
    {
        if (_purchaseInProgress)
            return;
        _purchaseInProgress = true;
        try
        {
            if (!checkCanPlayerPurchaseConditions())
                return;

            if (_purchaseHandler == null ||
                (_purchaseHandler is UnityEngine.Object owner && owner == null))
            {
                ShowPurchaseError("No purchase handler is configured.");
                return;
            }

            inventorySlotData[] cartItems = new inventorySlotData[_cartItemsSlots.Count];
            for (int i = 0; i < cartItems.Length; i++)
                cartItems[i] = new inventorySlotData(_cartItemsSlots[i].Item.ItemSO, _cartItemsSlots[i].Item.Count);
            int totalPrice = _cartPrice;

            if (!_purchaseHandler.TryPurchase(cartItems, totalPrice))
            {
                ShowPurchaseError("Purchase failed. Please try again.");
                return;
            }

            clearCart();
            if (OnPurchaseCompleted != null)
            {
                foreach (Action<inventorySlotData[], int> listener in OnPurchaseCompleted.GetInvocationList())
                {
                    try { listener(cartItems, totalPrice); }
                    catch (Exception exception) { Debug.LogException(exception, this); }
                }
            }
        }
        finally
        {
            _purchaseInProgress = false;
        }
    }

    private void ShowPurchaseError(string message)
    {
        _cartErrorBuyTV.SetText(message);
        _cartErrorBuyTV.gameObject.SetActive(true);
    }

    public void clearCart()
    {
        foreach(ShopCartItemSlot item in _cartItemsSlots)
        {
            Destroy(item.gameObject);
        }

        _cartItemsSlots.Clear();
        _cartPrice = 0;
        _canPlayerPurchase = false;
        _cartErrorBuyTV.gameObject.SetActive(false);
        updateCartTVs();
    }

    private void updateCartTVs()
    {
        _cartPriceTV.SetText(_cartPrice.ToString());
    }

}

[System.Serializable]
public class ShopItem
{
    private SOItemData _item;
    public SOItemData ItemSO => _item;
    IItemTradeable _iTradeable;
    ITEM_TYPE _type;
    //private ShopItemData _shopItem;
    private int _count;
    public int Count => _count;
    private int _price;
    public int PriceMultiple => _price;
    public int pricePerUnit => _iTradeable.Value;
    //public ITEM_TYPE Type => _shopItem.Type;
    public ITEM_TYPE Type => _type;

    public ShopItem(SOItemData item, IItemTradeable iTradeable, ITEM_TYPE type, int count = 1)
    {
        _item = item;
        _iTradeable = iTradeable;
        _type = type;
        changeCount(count);
    }

    public ShopItem(ShopItem item )
    {
        _item = item._item;
        _iTradeable = item._iTradeable;
        _type = item._type;
        _count = item._count;

        updatePrice();
    }

    public void updatePrice()
    {
        if (_iTradeable.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(_iTradeable), "Item prices cannot be negative.");
        _price = checked(_iTradeable.Value * Count);
    }

    public void changeCount(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (_iTradeable.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(_iTradeable), "Item prices cannot be negative.");

        int price = checked(_iTradeable.Value * count);
        _count = count;
        _price = price;
    }


}
