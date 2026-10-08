using TMPro;
using UnityEngine;

public class ShopCartItemSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _countTV;
    [SerializeField] private TextMeshProUGUI _nameTV;
    [SerializeField] private TextMeshProUGUI _priceTV;

    private ShopItem _item;
    public ShopItem Item => _item;

    private ShopManager _manager;


    public void init(ShopItem item, ShopManager manager)
    {
        _item = new ShopItem(item);
        _manager = manager;
        _nameTV.SetText(item.ItemSO.Name);
        updateCountTVs();
        _manager.OnCartItemDataChange(this);
    }

    public void updateData(int count)
    {
        if(count > _manager.GetMaxQuantityPerItem)
            _item.changeCount(_manager.GetMaxQuantityPerItem);
        else
        _item.changeCount(count);
        
        updateCountTVs();
        _manager.OnCartItemDataChange(this);

    }
    private void updateCountTVs()
    {
        _countTV.SetText(_item.Count.ToString());
        _priceTV.SetText(_item.PriceMultiple.ToString());
    }

    public void increaseCountButton()
    {
        updateData(_item.Count + 1);
    }

    public void decreaseCountButton()
    {
        updateData(_item.Count - 1);

    }


}
