using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

public class ShopOriginalSceneSmokeTests
{
    [UnitySetUp]
    public IEnumerator OpenOriginal()
    {
        EditorSceneManager.OpenScene("Assets/ShopSystem/Scenes/ShopSystemSampleScene.unity");
        yield return new EnterPlayMode();
    }

    [UnityTest]
    public IEnumerator OriginalSceneStillOpensItsOriginalShop()
    {
        var debug = Object.FindFirstObjectByType<ShopDebug>();
        Assert.IsNotNull(debug);
        var manager = (ShopManager)typeof(ShopDebug).GetField("_shopManager", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(debug);
        Assert.IsNotNull(manager);
        Assert.IsNull(manager.transform.Find("IllustratedCream"));
        debug.openShop();
        yield return null;
        yield return null;
        Assert.IsTrue(manager.gameObject.activeInHierarchy);
        ScreenCapture.CaptureScreenshot("output/shop-ui/original-shop-restored.png");
        yield return null;
        yield return null;
        manager.closeShop();
        Assert.IsFalse(manager.gameObject.activeSelf);
    }

    [UnityTearDown]
    public IEnumerator StopOriginal()
    {
        yield return new ExitPlayMode();
        EditorSceneManager.OpenScene(IllustratedCreamShopBuilder.ScenePath);
    }
}
