using UnityEngine;

/// <summary>Keeps the illustrated background and interactive controls aligned at any aspect ratio.</summary>
[ExecuteAlways, RequireComponent(typeof(RectTransform))]
public sealed class ShopDesignCanvas : MonoBehaviour
{
    private void OnEnable() => Fit();
    private void OnRectTransformDimensionsChange() => Fit();
    private void LateUpdate() => Fit();

    private void Fit()
    {
        var rect = (RectTransform)transform;
        if (!(rect.parent is RectTransform parent)) return;
        float scale = Mathf.Min(parent.rect.width / 1920f, parent.rect.height / 1080f);
        if (scale <= 0) return;
        rect.localScale = new Vector3(scale, scale, 1);
    }
}
