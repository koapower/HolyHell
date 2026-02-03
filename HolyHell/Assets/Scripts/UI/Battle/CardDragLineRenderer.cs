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
        [SerializeField] private int curveSegments = 40; // Number of points in the curve
        private float endControlPointYOffset = 100f; // in screen space pixels
        private float endpointWidth = 5f; // Width at start/end points
        private float centerWidth = 15f; // Width at center of the line

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private Color lockedColor = new Color(0f, 1f, 0f, 0.8f); // Green when locked to target

        [Header("Shine Animation")]
        [SerializeField] private bool enableShine = true;
        [SerializeField] private float shineDuration = 0.5f; // Time to move from start to end
        [SerializeField] private float shineCooldown = 0.2f; // Time between shine loops
        [SerializeField] private bool loopShine = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private Color gizmosColor = Color.yellow;

        private Vector2 startPosition;
        private Vector2 endPosition;
        private bool isLockedToTarget = false;
        private bool isVisible = false;

        // Control points for debugging
        private List<Vector2> lastControlPoints = new List<Vector2>();

        // Shine animation
        private float shineTimer = 0f;
        private static readonly int ShinePositionProperty = Shader.PropertyToID("_ShinePosition");

        /// <summary>
        /// Show the line
        /// </summary>
        public void Show()
        {
            isVisible = true;
            gameObject.SetActive(true);
            shineTimer = 0f; // Reset shine animation
            SetVerticesDirty(); // Force mesh rebuild
        }

        /// <summary>
        /// Hide the line
        /// </summary>
        public void Hide()
        {
            isVisible = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!isVisible || !enableShine) return;

            // Update shine animation
            if (shineDuration > 0)
            {
                shineTimer += Time.deltaTime;

                if (loopShine && shineTimer >= shineDuration)
                {
                    shineTimer = -shineCooldown; // Loop back to start + cooldown time
                }

                if (shineTimer >= 0)
                {
                    // Calculate shine position (0 to 1 along the line)
                    float shinePosition = Mathf.Clamp01(shineTimer / shineDuration);

                    // Update material property
                    if (material != null)
                    {
                        material.SetFloat(ShinePositionProperty, shinePosition);
                    }
                }
            }
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

            // Calculate cubic bezier curve control points
            List<Vector2> controlPoints = CalculateControlPoints(localStart, localEnd);

            // Store for gizmos debugging
            lastControlPoints = new List<Vector2>(controlPoints);

            // Generate curve points
            List<Vector2> curvePoints = new List<Vector2>();
            for (int i = 0; i < curveSegments; i++)
            {
                float t = i / (float)(curveSegments - 1);
                Vector2 point = CalculateBezierPoint(t, controlPoints);
                curvePoints.Add(point);
            }

            // Generate mesh for the line (as a ribbon/strip)
            GenerateLineMesh(vh, curvePoints);
        }

        /// <summary>
        /// Generate a mesh for the line using triangle strip
        /// Width varies from endpointWidth (A) -> centerWidth (B) -> endpointWidth (A)
        /// UV.y goes from 0 (outer edge) to 1 (center line) for gradient material support
        /// </summary>
        private void GenerateLineMesh(VertexHelper vh, List<Vector2> points)
        {
            if (points.Count < 2) return;

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

                // Calculate width at this point (A -> B -> A using smooth curve)
                float t = i / (float)(points.Count - 1); // 0 to 1 along the line
                float widthMultiplier = CalculateWidthMultiplier(t);
                float currentWidth = Mathf.Lerp(endpointWidth, centerWidth, widthMultiplier);
                float halfWidth = currentWidth * 0.5f;

                // Create two vertices perpendicular to the line direction
                Vector2 offset = perpendicular * halfWidth;
                Vector2 v1 = point + offset;
                Vector2 v2 = point - offset;

                // UV coordinates
                // uvX: 0 to 1 along the line length (for texture tiling along the line)
                // uvY: 0 at outer edge, 1 at center line (for gradient from edge to center)
                float uvX = t;

                UIVertex vertex = UIVertex.simpleVert;
                vertex.color = color;

                // Add outer vertex (uvY = 0, for dark outer edge)
                vertex.position = new Vector3(v1.x, v1.y, 0);
                vertex.uv0 = new Vector2(uvX, 0);
                vh.AddVert(vertex);

                // Add inner vertex (uvY = 1, for white center)
                vertex.position = new Vector3(v2.x, v2.y, 0);
                vertex.uv0 = new Vector2(uvX, 1);
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
        /// Calculate width multiplier at position t (0 to 1)
        /// Returns 0 at endpoints, 1 at center using smooth curve
        /// </summary>
        private float CalculateWidthMultiplier(float t)
        {
            // Use sine curve for smooth transition: 0 -> 1 -> 0
            // sin goes from 0 to 1 to 0 as input goes from 0 to PI
            return Mathf.Sin(t * Mathf.PI);
        }

        /// <summary>
        /// Calculate the control points for cubic bezier curve
        /// Returns 4 points: P0 (start), P1 (start + Y offset), P2 (end + Y offset), P3 (end)
        /// </summary>
        private List<Vector2> CalculateControlPoints(Vector2 start, Vector2 end)
        {
            List<Vector2> points = new List<Vector2>();
            points.Add(start);
            points.Add(new Vector2(start.x, end.y + endControlPointYOffset));
            points.Add(new Vector2((start.x + end.x) / 2, end.y + endControlPointYOffset * 3f));
            points.Add(new Vector2(end.x, end.y + endControlPointYOffset));
            points.Add(end);

            return points;
        }

        /// <summary>
        /// Calculate a point on a bezier curve with any number of control points
        /// Uses De Casteljau's algorithm for numerical stability
        /// </summary>
        private Vector2 CalculateBezierPoint(float t, List<Vector2> controlPoints)
        {
            if (controlPoints == null || controlPoints.Count == 0)
            {
                Debug.LogError("CalculateBezierPoint requires at least one control point");
                return Vector2.zero;
            }

            if (controlPoints.Count == 1)
            {
                return controlPoints[0];
            }

            // De Casteljau's algorithm
            // Create a temporary list to store intermediate points
            List<Vector2> tempPoints = new List<Vector2>(controlPoints);

            // Recursively interpolate between points until we have just one point left
            while (tempPoints.Count > 1)
            {
                List<Vector2> nextLevel = new List<Vector2>(tempPoints.Count - 1);

                for (int i = 0; i < tempPoints.Count - 1; i++)
                {
                    // Linear interpolation between adjacent points
                    Vector2 interpolated = Vector2.Lerp(tempPoints[i], tempPoints[i + 1], t);
                    nextLevel.Add(interpolated);
                }

                tempPoints = nextLevel;
            }

            return tempPoints[0];
        }

        /// <summary>
        /// Set the line widths dynamically
        /// </summary>
        public void SetLineWidths(float endWidth, float midWidth)
        {
            endpointWidth = endWidth;
            centerWidth = midWidth;
            SetVerticesDirty();
        }

        /// <summary>
        /// Set uniform line width (same at endpoints and center)
        /// </summary>
        public void SetLineWidth(float width)
        {
            endpointWidth = width;
            centerWidth = width;
            SetVerticesDirty();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Draw gizmos in editor to visualize control points
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || !isVisible || lastControlPoints.Count == 0) return;

            // Draw control points as spheres
            Gizmos.color = gizmosColor;

            // Convert local control points to world space for gizmos
            for (int i = 0; i < lastControlPoints.Count; i++)
            {
                Vector3 worldPos = transform.TransformPoint(lastControlPoints[i]);

                // Draw sphere at control point
                Gizmos.DrawSphere(worldPos, 5f);

                // Draw label
                UnityEditor.Handles.Label(worldPos, $"P{i}");
            }

            // Draw lines connecting control points to show the control polygon
            Gizmos.color = new Color(gizmosColor.r, gizmosColor.g, gizmosColor.b, 0.3f);
            for (int i = 0; i < lastControlPoints.Count - 1; i++)
            {
                Vector3 worldPos1 = transform.TransformPoint(lastControlPoints[i]);
                Vector3 worldPos2 = transform.TransformPoint(lastControlPoints[i + 1]);
                Gizmos.DrawLine(worldPos1, worldPos2);
            }

            // Draw the actual bezier curve
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f); // Red for the curve
            for (int i = 0; i < curveSegments - 1; i++)
            {
                float t1 = i / (float)(curveSegments - 1);
                float t2 = (i + 1) / (float)(curveSegments - 1);

                Vector2 point1 = CalculateBezierPoint(t1, lastControlPoints);
                Vector2 point2 = CalculateBezierPoint(t2, lastControlPoints);

                Vector3 worldPos1 = transform.TransformPoint(point1);
                Vector3 worldPos2 = transform.TransformPoint(point2);

                Gizmos.DrawLine(worldPos1, worldPos2);
            }
        }
#endif
    }
}
