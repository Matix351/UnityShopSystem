using UnityEngine;

public class inventorySlotData
{

    [SerializeField] private SOItemData _item;
    [SerializeField] private int _ammount;
    public SOItemData item => _item;
    public int ammount => _ammount;
    public inventorySlotData(SOItemData item, int ammount)
    {
        _item = item;
        _ammount = ammount;
    }

    public inventorySlotData(inventorySlotData copy)
    {
        _item = copy.item;
        _ammount = copy.ammount;
    }

    public inventorySlotData()
    {
        _item = null;
        _ammount = 0;
    }

    /// <summary>
    /// addItemsToSlot, returns reamining ammount of items that didn't fit in invetoryStack
    /// </summary>
    /// <param name="ammount"></param>
    /// <returns></returns>
    public int addItems(int ammount)
    {
        if (ammount <= 0)
        {
            return ammount;
        }

        int newAmmount = ammount + _ammount;
        int remaining = newAmmount - _item.maxStackAmmount;

        if (remaining <= 0)
        {
            _ammount = newAmmount;
            return 0;
        }
        _ammount = _item.maxStackAmmount;
        return remaining;

    }
    /// <summary>
    /// removes item, returns number of removedItemds
    /// </summary>
    /// <param name="ammount"></param>
    /// <returns></returns>
    public int removeItems(int ammount)
    {
        if (ammount <= 0)
            return 0;

        int removedAmount = 0;
        int newAmmount = _ammount - ammount;

        if (newAmmount > 0)
        {
            _ammount = newAmmount;
            //return ammount;
            removedAmount = ammount;
        }
        else
        {
            removedAmount = _ammount;
            _ammount = 0;
            _item = null;
        }

        return removedAmount;
    }

    public void changeAmmount(int newAmmount)
    {
        _ammount = newAmmount;
        if (newAmmount == 0)
        {
            _item = null;

        }
    }

    public void clearData()
    {
        _item = null;
        _ammount = 0;
    }

    public void changeData(SOItemData newItem, int ammount)
    {
        _item = newItem;
        _ammount = ammount;
    }

    public void changeData(inventorySlotData data)
    {
        _item = data.item;
        _ammount = data.ammount;
    }

}