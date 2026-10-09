using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

internal static class ShopSkinTestPaths
{
    public const string Art = "Assets/ShopSystem/Sprites/IllustratedCream/PNG/";
    public const string Parts = "Assets/ShopSystem/Prefabs/IllustratedCream/Variants/";
    public const string ScenePath = "Assets/ShopSystem/Scenes/ShopSystemIllustratedCream.unity";
}

public class ShopSkinAssetTests
{
    [TestCase("ShopManager", "ShopManager")]
    [TestCase("Canvas", "Canvas")]
    [TestCase("ItemSlot", "comp/ItemSlot")]
    [TestCase("CartItemSlot", "comp/CartItemSlot")]
    [TestCase("TagButton", "comp/TagButton")]
    public void SkinRetainsOriginalPrefabParent(string name, string parent)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ShopSkinTestPaths.Parts + name + ".prefab");
        Assert.AreEqual(PrefabAssetType.Variant, PrefabUtility.GetPrefabAssetType(asset));
        Assert.AreEqual("Assets/ShopSystem/Prefabs/" + parent + ".prefab", AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(asset)));
    }

    [Test]
    public void AllArtworkHasSeparateSpriteAssetsAndSlicing()
    {
        foreach (var path in Directory.GetFiles(ShopSkinTestPaths.Art, "*.png"))
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<Sprite>(path.Replace('\\', '/')), path);
        foreach (var name in new[] { "window-frame", "cart-panel", "item-card", "cart-row", "button-add", "button-purchase", "category-tab" })
            Assert.Greater(AssetDatabase.LoadAssetAtPath<Sprite>(ShopSkinTestPaths.Art + name + ".png").border.x, 0, name);
        Assert.IsTrue(File.Exists(ShopSkinTestPaths.ScenePath));
    }
}
