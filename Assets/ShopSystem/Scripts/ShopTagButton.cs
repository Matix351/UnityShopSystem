using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopTagButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nametTV;
    [SerializeField] private Image _background;

    [Header("Selection Colors")]
    [SerializeField] private Color _selectedBackgroundColor = new Color(0.43f, 0.57f, 0.29f);
    [SerializeField] private Color _unselectedBackgroundColor = new Color(0.85f, 0.9f, 0.72f);
    [SerializeField] private Color _selectedTextColor = new Color(1, 0.97f, 0.87f);
    [SerializeField] private Color _unselectedTextColor = new Color(0.16f, 0.23f, 0.15f);

     private ITEM_TYPE _itemType;
     private bool _allItemTyoes = false;
    private ShopManager _shopManager;

    public void init(ITEM_TYPE name, ShopManager manager)
    {
        _itemType = name;
        _nametTV.SetText(name.ToString());
        _shopManager = manager;
        SetSelected(false);
    }

    public void initAllTypesTag(ShopManager manager)
    {
        _allItemTyoes = true;
        _itemType = (ITEM_TYPE)0;
        _shopManager = manager;
        _nametTV.SetText("All");
        SetSelected(true);


    }

    public void onClick()
    {
        _shopManager.OnTagClick(_itemType, _allItemTyoes);
    }

    public void UpdateSelection(ITEM_TYPE type, bool all)
    {
        SetSelected(all ? _allItemTyoes : !_allItemTyoes && _itemType == type);
    }

    private void SetSelected(bool selected)
    {
        if (_background == null) return;
        _background.color = selected ? _selectedBackgroundColor : _unselectedBackgroundColor;
        _nametTV.color = selected ? _selectedTextColor : _unselectedTextColor;
    }
}
