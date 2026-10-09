using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Inventory/ShopItem")]
public class SOShopItemExample : SOItemData, IItemTradeable, IItemType
{
    [SerializeField] private int _value = 100;
    public int Value => _value;
    [SerializeField] private ITEM_TYPE _type;
    public ITEM_TYPE Type => _type;


}


