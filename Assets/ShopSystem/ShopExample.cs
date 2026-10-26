using UnityEngine;

public class ShopExample : MonoBehaviour
{
    [SerializeField] private ShopManager _shopManager;
    [SerializeField] private SOShopItemExample[] _shopItems;


    public void openShop(IPurchaseHandler purchaseHandler)
    {
        _shopManager.openShop(_shopItems, purchaseHandler);
    }

}
