using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


public class ShopManager : MonoBehaviour
{

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

    //purchase Event
    public delegate bool PurchaseEvent(inventorySlotData[] cartItems , int price);
    public event PurchaseEvent OnPurchaseEvent;
    //canPurchaseEvent
    public delegate bool CanPurchaseMoneyDelegate(int price);
    public event CanPurchaseMoneyDelegate OnCanPurchaseMoneyEvent;
    public delegate bool CanPurchaseInventorySlotsDelegate(inventorySlotData[] cartItems);
    public event CanPurchaseInventorySlotsDelegate OnCanPurchaseInventorySlotsEvent;
    //openShopEvent
    public delegate void shopOpenEvent(bool isOpen);
    public event shopOpenEvent OnShopOpenEvent;

    UnityAction _onShopCloseAction = null;
    private bool _canPlayerPurchase = false;
    private int _cartPrice;


    public void openShop(SOItemData[] items, UnityAction onShopClosedAction = null)
    {
        Debug.Log("MATI: ShopManager go");
        for(int i=0; i<items.Length; i++)
        {
            if ( !(items[i] is IItemTradeable) )
            {
                Debug.Log("MATI: ShopManager one of item is not IItemTradeable");
                return;
            }
            //if (!(items[i] is IItemType))
            //{
            //    Debug.Log("MATI: ShopManager one of item is not IItemType");
            //    return;
            //}

        }
        Debug.Log("MATI: ShopManager AfterCheck");
        _onShopCloseAction = onShopClosedAction;

        //TODO
        //tmp[i] = items[i] as SOShopItemExample;
        //Create a container class for both interfaces and validate the results of the casts.
        //A non-null result indicates a successful cast; otherwise, log an error.

        //SHOP_ITEM_VIEW
        foreach (Transform child in _itemsSlotViewContent)
        {
            GameObject.Destroy(child.gameObject);
        }

        _itemSlots = new ShopItemSlot[items.Length];
        HashSet<ITEM_TYPE> tags = new HashSet<ITEM_TYPE>();

        for(int i =0; i< items.Length; i++)
        {
            ShopItemSlot itemSlot = Instantiate(_shopItemSlotPrefab, _itemsSlotViewContent);
            itemSlot.init(items[i], this);

            tags.Add((items[i] as IItemType).Type);
            
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

        //carItemSlots
        foreach (Transform child in _cartItemSlotViewContent)
        {
            GameObject.Destroy(child.gameObject);
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
        
        _onShopCloseAction.Invoke();
        _onShopCloseAction = null;

    }


    public void OnTagClick(ITEM_TYPE type, bool all = false)
    {
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
        foreach(ShopCartItemSlot slot in _cartItemsSlots)
        {
            if(slot.Item.ItemSO == item.ItemSO)
            {
                slot.updateData(item.Count + slot.Item.Count);
                return;
            }
        }

        ShopCartItemSlot itemSlot = Instantiate(_cartItemSlotPrefab, _cartItemSlotViewContent);
        _cartItemsSlots.Add(itemSlot);
        itemSlot.init(item, this);
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
    public bool checkCanPlayerPurchaseConditions()
    {
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
        if (!_canPlayerPurchase) 
        {
            return;
        }

        inventorySlotData[] cartItems = new inventorySlotData[_cartItemsSlots.Count];
        for(int i=0; i<cartItems.Length; i++)
        {
            cartItems[i] = new inventorySlotData(_cartItemsSlots[i].Item.ItemSO, _cartItemsSlots[i].Item.Count);
        }

        if (OnPurchaseEvent.Invoke(cartItems, _cartPrice))
        {
            clearCart();
        }
    }

    public void clearCart()
    {
        foreach(ShopCartItemSlot item in _cartItemsSlots)
        {
            Destroy(item.gameObject);
        }

        _cartItemsSlots.Clear();
        _cartPrice = 0;
        updateCartTVs();
    }

    private void updateCartTVs()
    {
        _cartPriceTV.SetText(_cartPrice.ToString());
    }

}

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
        _count = count;
        updatePrice();
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
        _price = _iTradeable.Value * Count;
    }

    public void changeCount(int count)
    {
        _count = count;
        updatePrice();
    }


}
