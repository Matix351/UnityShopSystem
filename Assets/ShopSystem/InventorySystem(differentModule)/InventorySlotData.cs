using UnityEngine;

public class inventorySlotData
{

    [SerializeField] private IItemData _item;
    [SerializeField] private int _ammount;
    public IItemData item => _item;
    public int ammount => _ammount;
    public inventorySlotData(IItemData item, int ammount)
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
    public int AddItems(int ammount)
    {
        if (ammount <= 0)
        {
            return ammount;
        }

        int newAmmount = ammount + _ammount;
        int remaining = newAmmount - _item.MaxStackAmmount;

        if (remaining <= 0)
        {
            _ammount = newAmmount;
            return 0;
        }
        _ammount = _item.MaxStackAmmount;
        return remaining;

    }
    /// <summary>
    /// removes item, returns number of removedItemds
    /// </summary>
    /// <param name="ammount"></param>
    /// <returns></returns>
    public int RemoveItems(int ammount)
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

    public void ChangeAmmount(int newAmmount)
    {
        _ammount = newAmmount;
        if (newAmmount <= 0)
        {
            _ammount = 0;
            _item = null;

        }
    }

    public void ClearData()
    {
        _item = null;
        _ammount = 0;
    }

    public void ChangeData(IItemData newItem, int ammount)
    {
        _item = newItem;
        _ammount = ammount;
    }

    public void ChangeData(inventorySlotData data)
    {
        _item = data.item;
        _ammount = data.ammount;
    }

}