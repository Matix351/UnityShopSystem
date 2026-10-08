using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemSlot : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameTV;
    [SerializeField] private TextMeshProUGUI _priceTV;
    [SerializeField] private TMP_InputField _inputAmmountTV;
    private ShopItem _itemData;
    public ShopItem ItemData => _itemData;
    private ShopManager _shopManager;

    

    public void init(SOItemData item, ShopManager manager)
    {

        if (item is IItemTradeable iTradeable)
        {
            ITEM_TYPE itemType = ITEM_TYPE.Other;

            if (item is IItemType iType)
            {
                itemType = iType.Type;
            }
            _itemData = new ShopItem(item, iTradeable, itemType);
            _shopManager = manager;

            _iconImage.sprite = item.image;
            _nameTV.text = item.Name;
            _priceTV.SetText(iTradeable.Value.ToString());
        }
        else
        {
            Debug.LogError(" ShopItemSlot init " + item.Name + " lacks interface (IItemTradeable or IItemType", this);
            return;
        }

    }

   
    public void AddToCartOneItem()
    {
        resetItemCount();
        addItemToCart();
    }

    public void AddToCartMultipleItems()
    {
        if(changeItemCount())
            addItemToCart();

        resetItemCount();
        _inputAmmountTV.text = "";
    }

    private bool changeItemCount()
    {
        if ( !(int.TryParse(_inputAmmountTV.text, out int count)) )
        {
            Debug.Log("WrongInput", this);
        }
        else if(count >= 0) 
        {
            _itemData.changeCount(count);
            return true;

        }

        return false;

    }

    private void resetItemCount()
    {
        _itemData.changeCount(1);
    }
    private void addItemToCart()
    {
        if (_itemData.Count <= 0) 
        {
            return;
        }
        Debug.Log("Adding " + _itemData.Count + " items to Cart");
        _shopManager.addToCart(_itemData);
        resetItemCount();
    }
}

//[System.Serializable]
//public class ShopItemData 
//{

//    [SerializeField] private SOItemData _item;
//    public SOItemData Item => _item;
//    private IShopItem _shopItem;
//    public int Value => _shopItem.Value;
//    public ITEM_TYPE Type => _shopItem.Type;




//    public ShopItemData(SOItemData item, IShopItem shopItem)
//    {
//        _item = item;
//        _shopItem = (IShopItem)item;
//    }

//    //public int CalulatePrice(int ammount)
//    //{
//    //    return _shopItem.Value * ammount;
//    //}
//}







