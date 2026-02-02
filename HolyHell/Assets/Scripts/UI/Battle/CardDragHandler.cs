using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using HolyHell.Battle.Card;
using HolyHell.Battle;
using HolyHell.Battle.Entity;
using R3;
using System.Collections.Generic;

namespace HolyHell.UI.Battle
{
    /// <summary>
    /// Handles card dragging logic, target detection, and line rendering for any card being dragged
    /// Only one card can be dragged at a time, so this handler is shared
    /// </summary>
    public class CardDragHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private GraphicRaycaster graphicRaycaster;
        [SerializeField] private CardDragLineRenderer dragLineRenderer;
        [SerializeField] private RectTransform aimObject;

        private BattleManager battleManager;
        private HandUI handUI;
        private EnemyListUI enemyListUI;

        // Current drag state
        private CardInstance currentCard;
        private RectTransform currentCardRectTransform;
        private EnemyEntity currentHoveredEnemy;
        private bool isLockedToTarget = false;
        private bool isDragging = false;

        private CompositeDisposable disposables = new CompositeDisposable();

        /// <summary>
        /// Initialize the drag handler with BattleUI references
        /// Called once by BattleUI during initialization
        /// </summary>
        public void Initialize(BattleManager battleManager, HandUI handUI, EnemyListUI enemyListUI)
        {
            this.battleManager = battleManager;
            this.handUI = handUI;
            this.enemyListUI = enemyListUI;
            dragLineRenderer.Hide();
            aimObject.gameObject.SetActive(false);

            // Subscribe to card interaction state changes
            battleManager.cardInteractionState
                .Subscribe(state =>
                {
                    Debug.Log($"CardDragHandler: Card interaction state changed to {state}");
                    if (state == CardInteractionState.Dragging && battleManager.currentSelectedCard.Value != null)
                    {
                        OnStartDrag(battleManager.currentSelectedCard.Value);
                    }
                    else if (isDragging)
                    {
                        OnEndDrag();
                    }
                })
                .AddTo(disposables);
        }

        /// <summary>
        /// Called when drag starts
        /// </summary>
        private void OnStartDrag(CardInstance card)
        {
            currentCard = card;
            isDragging = true;

            // Show drag line if card requires target
            if (DoesCardRequireTarget() && dragLineRenderer != null)
            {
                dragLineRenderer.Show();
            }
        }

        /// <summary>
        /// Called when drag ends
        /// </summary>
        private void OnEndDrag()
        {
            isDragging = false;

            // Hide ui elements
            if (dragLineRenderer != null)
            {
                dragLineRenderer.Hide();
            }
            if (aimObject != null)
            {
                aimObject.gameObject.SetActive(false);
            }

            // Reset targeting
            currentHoveredEnemy = null;
            isLockedToTarget = false;
            currentCard = null;
            currentCardRectTransform = null;
        }

        /// <summary>
        /// Update drag position and targeting
        /// Called from Update loop when dragging is active
        /// </summary>
        public void UpdateDrag(RectTransform cardRect)
        {
            if (!isDragging || Mouse.current == null || currentCard == null) return;

            currentCardRectTransform = cardRect;
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            bool requiresTarget = DoesCardRequireTarget();

            if (!requiresTarget)
            {
                // No target required - just update line to mouse if visible
                if (dragLineRenderer != null && dragLineRenderer.gameObject.activeSelf)
                {
                    dragLineRenderer.UpdateLineFromUIToScreen(currentCardRectTransform, mouseScreenPos, false);
                }
            }
            else
            {
                // Requires target - detect enemies and update targeting line
                DetectEnemyUnderMouse(mouseScreenPos);

                // Update drag line
                if (dragLineRenderer != null)
                {
                    if (isLockedToTarget && currentHoveredEnemy != null)
                    {
                        // Line to enemy position
                        var enemyUI = FindEnemyUI(currentHoveredEnemy);
                        if (enemyUI != null)
                        {
                            var enemyRect = enemyUI.GetComponent<RectTransform>();
                            dragLineRenderer.UpdateLineFromUI(currentCardRectTransform, enemyRect.position, true);
                        }
                    }
                    else
                    {
                        // Line to mouse position
                        dragLineRenderer.UpdateLineFromUIToScreen(currentCardRectTransform, mouseScreenPos, false);
                    }
                }

                if (aimObject != null)
                {
                    Vector3 worldPos = default;
                    if (isLockedToTarget && currentHoveredEnemy != null)
                    {
                        var enemyUI = FindEnemyUI(currentHoveredEnemy);
                        if (enemyUI != null)
                        {
                            worldPos = enemyUI.transform.position;
                        }
                    }
                    else
                    {
                        RectTransformUtility.ScreenPointToWorldPointInRectangle(
                            aimObject,
                            mouseScreenPos,
                            canvas.worldCamera,
                            out worldPos
                        );
                    }
                    aimObject.gameObject.SetActive(true);
                    aimObject.transform.position = worldPos;
                }
            }
        }

        /// <summary>
        /// Check if the current card requires a target to be played
        /// </summary>
        public bool DoesCardRequireTarget()
        {
            if (currentCard == null) return false;

            // Check if any of the card's effects require targeting an enemy
            for (int i = 1; i <= 4; i++)
            {
                var effectType = currentCard.GetEffectType(i);
                if (IsTargetRequiredForEffect(effectType))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a specific card requires a target (can be called for any card)
        /// </summary>
        public bool DoesCardRequireTarget(CardInstance card)
        {
            if (card == null) return false;

            for (int i = 1; i <= 4; i++)
            {
                var effectType = card.GetEffectType(i);
                if (IsTargetRequiredForEffect(effectType))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if a specific effect type requires targeting
        /// </summary>
        private bool IsTargetRequiredForEffect(CardEffectType effectType)
        {
            return effectType switch
            {
                CardEffectType.SingleDamage => true,
                CardEffectType.TargetSingleBuff => true,
                CardEffectType.DelaySingleDamage => true,
                // AOE, self-buffs, and other effects don't require targeting
                _ => false
            };
        }

        /// <summary>
        /// Detect if mouse is hovering over an enemy using raycasting
        /// </summary>
        private void DetectEnemyUnderMouse(Vector2 screenPos)
        {
            if (graphicRaycaster == null) return;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = screenPos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerData, results);

            // Find if any result is an EnemyUI
            EnemyEntity hoveredEnemy = null;
            foreach (var result in results)
            {
                var enemyUI = result.gameObject.GetComponent<EnemyUI>();
                if (enemyUI != null && enemyUI.Enemy != null && enemyUI.Enemy.hp.Value > 0)
                {
                    hoveredEnemy = enemyUI.Enemy;
                    break;
                }
            }

            // Update locked state
            if (hoveredEnemy != null)
            {
                if (currentHoveredEnemy != hoveredEnemy)
                {
                    currentHoveredEnemy = hoveredEnemy;
                    Debug.Log($"CardDragHandler: Locked onto enemy: {hoveredEnemy.enemyData?.DisplayName}");
                }
                isLockedToTarget = true;
            }
            else
            {
                if (isLockedToTarget)
                {
                    Debug.Log("CardDragHandler: Unlocked from enemy");
                }
                currentHoveredEnemy = null;
                isLockedToTarget = false;
            }
        }

        /// <summary>
        /// Find the UI component for a given enemy entity (uses cached lookup from EnemyListUI)
        /// </summary>
        private EnemyUI FindEnemyUI(EnemyEntity enemy)
        {
            if (enemyListUI == null)
            {
                Debug.LogWarning("CardDragHandler: EnemyListUI reference is null, falling back to slow search");
                // Fallback to FindObjectsOfType if EnemyListUI is not available
                var allEnemyUIs = FindObjectsByType<EnemyUI>(FindObjectsSortMode.None);
                foreach (var enemyUI in allEnemyUIs)
                {
                    if (enemyUI.Enemy == enemy)
                    {
                        return enemyUI;
                    }
                }
                return null;
            }

            // Use cached lookup from EnemyListUI (much faster)
            return enemyListUI.FindEnemyUI(enemy);
        }

        /// <summary>
        /// Get the currently locked target (if any)
        /// </summary>
        public EnemyEntity GetLockedTarget()
        {
            return isLockedToTarget ? currentHoveredEnemy : null;
        }

        public Camera GetCanvasCamera()
        {
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                return canvas.worldCamera;
            }
            return null;
        }

        /// <summary>
        /// Check if currently locked to a target
        /// </summary>
        public bool IsLockedToTarget => isLockedToTarget;

        /// <summary>
        /// Clean up subscriptions
        /// </summary>
        public void Cleanup()
        {
            disposables.Dispose();
            isDragging = false;
            currentHoveredEnemy = null;
            isLockedToTarget = false;

            if (dragLineRenderer != null)
            {
                dragLineRenderer.Hide();
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
