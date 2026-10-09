using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SharedShopExampleSetup
{
    public const string CanvasPath = "Assets/ShopSystem/Prefabs/CanvasDebug.prefab";
    public const string ExamplePath = "Assets/ShopSystem/Prefabs/ShopExample.prefab";
    public static readonly string[] Scenes = {
        "Assets/ShopSystem/Scenes/ShopSystemSampleScene.unity",
        "Assets/ShopSystem/Scenes/ShopSystemIllustratedCream.unity"
    };

    [MenuItem("Tools/Shop/Share Example Prefabs Between Scenes")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
        if (Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
            throw new InvalidOperationException("Save open scene changes first.");
        var previous = EditorSceneManager.GetSceneManagerSetup();
        try
        {
            foreach (string path in Scenes)
            {
                var scene = EditorSceneManager.OpenScene(path);
                var canvas = scene.GetRootGameObjects().Single(go => go.name == "CanvasDebug");
                var debug = canvas.GetComponentInChildren<ShopDebug>(true);
                var player = canvas.GetComponentInChildren<PlayerDebug>(true);
                var manager = Manager(debug);
                var playerManager = Manager(player);
                if (manager == null || playerManager == null) throw new InvalidOperationException("Missing shop reference in " + path);

                // Scene references are overrides. Shared assets must not reference a scene object.
                Bind(debug, null);
                Bind(player, null);
                Connect(debug.gameObject, ExamplePath);
                Connect(canvas, CanvasPath);
                debug = canvas.GetComponentInChildren<ShopDebug>(true);
                player = canvas.GetComponentInChildren<PlayerDebug>(true);
                Bind(debug, manager);
                Bind(player, playerManager);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            Verify();
        }
        finally { EditorSceneManager.RestoreSceneManagerSetup(previous); }
    }

    private static void Connect(GameObject obj, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
            PrefabUtility.SaveAsPrefabAssetAndConnect(obj, path, InteractionMode.AutomatedAction);
        else if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj) != path)
            PrefabUtility.ConvertToPrefabInstance(obj, asset, new ConvertToPrefabInstanceSettings {
                objectMatchMode = ObjectMatchMode.ByHierarchy,
                recordPropertyOverridesOfMatches = true,
                gameObjectsNotMatchedBecomesOverride = true,
                componentsNotMatchedBecomesOverride = true,
                changeRootNameToAssetName = false
            }, InteractionMode.AutomatedAction);
    }

    public static void Verify()
    {
        foreach (string path in Scenes)
        {
            var scene = EditorSceneManager.OpenScene(path);
            var canvas = scene.GetRootGameObjects().Single(go => go.name == "CanvasDebug");
            var debug = canvas.GetComponentInChildren<ShopDebug>(true);
            var player = canvas.GetComponentInChildren<PlayerDebug>(true);
            if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(canvas) != CanvasPath ||
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(debug.gameObject) != ExamplePath)
                throw new InvalidOperationException("Shared prefab identity mismatch in " + path);
            if (Manager(debug) == null || Manager(debug) != Manager(player))
                throw new InvalidOperationException("Scene shop binding mismatch in " + path);
            bool illustrated = Manager(debug).transform.Find("IllustratedCream") != null;
            if (illustrated != path.Contains("IllustratedCream")) throw new InvalidOperationException("Wrong shop style in " + path);
            var button = debug.GetComponent<UnityEngine.UI.Button>();
            if (button.onClick.GetPersistentTarget(0) != debug || button.onClick.GetPersistentMethodName(0) != "openShop")
                throw new InvalidOperationException("OpenShop binding changed in " + path);
        }
        Directory.CreateDirectory("output/shop-ui");
        File.WriteAllText("output/shop-ui/shared-prefab-validation.txt", "PASS: Both scenes use the same CanvasDebug and nested ShopExample prefab assets.\nPASS: ShopDebug, PlayerDebug, and OpenShop button bindings are preserved.\nPASS: Original and illustrated shop appearances remain separate.\n");
    }

    private static ShopManager Manager(MonoBehaviour owner) => (ShopManager)new SerializedObject(owner).FindProperty("_shopManager").objectReferenceValue;
    private static void Bind(MonoBehaviour owner, ShopManager manager)
    {
        var so = new SerializedObject(owner);
        so.FindProperty("_shopManager").objectReferenceValue = manager;
        so.ApplyModifiedPropertiesWithoutUndo();
        if (PrefabUtility.IsPartOfPrefabInstance(owner)) PrefabUtility.RecordPrefabInstancePropertyModifications(owner);
    }
}
