using UnityEngine;

public class ShopExample : MonoBehaviour
{
    [SerializeField] private ShopManager _shopManager;
    [SerializeField] private SOShopItemExample[] _shopItems;


    public void OpenShop(IPurchaseHandler purchaseHandler)
    {
        _shopManager.OpenShop(_shopItems, purchaseHandler);
    }

}
