using UnityEngine;

public class OpenShopButtonDebug : MonoBehaviour
{
    [SerializeField] private PlayerDebug _player;
    [SerializeField] private ShopExample _shopExample;

    //communication should go from the player to the shop NPC, f.e. through an interaction system.
    //for demonstration purposes, this class handles it and serves as man in the middle.
    public void OpenShopDebug()
    {
        IPurchaseHandler ph = _player;
        _shopExample.OpenShop(ph);
    }
}
