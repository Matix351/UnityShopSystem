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

    // The illustrated UI keeps a quantity field visible beside its Add button.
    public void AddSelectedQuantity()
    {
        if (string.IsNullOrWhiteSpace(_inputAmmountTV.text))
            AddToCartOneItem();
        else
            AddToCartMultipleItems();
        _inputAmmountTV.text = "1";
    }

    public void IncreaseSelectedQuantity() => ChangeSelectedQuantity(1);
    public void DecreaseSelectedQuantity() => ChangeSelectedQuantity(-1);

    private void ChangeSelectedQuantity(int delta)
    {
        int.TryParse(_inputAmmountTV.text, out int count);
        int maximum = _shopManager != null ? _shopManager.GetMaxQuantityPerItem : 999;
        _inputAmmountTV.text = Math.Max(1L, Math.Min((long)maximum, (long)count + delta)).ToString();
    }

    public void AddToCartMultipleItems()
    {
        if(setPlayerInputAmmount())
            addItemToCart();

        resetItemCount();
        _inputAmmountTV.text = "";
    }

    private bool setPlayerInputAmmount()
    {
        if ( !(int.TryParse(_inputAmmountTV.text, out int count)) )
        {
            Debug.Log("WrongInput", this);
        }
        else if(count > 0)
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






