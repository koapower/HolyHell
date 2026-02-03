using R3;
using System.Collections.Generic;
using HolyHell.Battle.Enemy;
using UnityEngine;

namespace HolyHell.Battle.Entity
{
    public class EnemyEntity : BattleEntity
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        // Enemy data reference
        public EnemyRow enemyData;

        // Current intent (what the enemy will do next turn)
        public ReactiveProperty<EnemySkill> currentIntent = new ReactiveProperty<EnemySkill>();

        // Available skills
        public List<EnemySkill> availableSkills = new List<EnemySkill>();

        // AI reference
        public EnemyAI ai;

        // Visual feedback
        private Color originalColor;
        private Color hoverColor = new Color(1f, 1f, 0.8f, 1f);
        private Color targetableColor = new Color(0.8f, 1f, 0.8f, 1f);
        private bool isTargetable = false;
        private bool isHovered = false;

        public void Initialize(EnemyRow data, List<EnemySkill> skills)
        {
            enemyData = data;
            availableSkills = skills;

            // Set initial stats
            maxHp.Value = data.Hp;
            hp.Value = data.Hp;
            shield.Value = 0;

            // Initialize AI
            ai = new EnemyAI(this);

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        public Vector3 GetTargetWorldPosition()
        {
            return spriteRenderer != null ? spriteRenderer.transform.position : Vector3.zero;
        }

        /// <summary>
        /// Set whether this enemy can be targeted (visual feedback)
        /// </summary>
        public void SetTargetable(bool targetable)
        {
            isTargetable = targetable;
            UpdateVisual();
        }

        /// <summary>
        /// Set whether mouse is hovering over this enemy
        /// </summary>
        public void SetHovered(bool hovered)
        {
            isHovered = hovered;
            UpdateVisual();
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
            else if (isHovered && isTargetable)
            {
                spriteRenderer.color = hoverColor;
            }
            else if (isTargetable)
            {
                spriteRenderer.color = targetableColor;
            }
            else
            {
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