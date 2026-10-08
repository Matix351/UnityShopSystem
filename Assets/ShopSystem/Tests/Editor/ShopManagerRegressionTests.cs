using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class UntypedShopTestItem : SOItemData, IItemTradeable
{
    public int Value => 10;
}

public class ShopManagerRegressionTests
{
    private sealed class TestPurchaseHandler : IShopPurchaseHandler
    {
        private readonly Func<inventorySlotData[], int, bool> _purchase;
        public TestPurchaseHandler(Func<inventorySlotData[], int, bool> purchase) => _purchase = purchase;
        public bool TryPurchase(inventorySlotData[] items, int totalPrice) => _purchase(items, totalPrice);
    }

    private GameObject _root;
    private ShopManager _manager;
    private SOShopItemExample _item;
    private readonly List<ScriptableObject> _testItems = new List<ScriptableObject>();

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();
        _root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/ShopSystem/Prefabs/ShopManager.prefab"));
        _manager = _root.GetComponentInChildren<ShopManager>(true);
        _item = CreateItem(200);
        _manager.openShop(new SOItemData[] { _item });
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(_root);
        foreach (ScriptableObject item in _testItems)
            Object.Destroy(item);
        _testItems.Clear();
        yield return null;
        yield return new ExitPlayMode();
    }

    private SOShopItemExample CreateItem(int price)
    {
        SOShopItemExample item = ScriptableObject.CreateInstance<SOShopItemExample>();
        SetField(item, "_value", price);
        _testItems.Add(item);
        return item;
    }

    private static void SetField(object target, string name, object value)
    {
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    private static T GetField<T>(object target, string name)
    {
        return (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(target);
    }

    private ShopItem MakeItem(SOShopItemExample item, int count = 1)
    {
        return new ShopItem(item, item, item.Type, count);
    }

    private List<ShopCartItemSlot> Cart => GetField<List<ShopCartItemSlot>>(_manager, "_cartItemsSlots");

    [UnityTest]
    public IEnumerator CheckoutHasExactlyOneTransactionOwner()
    {
        int calls = 0;
        var handler = new TestPurchaseHandler((items, price) => { calls++; return true; });
        var other = new TestPurchaseHandler((items, price) => throw new InvalidOperationException("Wrong handler called"));
        _manager.RegisterPurchaseHandler(handler);
        Assert.DoesNotThrow(() => _manager.RegisterPurchaseHandler(handler));
        Assert.Throws<InvalidOperationException>(() => _manager.RegisterPurchaseHandler(other));
        _manager.UnregisterPurchaseHandler(other);
        _manager.addToCart(MakeItem(_item));
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(1, calls);
        _manager.UnregisterPurchaseHandler(handler);
        Assert.DoesNotThrow(() => _manager.RegisterPurchaseHandler(other));
        yield return null;
    }

    [UnityTest]
    public IEnumerator CheckoutNotifiesAllListenersOnlyAfterSuccess()
    {
        bool success = false;
        int notifications = 0;
        _manager.RegisterPurchaseHandler(new TestPurchaseHandler((items, price) => success));
        _manager.OnPurchaseCompleted += (items, price) =>
        {
            Assert.IsEmpty(Cart);
            Assert.AreEqual(400, price);
            Assert.AreEqual(2, items[0].ammount);
            notifications++;
        };
        _manager.OnPurchaseCompleted += (items, price) => notifications++;
        _manager.addToCart(MakeItem(_item, 2));
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(0, notifications);
        success = true;
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(2, notifications);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CheckoutPreventsReentrantPurchases()
    {
        int calls = 0;
        _manager.RegisterPurchaseHandler(new TestPurchaseHandler((items, price) =>
        {
            calls++;
            _manager.onPurchaseButtonClick();
            return true;
        }));
        _manager.OnPurchaseCompleted += (items, price) =>
        {
            _manager.addToCart(MakeItem(_item));
            _manager.onPurchaseButtonClick();
        };
        _manager.addToCart(MakeItem(_item));
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(1, calls);
        Assert.AreEqual(1, Cart.Count);
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(2, calls);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CheckoutListenerExceptionDoesNotRepeatTransactionOrBlockOtherListeners()
    {
        int calls = 0;
        int notifications = 0;
        _manager.RegisterPurchaseHandler(new TestPurchaseHandler((items, price) => { calls++; return true; }));
        _manager.OnPurchaseCompleted += (items, price) => throw new InvalidOperationException("Test listener failure");
        _manager.OnPurchaseCompleted += (items, price) => notifications++;
        _manager.addToCart(MakeItem(_item));
        LogAssert.Expect(LogType.Exception, "InvalidOperationException: Test listener failure");
        _manager.onPurchaseButtonClick();
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(1, calls);
        Assert.AreEqual(1, notifications);
        Assert.IsEmpty(Cart);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CheckoutRejectsEmptyAndClearedCarts()
    {
        int purchases = 0;
        _manager.RegisterPurchaseHandler(new TestPurchaseHandler((items, price) => { purchases++; return true; }));
        Assert.IsFalse(_manager.checkCanPlayerPurchaseConditions());
        _manager.onPurchaseButtonClick();
        _manager.addToCart(MakeItem(_item));
        _manager.clearCart();
        _manager.onPurchaseButtonClick();
        _manager.addToCart(MakeItem(_item));
        Cart[0].updateData(0);
        Assert.IsFalse(_manager.checkCanPlayerPurchaseConditions());
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(0, purchases);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CheckoutRechecksBothConditionsAndAllowsRetryAfterRecovery()
    {
        bool hasMoney = true;
        bool hasSpace = true;
        int purchases = 0;
        _manager.OnCanPurchaseMoneyEvent += price => hasMoney;
        _manager.OnCanPurchaseInventorySlotsEvent += items => hasSpace;
        _manager.RegisterPurchaseHandler(new TestPurchaseHandler((items, price) =>
        {
            purchases++;
            Assert.AreEqual(400, price);
            Assert.AreEqual(2, items[0].ammount);
            return true;
        }));
        _manager.addToCart(MakeItem(_item, 2));
        hasMoney = false;
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(0, purchases);
        Assert.AreEqual(2, Cart[0].Item.Count);
        Assert.IsTrue(GetField<TextMeshProUGUI>(_manager, "_cartErrorBuyTV").gameObject.activeSelf);
        hasMoney = true;
        hasSpace = false;
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(0, purchases);
        Assert.AreEqual(400, GetField<int>(_manager, "_cartPrice"));
        hasSpace = true;
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(1, purchases);
        Assert.IsEmpty(Cart);
        Assert.AreEqual(0, GetField<int>(_manager, "_cartPrice"));
        Assert.IsFalse(GetField<bool>(_manager, "_canPlayerPurchase"));
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(1, purchases);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CheckoutFailureKeepsCartAndDisplaysError()
    {
        _manager.RegisterPurchaseHandler(new TestPurchaseHandler((items, price) => false));
        _manager.addToCart(MakeItem(_item, 2));
        _manager.onPurchaseButtonClick();
        Assert.AreEqual(2, Cart[0].Item.Count);
        Assert.AreEqual(400, GetField<int>(_manager, "_cartPrice"));
        var error = GetField<TextMeshProUGUI>(_manager, "_cartErrorBuyTV");
        Assert.IsTrue(error.gameObject.activeSelf);
        StringAssert.Contains("Purchase failed", error.text);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CheckoutWithoutPurchaseHandlerKeepsCart()
    {
        _manager.addToCart(MakeItem(_item));
        Assert.DoesNotThrow(() => _manager.onPurchaseButtonClick());
        Assert.AreEqual(1, Cart.Count);
        Assert.IsTrue(GetField<TextMeshProUGUI>(_manager, "_cartErrorBuyTV").gameObject.activeSelf);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CloseWithoutCallbackAndRepeatedCloseAreSafe()
    {
        Assert.DoesNotThrow(() => _manager.closeShop());
        Assert.DoesNotThrow(() => _manager.closeShop());
        yield return null;
    }

    [UnityTest]
    public IEnumerator ClosingCallbackCanReopenShopWithANewCallback()
    {
        int first = 0;
        int second = 0;
        _manager.openShop(new SOItemData[] { _item }, () =>
        {
            first++;
            _manager.openShop(new SOItemData[] { _item }, () => second++);
        });
        _manager.closeShop();
        _manager.closeShop();
        _manager.closeShop();
        Assert.AreEqual(1, first);
        Assert.AreEqual(1, second);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ReopeningClearsCartAndAllowsAddingAgain()
    {
        _manager.addToCart(MakeItem(_item, 3));
        ShopCartItemSlot oldSlot = Cart[0];
        _manager.openShop(new SOItemData[] { _item });
        Assert.IsEmpty(Cart);
        Assert.AreEqual(0, GetField<int>(_manager, "_cartPrice"));
        Assert.IsFalse(oldSlot.gameObject.activeSelf);
        yield return null;
        Assert.IsTrue(oldSlot == null);
        _manager.addToCart(MakeItem(_item, 2));
        Assert.AreEqual(1, Cart.Count);
        Assert.AreEqual(400, GetField<int>(_manager, "_cartPrice"));
    }

    [UnityTest]
    public IEnumerator TradeableWithoutCategoryUsesOther()
    {
        var item = ScriptableObject.CreateInstance<UntypedShopTestItem>();
        _testItems.Add(item);
        _manager.openShop(new SOItemData[] { item });
        Assert.AreEqual(ITEM_TYPE.Other, GetField<ShopItemSlot[]>(_manager, "_itemSlots")[0].ItemData.Type);
        _manager.OnTagClick(ITEM_TYPE.Other);
        yield return null;
    }

    [UnityTest]
    public IEnumerator QuantityLimitAppliesToTypingRepeatedAddsAndCartButtons()
    {
        SetField(_manager, "_maxQuantityPerItem", 5);
        ShopItemSlot slot = GetField<ShopItemSlot[]>(_manager, "_itemSlots")[0];
        GetField<TMP_InputField>(slot, "_inputAmmountTV").text = int.MaxValue.ToString();
        slot.AddToCartMultipleItems();
        Assert.AreEqual(5, Cart[0].Item.Count);
        slot.AddToCartOneItem();
        Cart[0].increaseCountButton();
        Assert.AreEqual(5, Cart[0].Item.Count);
        Cart[0].decreaseCountButton();
        Assert.AreEqual(4, Cart[0].Item.Count);
        _manager.addToCart(MakeItem(_item, 3));
        Assert.AreEqual(5, Cart[0].Item.Count);
        Cart[0].updateData(0);
        Assert.IsEmpty(Cart);
        yield return null;
    }

    [UnityTest]
    public IEnumerator CartTotalAndQuantityAdditionCannotWrap()
    {
        SetField(_manager, "_maxQuantityPerItem", int.MaxValue);
        var expensive = CreateItem(int.MaxValue - 100);
        _manager.addToCart(MakeItem(expensive));
        _manager.addToCart(MakeItem(_item));
        Assert.AreEqual(1, Cart.Count, "A second row must not overflow the cart total.");
        var cheap = CreateItem(10);
        _manager.addToCart(MakeItem(cheap, 100));
        Assert.AreEqual(10, Cart[1].Item.Count);
        Assert.AreEqual(int.MaxValue, GetField<int>(_manager, "_cartPrice"));
        _manager.clearCart();
        var free = CreateItem(0);
        _manager.addToCart(MakeItem(free, int.MaxValue));
        _manager.addToCart(MakeItem(free, int.MaxValue));
        Cart[0].increaseCountButton();
        Assert.AreEqual(int.MaxValue, Cart[0].Item.Count);
        Assert.AreEqual(0, Cart[0].Item.PriceMultiple);
        yield return null;
    }

    [UnityTest]
    public IEnumerator InvalidInspectorLimitStillAllowsOnlyOne()
    {
        SetField(_manager, "_maxQuantityPerItem", 0);
        _manager.addToCart(MakeItem(_item, 3));
        Assert.AreEqual(1, Cart[0].Item.Count);
        yield return null;
    }

    [UnityTest]
    public IEnumerator ModelRejectsInvalidCountsWithoutCorruptingExistingState()
    {
        ShopItem item = MakeItem(_item, 2);
        Assert.Throws<OverflowException>(() => item.changeCount(int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => item.changeCount(-1));
        Assert.AreEqual(2, item.Count);
        Assert.AreEqual(400, item.PriceMultiple);
        yield return null;
    }
}
