using UnityEngine;

/// <summary>
/// Attaches to a CardSlot parent. Scales the first child RectTransform so it
/// appears at childDesiredSize while fitting inside the slot, centered.
/// No update loop - reacts to inspector changes (OnValidate) and rect resize
/// (OnRectTransformDimensionsChange) only.
/// </summary>
[ExecuteAlways]
public class CardSlotSizer : MonoBehaviour
{
    [SerializeField] private Vector2 childDesiredSize = new Vector2(100f, 200f);

    private RectTransform slotRect;

    private void Awake()
    {
        slotRect = GetComponent<RectTransform>();
    }

    // Called in editor when a serialized field changes via inspector
    private void OnValidate()
    {
        if (slotRect == null)
            slotRect = GetComponent<RectTransform>();
        ApplyChildSize();
    }

    // Called when this RectTransform's size changes (including during editor layout)
    private void OnRectTransformDimensionsChange()
    {
        if (slotRect == null)
            slotRect = GetComponent<RectTransform>();
        ApplyChildSize();
    }

    /// <summary>
    /// Sets the first child's sizeDelta, anchoredPosition, and localScale so it
    /// fills childDesiredSize scaled uniformly to fit the slot, centered.
    /// </summary>
    public void ApplyChildSize()
    {
        if (slotRect == null) return;
        if (transform.childCount == 0) return;

        var child = transform.GetChild(0).GetComponent<RectTransform>();
        if (child == null) return;

        Vector2 slotSize = slotRect.rect.size;

        // Avoid division by zero during initial layout pass
        if (slotSize.x <= 0 || slotSize.y <= 0 || childDesiredSize.x <= 0 || childDesiredSize.y <= 0)
            return;

        float scaleX = slotSize.x / childDesiredSize.x;
        float scaleY = slotSize.y / childDesiredSize.y;
        float uniformScale = Mathf.Min(scaleX, scaleY);

        child.sizeDelta = childDesiredSize;
        child.anchoredPosition = Vector2.zero;
        child.localScale = new Vector3(uniformScale, uniformScale, 1f);
    }

    public Vector2 ChildDesiredSize
    {
        get => childDesiredSize;
        set
        {
            childDesiredSize = value;
            ApplyChildSize();
        }
    }
}
