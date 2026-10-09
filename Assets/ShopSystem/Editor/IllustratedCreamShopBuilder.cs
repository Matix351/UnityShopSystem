using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Creates derived assets only. Original prefabs and scene are never saved by this builder.</summary>
public static class IllustratedCreamShopBuilder
{
    public const string Art = "Assets/ShopSystem/Art/IllustratedCream/PNG/";
    public const string Parts = "Assets/ShopSystem/Prefabs/IllustratedCream/Variants/";
    public const string ScenePath = "Assets/ShopSystem/Scenes/ShopSystemIllustratedCream.unity";
    private const string Original = "Assets/ShopSystem/Prefabs/";
    private static readonly Color Ink = new Color(.16f, .22f, .15f);
    private static readonly Color Cream = new Color(1, .97f, .87f);
    private static Scene staging;

    [MenuItem("Tools/Shop/Build Illustrated Cream Variants")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode first.");
        Directory.CreateDirectory(Parts);
        foreach (var path in Directory.GetFiles(Art, "*.png")) ImportSprite(path.Replace('\\', '/'));
        staging = EditorSceneManager.NewPreviewScene();
        try
        {
            var item = BuildItem();
            var cart = BuildCart();
            var tag = BuildTag();
            BuildShop(item, cart, tag);
            BuildStandaloneControls();
        }
        finally { EditorSceneManager.ClosePreviewScene(staging); }
        AssetDatabase.SaveAssets();
        if (!File.Exists(ScenePath)) CreateScene();
        Debug.Log("Illustrated Cream prefab variants built. Original assets are unchanged.");
    }

    private static RectTransform Base(string path, float w, float h)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Original + path + ".prefab");
        if (asset == null) throw new InvalidOperationException("Missing original prefab: " + path);
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(asset, staging);
        // Inherited objects are retained as disabled overrides, never deleted or unpacked.
        foreach (Transform child in obj.transform) child.gameObject.SetActive(false);
        foreach (var layout in obj.GetComponents<LayoutGroup>()) layout.enabled = false;
        foreach (var fit in obj.GetComponents<ContentSizeFitter>()) fit.enabled = false;
        obj.SetActive(true);
        var rect = (RectTransform)obj.transform;
        Place(rect, 0, 0, w, h);
        return rect;
    }

    private static T SaveVariant<T>(RectTransform root, string name) where T : Component
    {
        foreach (var component in root.GetComponentsInChildren<Component>(true))
            if (component != null && PrefabUtility.IsPartOfPrefabInstance(component))
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            if (PrefabUtility.IsPartOfPrefabInstance(transform.gameObject))
                PrefabUtility.RecordPrefabInstancePropertyModifications(transform.gameObject);
        var asset = PrefabUtility.SaveAsPrefabAsset(root.gameObject, Parts + name + ".prefab");
        if (PrefabUtility.GetPrefabAssetType(asset) != PrefabAssetType.Variant)
            throw new InvalidOperationException(name + " must be a real Prefab Variant.");
        Object.DestroyImmediate(root.gameObject);
        return asset.GetComponent<T>();
    }

    private static void BuildShop(ShopItemSlot item, ShopCartItemSlot cart, ShopTagButton tag)
    {
        var root = Base("ShopManager", 1920, 1080);
        var manager = root.GetComponent<ShopManager>();
        Stretch(root);
        var canvas = root.GetComponent<Canvas>();
        if (canvas == null) canvas = root.gameObject.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 10;
        var canvasSettings = new SerializedObject(canvas);
        canvasSettings.FindProperty("m_OverrideSorting").boolValue = true;
        canvasSettings.ApplyModifiedPropertiesWithoutUndo();
        if (root.GetComponent<GraphicRaycaster>() == null) root.gameObject.AddComponent<GraphicRaycaster>();
        var shade = root.GetComponent<Image>();
        if (shade == null) shade = root.gameObject.AddComponent<Image>();
        shade.sprite = null;
        shade.color = new Color(.14f, .18f, .13f);
        var design = Rect("IllustratedCream", root, 0, 0, 1920, 1080);
        design.anchorMin = design.anchorMax = design.pivot = new Vector2(.5f,.5f);
        design.anchoredPosition = Vector2.zero;
        design.gameObject.AddComponent<ShopDesignCanvas>();
        Picture("Village", design, "village-background", 0, 0, 1920, 1080);
        Picture("WindowFrame", design, "window-frame", 180, 78, 1560, 952, true, 6);
        Picture("CartPanel", design, "cart-panel", 1228, 149, 492, 850, true, 6);
        Picture("TitleSign", design, "title-sign", 610, 4, 700, 128);
        var title = Label("Title", design, "Shop", 745, 10, 430, 103, 72, Cream);
        title.fontMaterial = new Material(title.fontSharedMaterial);
        title.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, new Color(.27f,.16f,.09f));
        title.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, .18f);
        // Save the title material so the variant has no transient material references.
        AssetDatabase.CreateAsset(title.fontMaterial, Parts + "TitleOutline.mat");
        var close = Button("Close", design, "X", "button-close", 1653, 55, 82, 82, 42, Cream);
        UnityEventTools.AddPersistentListener(close.onClick, manager.closeShop);
        var categories = Scroll("Categories", design, 215, 151, 990, 68, out var tagContent, true);
        var tags = tagContent.gameObject.AddComponent<HorizontalLayoutGroup>();
        tags.spacing = 10; tags.childControlWidth = tags.childControlHeight = true;
        tags.childForceExpandWidth = tags.childForceExpandHeight = false;
        tagContent.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = Scroll("Items", design, 214, 233, 994, 752, out var itemContent);
        var grid = itemContent.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(316, 365); grid.spacing = new Vector2(16,16);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3;
        itemContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        AddScrollbar(scroll, design, 1201, 242, 10, 730);
        Picture("CartIcon", design, "cart-icon", 1252, 180, 70, 62);
        Label("CartHeading", design, "Your Cart", 1340, 173, 343, 80, 46, Ink, TextAlignmentOptions.Left);
        var cartScroll = Scroll("Cart", design, 1242, 273, 460, 400, out var cartContent);
        var rows = cartContent.gameObject.AddComponent<VerticalLayoutGroup>();
        rows.spacing = 13; rows.childControlWidth = rows.childControlHeight = true;
        rows.childForceExpandWidth = true; rows.childForceExpandHeight = false;
        cartContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        AddScrollbar(cartScroll, design, 1704, 280, 8, 390);
        var divider = Picture("CartDivider", design, "cart-panel", 1250, 701, 445, 2);
        divider.color = new Color(.5f,.57f,.39f,.6f);
        Label("TotalLabel", design, "Total", 1260, 729, 205, 76, 43, Ink, TextAlignmentOptions.Left);
        Picture("TotalCoin", design, "gold-coin", 1530, 742, 48, 48);
        var total = Label("TotalValue", design, "0", 1578, 730, 109, 76, 43, Ink, TextAlignmentOptions.Right);
        var buy = Button("Purchase", design, "Purchase", "button-purchase", 1246, 818, 452, 123, 44, Cream);
        Picture("LeftLeaf", buy.transform, "leaf-ornament", 34, 42, 41, 42);
        var leaf = Picture("RightLeaf", buy.transform, "leaf-ornament", 377, 42, 41, 42);
        leaf.rectTransform.localScale = new Vector3(-1,1,1);
        UnityEventTools.AddPersistentListener(buy.onClick, manager.onPurchaseButtonClick);
        var error = Label("PurchaseError", design, "", 1260, 947, 423, 40, 19, new Color(.65f,.18f,.12f));
        error.gameObject.SetActive(false);
        Set(manager, "_shopItemSlotPrefab", item); Set(manager, "_cartItemSlotPrefab", cart);
        Set(manager, "_tagButtonPrefab", tag); Set(manager, "_itemsSlotViewContent", itemContent);
        Set(manager, "_tagViewContent", tagContent); Set(manager, "_cartItemSlotViewContent", cartContent);
        Set(manager, "_cartPriceTV", total); Set(manager, "_cartErrorBuyTV", error);
        root.gameObject.SetActive(false);
        SaveVariant<ShopManager>(root, "ShopManager");
    }

    private static ShopItemSlot BuildItem()
    {
        var root = Base("comp/ItemSlot", 316, 365);
        Skin(root, "item-card", true, 6);
        var slot = root.GetComponent<ShopItemSlot>();
        var icon = Picture("Icon", root, null, 35, 12, 246, 214);
        var name = Label("Name", root, "Item", 18, 222, 280, 38, 26, Ink);
        Picture("PriceCoin", root, "gold-coin", 115, 263, 31, 31);
        var price = Label("Price", root, "0", 151, 258, 100, 39, 25, Ink, TextAlignmentOptions.Left);
        var inputRect = Rect("Quantity", root, 16, 307, 71, 45);
        var inputImage = Skin(inputRect, "quantity-field", true, 6);
        var input = inputRect.gameObject.AddComponent<TMP_InputField>();
        var textRect = Rect("TextArea", inputRect, 5, 2, 60, 40);
        textRect.gameObject.AddComponent<RectMask2D>();
        var inputText = Label("Text", textRect, "1", 0, 0, 60, 40, 24, Ink);
        input.textViewport = textRect; input.textComponent = inputText; input.targetGraphic = inputImage;
        input.contentType = TMP_InputField.ContentType.IntegerNumber; input.characterLimit = 3; input.text = "1";
        var up = Button("QuantityUp", root, "+", "button-quantity", 89, 306, 35, 24, 22, Ink);
        var down = Button("QuantityDown", root, "-", "button-quantity", 89, 331, 35, 24, 22, Ink);
        UnityEventTools.AddPersistentListener(up.onClick, slot.IncreaseSelectedQuantity);
        UnityEventTools.AddPersistentListener(down.onClick, slot.DecreaseSelectedQuantity);
        var add = Button("Add", root, "Add", "button-add", 137, 306, 163, 49, 26, Cream);
        UnityEventTools.AddPersistentListener(add.onClick, slot.AddSelectedQuantity);
        Set(slot, "_iconImage", icon); Set(slot, "_nameTV", name); Set(slot, "_priceTV", price); Set(slot, "_inputAmmountTV", input);
        return SaveVariant<ShopItemSlot>(root, "ItemSlot");
    }

    private static ShopCartItemSlot BuildCart()
    {
        var root = Base("comp/CartItemSlot", 460, 137);
        Skin(root, "cart-row", true, 6);
        var element = root.GetComponent<LayoutElement>();
        if (element == null) element = root.gameObject.AddComponent<LayoutElement>();
        element.preferredHeight = 137;
        var slot = root.GetComponent<ShopCartItemSlot>();
        Picture("IconWell", root, "quantity-field", 12, 15, 95, 108, true, 7);
        var icon = Picture("Icon", root, null, 15, 18, 89, 102);
        var name = Label("Name", root, "Item", 120, 16, 122, 35, 24, Ink, TextAlignmentOptions.Left);
        Picture("UnitCoin", root, "gold-coin", 120, 80, 30, 30);
        var unit = Label("UnitPrice", root, "0", 157, 74, 95, 41, 23, Ink, TextAlignmentOptions.Left);
        Picture("CountWell", root, "quantity-field", 287, 23, 55, 43, true, 6);
        var count = Label("Count", root, "1", 287, 24, 55, 40, 24, Ink);
        var minus = Button("Minus", root, "-", "button-minus", 247, 23, 41, 43, 26, Cream);
        var plus = Button("Plus", root, "+", "button-plus", 342, 23, 41, 43, 26, Ink);
        UnityEventTools.AddPersistentListener(minus.onClick, slot.decreaseCountButton);
        UnityEventTools.AddPersistentListener(plus.onClick, slot.increaseCountButton);
        var remove = Button("Remove", root, "X", "button-remove", 411, 16, 33, 33, 20, new Color(.3f,.25f,.2f));
        UnityEventTools.AddIntPersistentListener(remove.onClick, slot.updateData, 0);
        var price = Label("Price", root, "0", 337, 79, 102, 38, 24, Ink, TextAlignmentOptions.Right);
        Set(slot, "_iconImage", icon); Set(slot, "_nameTV", name); Set(slot, "_countTV", count);
        Set(slot, "_priceTV", price); Set(slot, "_unitPriceTV", unit);
        return SaveVariant<ShopCartItemSlot>(root, "CartItemSlot");
    }

    private static ShopTagButton BuildTag()
    {
        var root = Base("comp/TagButton", 188, 65);
        var image = Skin(root, "category-tab", true, 6);
        var button = root.GetComponent<Button>(); button.targetGraphic = image;
        var layout = root.GetComponent<LayoutElement>();
        if (layout == null) layout = root.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = 188; layout.preferredHeight = 65;
        var tag = root.GetComponent<ShopTagButton>();
        var label = Label("Label", root, "All", 8, 5, 172, 55, 27, Ink);
        Set(tag, "_nametTV", label); Set(tag, "_background", image);
        // The original tag Button already calls onClick; preserve its persistent binding.
        return SaveVariant<ShopTagButton>(root, "TagButton");
    }

    private static void BuildStandaloneControls()
    {
        Directory.CreateDirectory(Parts + "Controls");
        foreach (var spec in new[] { ("Add", "button-add", "Add", 164f, 52f), ("Purchase", "button-purchase", "Purchase", 452f, 123f), ("Plus", "button-plus", "+", 44f, 44f), ("Minus", "button-minus", "-", 44f, 44f), ("Remove", "button-remove", "X", 36f, 36f), ("Close", "button-close", "X", 82f, 82f) })
        {
            var control = Button(spec.Item1, null, spec.Item3, spec.Item2, 0, 0, spec.Item4, spec.Item5, spec.Item1 == "Purchase" ? 44 : 26, Cream);
            SceneManager.MoveGameObjectToScene(control.gameObject, staging);
            PrefabUtility.SaveAsPrefabAsset(control.gameObject, Parts + "Controls/" + spec.Item1 + "Button.prefab");
            Object.DestroyImmediate(control.gameObject);
        }
    }

    public static void CreateScene()
    {
        if (EditorSceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save your current scene before creating the demo.");
        Directory.CreateDirectory("Assets/ShopSystem/Scenes");
        AssetDatabase.Refresh();
        if (!AssetDatabase.CopyAsset("Assets/ShopSystem/Scenes/ShopSystemSampleScene.unity", ScenePath))
            throw new InvalidOperationException("Could not copy the original sample scene.");
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var old = scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<ShopManager>(true)).Single();
        var canvas = old.GetComponentInParent<Canvas>().gameObject;
        var fresh = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Parts + "ShopManager.prefab"), canvas.transform);
        var manager = fresh.GetComponent<ShopManager>();
        old.gameObject.SetActive(false);
        PrefabUtility.RecordPrefabInstancePropertyModifications(old.gameObject);
        foreach (var component in scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<MonoBehaviour>(true)))
        {
            if (!(component is ShopDebug) && !(component is PlayerDebug)) continue;
            var so = new SerializedObject(component);
            so.FindProperty("_shopManager").objectReferenceValue = manager;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        // A Canvas variant retains the original nested shop as an inactive inherited object.
        PrefabUtility.SaveAsPrefabAssetAndConnect(canvas, Parts + "Canvas.prefab", InteractionMode.AutomatedAction);
        EditorSceneManager.SaveScene(scene);
    }

    private static void ImportSprite(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100; importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048; importer.wrapMode = TextureWrapMode.Clamp;
        string name = Path.GetFileNameWithoutExtension(path);
        bool sliced = !(name.StartsWith("item-") && name != "item-card") && !new[] { "village-background", "title-sign", "gold-coin", "cart-icon", "leaf-ornament", "button-remove" }.Contains(name);
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        float border = Mathf.Round(Mathf.Min(width, height) * .2f);
        importer.spriteBorder = sliced ? new Vector4(border,border,border,border) : Vector4.zero;
        importer.SaveAndReimport();
    }

    private static Image Skin(RectTransform rect, string asset, bool sliced = true, float multiplier = 6)
    {
        var image = rect.GetComponent<Image>();
        if (image == null) image = rect.gameObject.AddComponent<Image>();
        image.sprite = asset == null ? null : AssetDatabase.LoadAssetAtPath<Sprite>(Art + asset + ".png");
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.pixelsPerUnitMultiplier = multiplier; image.color = Color.white;
        image.raycastTarget = true; return image;
    }
    private static Image Picture(string name, Transform parent, string asset, float x,float y,float w,float h,bool sliced=false,float multiplier=6)
    {
        var image = Skin(Rect(name,parent,x,y,w,h),asset,sliced,multiplier);
        image.raycastTarget = false; image.preserveAspect = !sliced && asset != "village-background";
        return image;
    }
    private static RectTransform Rect(string name, Transform parent,float x,float y,float w,float h)
    {
        var go = new GameObject(name,typeof(RectTransform)); go.layer = 5;
        var rect = (RectTransform)go.transform; rect.SetParent(parent,false); Place(rect,x,y,w,h); return rect;
    }
    private static void Place(RectTransform rect,float x,float y,float w,float h)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0,1);
        rect.anchoredPosition = new Vector2(x,-y); rect.sizeDelta = new Vector2(w,h);
        rect.localScale = Vector3.one; rect.localRotation = Quaternion.identity;
    }
    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one;
    }
    private static TextMeshProUGUI Label(string name,Transform parent,string text,float x,float y,float w,float h,float size,Color color,TextAlignmentOptions alignment=TextAlignmentOptions.Center)
    {
        var label = Rect(name,parent,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset; label.text = text; label.fontSize = size; label.fontStyle = FontStyles.Bold;
        label.color = color; label.alignment = alignment; label.enableAutoSizing = true; label.fontSizeMin = size*.7f; label.fontSizeMax = size;
        label.textWrappingMode = TextWrappingModes.NoWrap; label.overflowMode = TextOverflowModes.Truncate; label.raycastTarget = false; return label;
    }
    private static Button Button(string name,Transform parent,string text,string sprite,float x,float y,float w,float h,float size,Color color)
    {
        float borderScale = sprite == "button-quantity" || sprite == "button-plus" || sprite == "button-minus" ? 32 : sprite == "button-close" ? 14 : 10;
        var rect = Rect(name,parent,x,y,w,h); var image = Skin(rect,sprite,sprite != "button-remove",borderScale);
        var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
        var colors = button.colors; colors.highlightedColor = new Color(1.1f,1.1f,1.1f); colors.pressedColor = new Color(.8f,.8f,.8f); colors.selectedColor = Color.white; button.colors = colors;
        Label("Label",rect,text,3,2,w-6,h-4,size,color); return button;
    }
    private static ScrollRect Scroll(string name,Transform parent,float x,float y,float w,float h,out RectTransform content,bool horizontal=false)
    {
        var rect = Rect(name,parent,x,y,w,h); var hit = rect.gameObject.AddComponent<Image>(); hit.color = Color.clear;
        rect.gameObject.AddComponent<RectMask2D>(); var scroll = rect.gameObject.AddComponent<ScrollRect>();
        content = Rect("Content",rect,0,0,w,h); content.anchorMin = new Vector2(0,1); content.anchorMax = new Vector2(horizontal?0:1,1); content.sizeDelta = new Vector2(horizontal?w:0,h);
        scroll.viewport = rect; scroll.content = content; scroll.horizontal = horizontal; scroll.vertical = !horizontal;
        scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 45; return scroll;
    }
    private static void AddScrollbar(ScrollRect scroll,Transform parent,float x,float y,float w,float h)
    {
        var rect = Rect(scroll.name+"Scrollbar",parent,x,y,w,h); var track = Skin(rect,"cart-panel",true,10); track.color = new Color(1,1,1,.4f);
        var handle = Rect("Handle",rect,0,0,w,h); Stretch(handle); var image = Skin(handle,"button-quantity",true,12);
        var bar = rect.gameObject.AddComponent<Scrollbar>(); bar.handleRect = handle; bar.targetGraphic = image; bar.direction = Scrollbar.Direction.BottomToTop;
        scroll.verticalScrollbar = bar; scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
    }
    private static void Set(Object target,string field,Object value)
    {
        var so = new SerializedObject(target); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
    }
}
