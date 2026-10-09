using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ShopSkinSceneIntegrationTests
{
    private static T Field<T>(object target, string name) => (T)target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);

    [UnitySetUp]
    public IEnumerator OpenDemo()
    {
        EditorSceneManager.OpenScene(IllustratedCreamShopBuilder.ScenePath);
        yield return new EnterPlayMode();
    }

    [UnityTest]
    public IEnumerator CopiedSceneUsesVariantAndOriginalPurchaseBindings()
    {
        var debug = Object.FindFirstObjectByType<ShopDebug>();
        var player = Object.FindFirstObjectByType<PlayerDebug>();
        Assert.IsNotNull(debug);
        Assert.IsNotNull(player);
        var manager = Field<ShopManager>(debug, "_shopManager");
        Assert.AreSame(manager, Field<ShopManager>(player, "_shopManager"));
        Assert.IsNotNull(manager.transform.Find("IllustratedCream"));
        player.setEnoughMoney(true);
        player.setEnougInventorySlots(true);
        debug.openShop();
        yield return null;
        yield return null;
        var slots = Field<ShopItemSlot[]>(manager, "_itemSlots");
        Assert.Greater(slots.Length, 6);
        slots[0].transform.Find("Add").GetComponent<Button>().onClick.Invoke();
        slots[2].transform.Find("Add").GetComponent<Button>().onClick.Invoke();
        var cart = Field<List<ShopCartItemSlot>>(manager, "_cartItemsSlots");
        Assert.AreEqual(2, cart.Count);
        yield return null;
        yield return null;
        Directory.CreateDirectory("output/shop-ui");
        ScreenCapture.CaptureScreenshot("output/shop-ui/illustrated-cream-scene.png");
        yield return null;
        yield return null;
        manager.transform.Find("IllustratedCream/Purchase").GetComponent<Button>().onClick.Invoke();
        Assert.IsEmpty(cart);
        manager.closeShop();
        Assert.IsFalse(manager.gameObject.activeSelf);
        debug.openShop();
        yield return null;
        Assert.IsTrue(manager.gameObject.activeSelf);
        Assert.IsEmpty(cart);
    }

    [UnityTearDown]
    public IEnumerator StopDemo() { yield return new ExitPlayMode(); }
}
