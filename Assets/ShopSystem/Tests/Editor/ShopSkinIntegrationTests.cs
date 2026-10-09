using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class ShopSkinIntegrationTests
{
    private GameObject root;
    private sealed class Purchase : IShopPurchaseHandler
    {
        public int calls;
        public bool TryPurchase(inventorySlotData[] items, int price) { calls++; return true; }
        public void onPurchase(inventorySlotData[] items, int price) { }
    }

    private static T Field<T>(object target, string name) => (T)target.GetType()
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);

    [UnitySetUp]
    public IEnumerator EnterRuntime()
    {
        yield return new EnterPlayMode();
    }

    [UnityTest]
    public IEnumerator IllustratedPrefabSupportsItsVisibleControls()
    {
        root = new GameObject("Shop skin integration canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = .5f;
        var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(IllustratedCreamShopBuilder.Parts + "ShopManager.prefab"), root.transform);
        var manager = instance.GetComponent<ShopManager>();
        var stock = new[] { "Sword", "BattleAxe", "BluePotion", "Bread", "Pickaxe", "BrassKey" }
            .Select(name => AssetDatabase.LoadAssetAtPath<SOShopItemExample>("Assets/ShopSystem/SO_ittems/SO_ShopItemExample/" + name + ".asset")).ToArray();
        Assert.That(stock.All(item => item != null));
        bool canBuy = true;
        manager.OnCanPurchaseMoneyEvent += _ => canBuy;
        manager.OnCanPurchaseInventorySlotsEvent += _ => true;
        var purchase = new Purchase();
        manager.RegisterPurchaseHandler(purchase);
        manager.openShop(stock);
        yield return null;
        yield return null;

        var slots = Field<ShopItemSlot[]>(manager, "_itemSlots");
        Assert.AreEqual(6, slots.Length);
        var quantity = slots[0].GetComponentInChildren<TMP_InputField>();
        slots[0].transform.Find("QuantityUp").GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual("2", quantity.text);
        slots[0].transform.Find("QuantityDown").GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual("1", quantity.text);
        quantity.text = "2";
        slots[0].transform.Find("Add").GetComponent<Button>().onClick.Invoke();
        var cart = Field<List<ShopCartItemSlot>>(manager, "_cartItemsSlots");
        Assert.AreEqual(2, cart[0].Item.Count);
        cart[0].transform.Find("Plus").GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual(3, cart[0].Item.Count);
        cart[0].transform.Find("Minus").GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual(2, cart[0].Item.Count);
        slots[2].transform.Find("Add").GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual(2 * stock[0].Value + stock[2].Value, Field<int>(manager, "_cartPrice"));
        cart[1].transform.Find("Remove").GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual(1, cart.Count);
        Assert.AreEqual(2 * stock[0].Value, Field<int>(manager, "_cartPrice"));
        yield return null;
        slots[2].transform.Find("Add").GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual(2, cart.Count);
        Assert.IsNotNull(slots[0].transform.Find("PriceCoin").GetComponent<Image>().sprite);
        Assert.IsNotNull(cart[0].transform.Find("UnitCoin").GetComponent<Image>().sprite);

        var tags = Field<Transform>(manager, "_tagViewContent").GetComponentsInChildren<ShopTagButton>();
        var weaponTag = tags.First(tag => tag.GetComponentInChildren<TMP_Text>().text == stock[0].Type.ToString());
        weaponTag.GetComponent<Button>().onClick.Invoke();
        Assert.That(slots.All(slot => slot.gameObject.activeSelf == (slot.ItemData.Type == stock[0].Type)));
        tags.First(tag => tag.GetComponentInChildren<TMP_Text>().text == "All").GetComponent<Button>().onClick.Invoke();
        Assert.That(slots.All(slot => slot.gameObject.activeSelf));
        Assert.IsTrue(instance.GetComponent<Canvas>().overrideSorting);
        Assert.That(instance.GetComponentsInChildren<TMP_Text>(true).All(label => !label.text.Contains("adventure starts")));

        var design = (RectTransform)instance.transform.Find("IllustratedCream");
        var shopRect = (RectTransform)instance.transform;
        shopRect.anchorMin = shopRect.anchorMax = new Vector2(.5f, .5f);
        shopRect.pivot = new Vector2(.5f, .5f);
        shopRect.anchoredPosition = Vector2.zero;
        foreach (var size in new[] { new Vector2(1024, 768), new Vector2(2560, 1080), new Vector2(1920, 1080) })
        {
            shopRect.sizeDelta = size;
            Canvas.ForceUpdateCanvases();
            yield return null;
            yield return null;
            Assert.LessOrEqual(design.rect.width * design.localScale.x, size.x + .1f);
            Assert.LessOrEqual(design.rect.height * design.localScale.y, size.y + .1f);
            Assert.AreEqual(design.localScale.x, design.localScale.y, .0001f);
        }
        shopRect.anchorMin = Vector2.zero;
        shopRect.anchorMax = Vector2.one;
        shopRect.offsetMin = shopRect.offsetMax = Vector2.zero;
        yield return null;
        yield return null;
        Directory.CreateDirectory("output/shop-ui");
        ScreenCapture.CaptureScreenshot("output/shop-ui/illustrated-cream-variants-in-unity.png");
        yield return null;
        yield return null;

        var buy = design.Find("Purchase").GetComponent<Button>();
        canBuy = false;
        buy.onClick.Invoke();
        Assert.AreEqual(0, purchase.calls);
        Assert.AreEqual(2, cart.Count);
        Assert.IsTrue(Field<TMP_Text>(manager, "_cartErrorBuyTV").gameObject.activeSelf);
        canBuy = true;
        buy.onClick.Invoke();
        Assert.AreEqual(1, purchase.calls);
        Assert.IsEmpty(cart);
        yield return null;
        yield return null;
        design.Find("Close").GetComponent<Button>().onClick.Invoke();
        Assert.IsFalse(instance.activeSelf);
        manager.openShop(stock);
        yield return null;
        yield return null;
        Assert.IsTrue(instance.activeSelf);
        Assert.IsEmpty(cart);
        Field<ShopItemSlot[]>(manager, "_itemSlots")[0].transform.Find("Add").GetComponent<Button>().onClick.Invoke();
        Assert.AreEqual(1, cart.Count);
    }

    [UnityTearDown]
    public IEnumerator Cleanup()
    {
        if (root != null) Object.Destroy(root);
        yield return null;
        yield return null;
        yield return new ExitPlayMode();
    }
}
