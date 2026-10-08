using UnityEngine;

public class ShopDebug : MonoBehaviour
{
    [SerializeField] private ShopManager _shopManager;
    [SerializeField] private SOShopItemExample[] _shopItems;

    public void openShop()
    {
        _shopManager.openShop(_shopItems);
    }
}
