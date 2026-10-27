using System;
using System.Collections.Generic;
using TMPro;
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

    private IPurchaseHandler _purchaseHandler;
    private bool _purchaseInProgress;
    public event Action<inventorySlotData[], int> OnPurchaseCompleted;

    private void registerPurchaseHandler(IPurchaseHandler handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));
        if (ReferenceEquals(_purchaseHandler, handler))
            return;
        if (_purchaseHandler != null
            && !(_purchaseHandler is UnityEngine.Object owner 
            && owner == null))
            throw new InvalidOperationException("The shop already has a purchase handler.");
        _purchaseHandler = handler;
        _purchaseHandler.StateChanged += UpdatePlayerPurchaseConditions;
    }

    private void unregisterPurchaseHandler()
    {
        if (_purchaseHandler == null)
            return;

        _purchaseHandler.StateChanged -= UpdatePlayerPurchaseConditions;
        _purchaseHandler = null;
    }

    private void OnDisable()
    {
        CloseShop();
    }

    UnityAction _onShopCloseAction = null;
    private bool _canPlayerPurchase = false;
    private int _cartPrice;


    public void OpenShop(IItemData[] items,IPurchaseHandler purchaseHandler, UnityAction onShopClosedAction = null)
    {
        if(this.gameObject.activeSelf)
        {
            return;
        }

        Debug.Log("MATI: ShopManager go");
        for(int i=0; i<items.Length; i++)
        {
            if (items[i] == null || !(items[i] is IItemTradeable tradeable) )
            {
                Debug.LogError("Shop items must be non-null and implement  IItemTradeable", this);
                return;
            }

        }

        registerPurchaseHandler(purchaseHandler);

        Debug.Log("MATI: ShopManager AfterCheck");
        _onShopCloseAction = onShopClosedAction;
        ClearCart();

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
            itemSlot.Init(items[i], this);

            tags.Add(itemSlot.ItemData.Type);
            
            _itemSlots[i] = itemSlot;
        }

        //TAG_VIEW
        foreach (Transform child in _tagViewContent)
        {
            GameObject.Destroy(child.gameObject);
        }

        ShopTagButton allTagView = Instantiate(_tagButtonPrefab, _tagViewContent);
        allTagView.InitAllTypesTag(this);

        foreach (ITEM_TYPE tag in tags)
        {
            ShopTagButton tagView = Instantiate(_tagButtonPrefab, _tagViewContent);
            tagView.Init(tag, this);
        }




        _cartErrorBuyTV.gameObject.SetActive(false);
        Debug.Log("MATI: ShopManager setActiveTreu");
        this.gameObject.SetActive(true);
        Debug.Log("MATI: ShopManager active = " + this.gameObject.activeSelf);

        //OnShopOpenEvent?.Invoke(true);

    }

    //TODO: Clear the cart and shop items, and reset all state so the shop is empty.
    public void CloseShop()
    {
        ClearCart();
        unregisterPurchaseHandler();
        this.gameObject.SetActive(false);
        UnityAction onShopClosed = _onShopCloseAction;
        _onShopCloseAction = null;
        OnPurchaseCompleted = null;
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

    public void AddToCart(ShopItem item)
    {
        if (item == null || item.Count <= 0)
            return;

        foreach(ShopCartItemSlot slot in _cartItemsSlots)
        {
            if(slot.Item.IItemData == item.IItemData)
            {
                int newQuantity = item.Count + slot.Item.Count;
                if(newQuantity >  _maxQuantityPerItem)
                    newQuantity = _maxQuantityPerItem;

                slot.UpdateData(newQuantity);
                return;
            }
        }



        ShopItem cartItem = new ShopItem(item);
        if(item.Count > _maxQuantityPerItem)
            cartItem.ChangeCount(_maxQuantityPerItem);
        else
            cartItem.ChangeCount(item.Count);

        ShopCartItemSlot itemSlot = Instantiate(_cartItemSlotPrefab, _cartItemSlotViewContent);
        _cartItemsSlots.Add(itemSlot);
        itemSlot.Init(cartItem, this);
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
        CheckCanPlayerPurchaseConditions();
        updateCartTVs();

    }

    //TODO: TEST
    //TESTED xD
    public void UpdatePlayerPurchaseConditions()
    {
        CheckCanPlayerPurchaseConditions();
    }

    public bool CheckCanPlayerPurchaseConditions()
    {
        if (_purchaseHandler == null ||
            (_purchaseHandler is UnityEngine.Object owner && owner == null))
            {
            showPurchaseError("No purchase handler is configured.");
            return false;
            }

        if (_cartItemsSlots.Count == 0)
        {
            _canPlayerPurchase = false;
            _cartErrorBuyTV.gameObject.SetActive(false);
            return false;
        }

        bool enougMoney =false;
        bool enoughInventorySlots = false;
        enougMoney = _purchaseHandler.CanPurchaseMoney(_cartPrice);
        inventorySlotData[] cartItems = new inventorySlotData[_cartItemsSlots.Count];
        for (int i = 0; i < cartItems.Length; i++)
        {
            cartItems[i] = new inventorySlotData(_cartItemsSlots[i].Item.IItemData, _cartItemsSlots[i].Item.Count);
        }
        enoughInventorySlots = _purchaseHandler.CanPurchaseInventorySlots(cartItems);


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

    public void OnPurchaseButtonClick()
    {
        if (_purchaseInProgress)
            return;
        _purchaseInProgress = true;
        try
        {
            if (!CheckCanPlayerPurchaseConditions())
                return;


            inventorySlotData[] cartItems = new inventorySlotData[_cartItemsSlots.Count];
            for (int i = 0; i < cartItems.Length; i++)
                cartItems[i] = new inventorySlotData(_cartItemsSlots[i].Item.IItemData, _cartItemsSlots[i].Item.Count);
            int totalPrice = _cartPrice;

            if (!_purchaseHandler.TryPurchase(cartItems, totalPrice))
            {
                showPurchaseError("Purchase failed. Please try again.");
                return;
            }

            ClearCart();
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

    private void showPurchaseError(string message)
    {
        _cartErrorBuyTV.SetText(message);
        _cartErrorBuyTV.gameObject.SetActive(true);
    }

    public void ClearCart()
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
    private IItemData _item;
    public IItemData IItemData => _item;
    IItemTradeable _iTradeable;
    ITEM_TYPE _type;
    private int _count;
    public int Count => _count;
    private int _price;
    public int PriceMultiple => _price;
    public int pricePerUnit => _iTradeable.Value;
    public ITEM_TYPE Type => _type;

    public ShopItem(IItemData item, IItemTradeable iTradeable, ITEM_TYPE type, int count = 1)
    {
        _item = item;
        _iTradeable = iTradeable;
        _type = type;
        ChangeCount(count);
    }

    public ShopItem(ShopItem item )
    {
        _item = item._item;
        _iTradeable = item._iTradeable;
        _type = item._type;
        _count = item._count;

        UpdatePrice();
    }

    public void UpdatePrice()
    {
        if (_iTradeable.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(_iTradeable), "Item prices cannot be negative.");
        _price = checked(_iTradeable.Value * Count);
    }

    public void ChangeCount(int count)
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
