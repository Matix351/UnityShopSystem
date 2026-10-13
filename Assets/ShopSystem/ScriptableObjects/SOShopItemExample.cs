using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Inventory/ShopItem")]
public class SOShopItemExample : ScriptableObject, IItemData, IItemTradeable, IItemType
{
    [SerializeField] private string _name;
    [SerializeField] private Sprite _image;
    [SerializeField] private int _maxStackAmmount = 64;
    [SerializeField] private int _value = 100;
    public int Value => _value;
    [SerializeField] private ITEM_TYPE _type;
    public ITEM_TYPE Type => _type;

    public string Name => _name;

    public Sprite Image => _image;

    public int MaxStackAmmount => _maxStackAmmount;
}


