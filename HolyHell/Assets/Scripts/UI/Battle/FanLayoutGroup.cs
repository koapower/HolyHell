using UnityEngine;

[ExecuteInEditMode] // Allows real-time preview within the Unity Editor
public class FanLayoutGroup : MonoBehaviour
{
    [Header("Layout Settings")]
    public RectTransform anchorPoint; // The peak point of the arc
    public float radius = 100f;       // Distance 'N' from the anchor to the imaginary circle center
    public float anglePerCard = 5f;   // Angle 'M' added for each subsequent card

    [Header("Rotation")]
    public bool rotateCards = true;   // Whether cards should rotate to follow the arc

#if UNITY_EDITOR
    [Header("Editor Only")]
    public bool enableUpdateOnValidate = true; // Whether to update layout on property changes
#endif

#if UNITY_EDITOR
    void OnValidate()
    {
        if (enableUpdateOnValidate)
            LayoutCards();
    }
#endif

    [ContextMenu("Execute Layout Now")]
    public void LayoutCards()
    {
        if (anchorPoint == null) return;

        // Count only active children to avoid layout gaps
        int childCount = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeSelf) childCount++;
        }

        if (childCount == 0) return;

        // Calculate total spread angle: (Total Cards - 1) * M
        float totalAngle = (childCount - 1) * anglePerCard;

        // Start angle is half of total angle (negative) for symmetrical distribution
        float startAngle = -totalAngle / 2f;

        // The circle center is located N units directly below the Anchor Point
        Vector3 centerPosition = anchorPoint.localPosition + Vector3.down * radius;

        int currentIndex = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf) continue;

            // Calculate current angle (In Unity, 0 deg is Up, positive is counter-clockwise)
            float currentAngle = startAngle + (currentIndex * anglePerCard);

            // Convert angle to radians for trigonometry
            float radian = currentAngle * Mathf.Deg2Rad;

            // Calculate position on the arc (x = sin * r, y = cos * r)
            float x = Mathf.Sin(radian) * radius;
            float y = Mathf.Cos(radian) * radius;

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(child, "Fan Layout Update");
#endif

            // Apply position relative to the calculated circle center
            child.localPosition = centerPosition + new Vector3(x, y, 0);

            // Apply rotation so the "top" of the card points outward from the center
            if (rotateCards)
            {
                child.localRotation = Quaternion.Euler(0, 0, -currentAngle);
            }
            else
            {
                child.localRotation = Quaternion.identity;
            }

            currentIndex++;
        }
    }
}