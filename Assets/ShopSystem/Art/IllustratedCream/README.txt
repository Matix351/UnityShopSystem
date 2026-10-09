ILLUSTRATED CREAM SHOP

Original setup
  Scene: Assets/ShopSystem/Scenes/ShopSystemSampleScene.unity
  Prefabs: Assets/ShopSystem/Prefabs/ShopManager.prefab and Canvas.prefab
  Original components: Assets/ShopSystem/Prefabs/comp/

The original ShopManager prefab was restored from commit f08a662. The original
Canvas and component prefabs were not modified. Existing sample-scene overrides
were preserved. A pre-reorganization backup is in output/shop-ui/recovery/.

Redesigned setup
  Scene: Assets/ShopSystem/Scenes/ShopSystemIllustratedCream.unity
  Variants: Assets/ShopSystem/Prefabs/IllustratedCream/Variants/
  Separate reusable button prefabs: Variants/Controls/

ShopManager, Canvas, ItemSlot, CartItemSlot and TagButton are actual Unity Prefab
Variants with the original prefab as their direct parent. Original visual children
are retained as inactive overrides; the illustrated controls use the inherited
shop components with variant-specific references. Do not Apply All overrides to
the base prefabs if you want the original presentation to remain unchanged.

The redesigned scene was copied from the original sample scene. Its ShopDebug
and PlayerDebug references target the new shop. The existing item data, purchase
handler, affordability checks and inventory checks are reused.

Both scenes share Assets/ShopSystem/Prefabs/CanvasDebug.prefab, which contains
the nested shared Assets/ShopSystem/Prefabs/ShopExample.prefab. ShopDebug and
PlayerDebug keep their scene-specific shop references as instance overrides.

PNG assets
  All exported graphics: Assets/ShopSystem/Art/IllustratedCream/PNG/
  Every background, panel, button, coin and decoration is a separate PNG.
  item-*.png (except item-card.png) are copies of existing item icons; the original
  ScriptableObjects still reference their original icons to preserve shared data.
  The +, -, X, titles, quantities, prices and captions are live TextMeshPro objects.
  Blank plus/minus PNGs are intentionally separate copies of the same button face.
  Reusable panels and rectangular controls are imported with nine-slice borders.
  There is no text on the village background and no adventure tagline.

Art was generated with the built-in image_gen tool using the selected concept as
reference, then transparent outer padding was cropped. Exact prompts and sources
are saved in PNG/generation.json. Recreated sprites match the selected art direction;
existing item illustrations and live typography remain the project's own assets.

Rebuild
  Tools > Shop > Build Illustrated Cream Variants
  This updates generated variants only. It creates the demo scene if absent, but
  never overwrites an existing scene or saves original prefab assets. Back up any
  manual edits to generated variants before rebuilding.

Verification artifacts are in output/shop-ui/ (test results and runtime screenshots).
Previous flattened artwork and standalone prefabs are preserved outside Assets in
output/shop-ui/previous-pass/ so they cannot be confused with the new variants.
