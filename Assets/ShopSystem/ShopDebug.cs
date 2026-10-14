using UnityEngine;

public class ShopDebug : MonoBehaviour
{
    [SerializeField] private ShopManager _shopManager;
    [SerializeField] private SOShopItemExample[] _shopItems;
    [SerializeField] private PlayerDebug _player;

    public void openShop()
    {
        IPurchaseHandler ph = _player;
        _shopManager.openShop(_shopItems,ph);
    }
}
