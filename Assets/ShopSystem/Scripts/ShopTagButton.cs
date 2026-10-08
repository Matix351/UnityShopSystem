using TMPro;
using UnityEngine;

public class ShopTagButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nametTV;
     private ITEM_TYPE _itemType;
     private bool _allItemTyoes = false;
    private ShopManager _shopManager;

    public void init(ITEM_TYPE name, ShopManager manager)
    {
        _itemType = name;
        _nametTV.SetText(name.ToString());
        _shopManager = manager;
    }

    public void initAllTypesTag(ShopManager manager)
    {
        _allItemTyoes = true;
        _itemType = (ITEM_TYPE)0;
        _shopManager = manager;
        _nametTV.SetText("All");


    }

    public void onClick()
    {
        _shopManager.OnTagClick(_itemType, _allItemTyoes);
    }
}
