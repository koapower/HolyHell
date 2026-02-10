using R3;
using System.Collections.Generic;
using HolyHell.Battle.Enemy;
using UnityEngine;

namespace HolyHell.Battle.Entity
{
    public class EnemyEntity : BattleEntity
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider2D;

        private IBattleManager battleManager;
        public EnemyRow enemyData;
        public EnemyBehaviorRow behaviorRow;

        // Current intent (what the enemy will do next turn)
        public ReactiveProperty<EnemySkill> currentIntent = new ReactiveProperty<EnemySkill>();

        // Available skills (flat list kept for UI / fallback use)
        public List<EnemySkill> availableSkills = new List<EnemySkill>();

        // AI reference
        public EnemyAI ai;

        // Visual feedback
        private Color originalColor;
        private Color hoverColor = new Color(1f, 1f, 0.8f, 1f);
        private Color targetableColor = new Color(0.8f, 1f, 0.8f, 1f);
        private bool isInSelectionMode = false; // Whether selection mode is active globally
        private bool isHovered = false;

        public void Initialize(IBattleManager battleManager, EnemyRow data, EnemyBehaviorRow behavior, List<EnemySkill> skills)
        {
            this.battleManager = battleManager;
            enemyData = data;
            behaviorRow = behavior;
            availableSkills = skills;

            // Set initial stats
            maxHp.Value = data.Hp;
            hp.Value = data.Hp;
            shield.Value = 0;

            // Set base elemental resistances from table data
            elementResistances[ElementType.Despair]     = data.DespairResistance;
            elementResistances[ElementType.Enlightened] = data.EnlightenedResistance;
            elementResistances[ElementType.Bliss]       = data.BlissResistance;
            elementResistances[ElementType.Ravenous]    = data.RavenousResistance;
            elementResistances[ElementType.Domination]  = data.DominationResistance;

            // Initialize AI
            ai = new EnemyAI(battleManager, this);

            // Cache skill slots for weighted selection
            if (behavior != null)
                ai.CacheSkillSlots(behavior, skills);

            // Get references
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (boxCollider2D == null)
                boxCollider2D = GetComponent<BoxCollider2D>();

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            // Initially disable collider (not in selection mode)
            if (boxCollider2D != null)
            {
                boxCollider2D.enabled = false;
            }
        }

        public Vector3 GetTargetWorldPosition()
        {
            return spriteRenderer != null ? spriteRenderer.transform.position : Vector3.zero;
        }

        /// <summary>
        /// Enter selection mode - entity decides if it can be targeted
        /// </summary>
        public void EnterSelectionMode()
        {
            isInSelectionMode = true;

            // Check if this enemy can be targeted based on its state
            bool canBeTargeted = CanBeTargeted();

            if (canBeTargeted)
            {
                // Enable collider to allow raycasting
                if (boxCollider2D != null)
                {
                    boxCollider2D.enabled = true;
                }

                // Update visual to show targetable
                UpdateVisual();
            }
            else
            {
                // Keep collider disabled
                if (boxCollider2D != null)
                {
                    boxCollider2D.enabled = false;
                }
            }
        }

        /// <summary>
        /// Exit selection mode - disable interaction
        /// </summary>
        public void ExitSelectionMode()
        {
            isInSelectionMode = false;
            isHovered = false;

            // Disable collider
            if (boxCollider2D != null)
            {
                boxCollider2D.enabled = false;
            }

            // Reset visual
            UpdateVisual();
        }

        /// <summary>
        /// Set whether mouse is hovering over this enemy
        /// Only effective when in selection mode
        /// </summary>
        public void SetHovered(bool hovered)
        {
            if (!isInSelectionMode) return;

            isHovered = hovered;
            UpdateVisual();
        }

        /// <summary>
        /// Check if this enemy can be targeted based on its current state
        /// </summary>
        public bool CanBeTargeted()
        {
            // Cannot target dead enemies
            if (hp.Value <= 0) return false;

            // Add more conditions here if needed (e.g., stunned, invisible, etc.)
            // For example:
            // if (HasBuff(BuffType.Untargetable)) return false;

            return true;
        }

        /// <summary>
        /// Public accessor to check if entity can currently be targeted
        /// </summary>
        public bool IsTargetable()
        {
            return isInSelectionMode && CanBeTargeted();
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;

            if (hp.Value <= 0)
            {
                // Dead - use original color with low alpha
                Color deadColor = originalColor;
                deadColor.a = 0.5f;
                spriteRenderer.color = deadColor;
            }
            else if (isInSelectionMode && CanBeTargeted())
            {
                // In selection mode and can be targeted
                if (isHovered)
                {
                    spriteRenderer.color = hoverColor;
                }
                else
                {
                    spriteRenderer.color = targetableColor;
                }
            }
            else
            {
                // Normal state
                spriteRenderer.color = originalColor;
            }
        }

        /// <summary>
        /// Select next action and update intent
        /// </summary>
        public void DetermineIntent()
        {
            if (ai != null)
            {
                var selectedSkill = ai.SelectSkill();
                currentIntent.Value = selectedSkill;
            }
        }

        /// <summary>
        /// Execute the current intent
        /// </summary>
        public void ExecuteIntent(BattleEntity target)
        {
            if (currentIntent.Value != null && ai != null)
            {
                ai.ExecuteSkill(currentIntent.Value, target);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Dispose enemy-specific ReactiveProperties
            currentIntent?.Dispose();

            // Clear skills list
            availableSkills?.Clear();
        }
    }
}