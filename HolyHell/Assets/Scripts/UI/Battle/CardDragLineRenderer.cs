using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace HolyHell.UI.Battle
{
    /// <summary>
    /// Renders a curved line from card to mouse/target position during drag
    /// Uses UI.Graphic and OnPopulateMesh to work with Screen Space Overlay Canvas
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class CardDragLineRenderer : Graphic
    {
        [Header("Line Settings")]
        [SerializeField] private int curveSegments = 30; // Number of points in the curve
        [SerializeField] private float arcHeight = 50f; // Height of the arc in screen space pixels
        [SerializeField] private float lineWidth = 8f; // Width of the line in pixels

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private Color lockedColor = new Color(0f, 1f, 0f, 0.8f); // Green when locked to target

        private Vector2 startPosition;
        private Vector2 endPosition;
        private bool isLockedToTarget = false;
        private bool isVisible = false;

        /// <summary>
        /// Show the line
        /// </summary>
        public void Show()
        {
            isVisible = true;
            gameObject.SetActive(true);
            SetVerticesDirty(); // Force mesh rebuild
            Debug.Log("CardDragLineRenderer: Show line");
        }

        /// <summary>
        /// Hide the line
        /// </summary>
        public void Hide()
        {
            isVisible = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Update the line to draw from startPos to endPos with an arc
        /// Positions are in screen space
        /// </summary>
        public void UpdateLine(Vector2 startScreenPos, Vector2 endScreenPos, bool isLocked = false)
        {
            startPosition = startScreenPos;
            endPosition = endScreenPos;
            isLockedToTarget = isLocked;

            // Update color
            color = isLockedToTarget ? lockedColor : normalColor;

            // Mark mesh as dirty to trigger rebuild
            SetVerticesDirty();
        }

        /// <summary>
        /// Update the line from a UI RectTransform position to another position
        /// </summary>
        public void UpdateLineFromUI(RectTransform startRect, Vector3 endWorldPos, bool isLocked = false)
        {
            // Convert world positions to screen space
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

            Vector2 startScreenPos = RectTransformUtility.WorldToScreenPoint(cam, startRect.position);
            Vector2 endScreenPos = RectTransformUtility.WorldToScreenPoint(cam, endWorldPos);

            UpdateLine(startScreenPos, endScreenPos, isLocked);
        }

        /// <summary>
        /// Update the line from UI RectTransform to screen position (mouse)
        /// </summary>
        public void UpdateLineFromUIToScreen(RectTransform startRect, Vector2 screenPos, bool isLocked = false)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

            Vector2 startScreenPos = RectTransformUtility.WorldToScreenPoint(cam, startRect.position);

            UpdateLine(startScreenPos, screenPos, isLocked);
        }

        /// <summary>
        /// OnPopulateMesh is called by Unity UI system to generate the mesh
        /// </summary>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (!isVisible)
            {
                return; // Don't draw anything if not visible
            }

            // Convert screen space positions to local rect space
            Vector2 localStart, localEnd;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, startPosition, null, out localStart);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, endPosition, null, out localEnd);

            // Calculate bezier curve control point
            Vector2 controlPoint = CalculateControlPoint(localStart, localEnd);

            // Generate curve points
            List<Vector2> curvePoints = new List<Vector2>();
            for (int i = 0; i < curveSegments; i++)
            {
                float t = i / (float)(curveSegments - 1);
                Vector2 point = CalculateBezierPoint(t, localStart, controlPoint, localEnd);
                curvePoints.Add(point);
            }

            // Generate mesh for the line (as a ribbon/strip)
            GenerateLineMesh(vh, curvePoints);
        }

        /// <summary>
        /// Generate a mesh for the line using triangle strip
        /// </summary>
        private void GenerateLineMesh(VertexHelper vh, List<Vector2> points)
        {
            if (points.Count < 2) return;

            float halfWidth = lineWidth * 0.5f;

            // Generate vertices along the curve
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];
                Vector2 perpendicular;

                if (i == 0)
                {
                    // First point - use direction to next point
                    Vector2 forward = (points[i + 1] - point).normalized;
                    perpendicular = new Vector2(-forward.y, forward.x);
                }
                else if (i == points.Count - 1)
                {
                    // Last point - use direction from previous point
                    Vector2 forward = (point - points[i - 1]).normalized;
                    perpendicular = new Vector2(-forward.y, forward.x);
                }
                else
                {
                    // Middle points - use average of incoming and outgoing directions
                    Vector2 forward1 = (point - points[i - 1]).normalized;
                    Vector2 forward2 = (points[i + 1] - point).normalized;
                    Vector2 forward = (forward1 + forward2).normalized;
                    perpendicular = new Vector2(-forward.y, forward.x);
                }

                // Create two vertices perpendicular to the line direction
                Vector2 offset = perpendicular * halfWidth;
                Vector2 v1 = point + offset;
                Vector2 v2 = point - offset;

                // UV coordinates for texturing (if needed)
                float uvX = i / (float)(points.Count - 1);

                UIVertex vertex = UIVertex.simpleVert;
                vertex.color = color;

                // Add top vertex
                vertex.position = new Vector3(v1.x, v1.y, 0);
                vertex.uv0 = new Vector2(uvX, 1);
                vh.AddVert(vertex);

                // Add bottom vertex
                vertex.position = new Vector3(v2.x, v2.y, 0);
                vertex.uv0 = new Vector2(uvX, 0);
                vh.AddVert(vertex);
            }

            // Generate triangles to connect vertices
            for (int i = 0; i < points.Count - 1; i++)
            {
                int baseIndex = i * 2;

                // Two triangles per segment to form a quad
                // Triangle 1
                vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
                // Triangle 2
                vh.AddTriangle(baseIndex + 2, baseIndex + 1, baseIndex + 3);
            }
        }

        /// <summary>
        /// Calculate the control point for the bezier curve (creates the arc)
        /// </summary>
        private Vector2 CalculateControlPoint(Vector2 start, Vector2 end)
        {
            // Middle point between start and end
            Vector2 midPoint = (start + end) / 2f;

            // Calculate perpendicular direction (for 2D)
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            // Offset the control point perpendicular to the line to create an arc
            Vector2 controlPoint = midPoint + perpendicular * arcHeight;

            return controlPoint;
        }

        /// <summary>
        /// Calculate a point on a quadratic bezier curve
        /// </summary>
        private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            // Quadratic Bezier formula: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector2 point = uu * p0; // (1-t)² * P0
            point += 2 * u * t * p1; // 2(1-t)t * P1
            point += tt * p2; // t² * P2

            return point;
        }

        /// <summary>
        /// Set the arc height dynamically
        /// </summary>
        public void SetArcHeight(float height)
        {
            arcHeight = height;
            SetVerticesDirty();
        }

        /// <summary>
        /// Set the line width dynamically
        /// </summary>
        public void SetLineWidth(float width)
        {
            lineWidth = width;
            SetVerticesDirty();
        }
    }
}
