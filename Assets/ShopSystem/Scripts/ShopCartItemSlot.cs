using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCartItemSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countTV;
    [SerializeField] private TextMeshProUGUI _nameTV;
    [SerializeField] private TextMeshProUGUI _priceTV;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _unitPriceTV;

    private ShopItem _item;
    public ShopItem Item => _item;

    private ShopManager _manager;


    public void Init(ShopItem item, ShopManager manager)
    {
        _item = new ShopItem(item);
        _manager = manager;
        _nameTV.SetText(item.IItemData.Name);
        if (_iconImage != null) _iconImage.sprite = item.IItemData.Image;
        if (_unitPriceTV != null) _unitPriceTV.SetText(_item.pricePerUnit.ToString());
        updateCountTVs();
        _manager.OnCartItemDataChange(this);
    }

    public void UpdateData(int count)
    {
        if(count > _manager.GetMaxQuantityPerItem)
            _item.ChangeCount(_manager.GetMaxQuantityPerItem);
        else
        _item.ChangeCount(count);
        
        updateCountTVs();
        _manager.OnCartItemDataChange(this);

    }
    private void updateCountTVs()
    {
        _countTV.SetText(_item.Count.ToString());
        _priceTV.SetText(_item.PriceMultiple.ToString());
    }

    public void IncreaseCountButton()
    {
        UpdateData(_item.Count + 1);
    }

    public void DecreaseCountButton()
    {
        UpdateData(_item.Count - 1);

    }
}
