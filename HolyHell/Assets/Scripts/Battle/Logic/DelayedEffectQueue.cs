using HolyHell.Battle.Entity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HolyHell.Battle.Logic
{
    /// <summary>
    /// Manages delayed effects that trigger after a certain number of turns
    /// </summary>
    public class DelayedEffectQueue
    {
        private List<DelayedEffect> queuedEffects = new List<DelayedEffect>();
        private List<BattleEntity> allEnemies;
        private BattleEntity player;

        public DelayedEffectQueue(BattleEntity player, List<BattleEntity> enemies)
        {
            this.player = player;
            this.allEnemies = enemies;
        }

        /// <summary>
        /// Update enemy list (for dynamic battle scenarios)
        /// </summary>
        public void UpdateEnemyList(List<BattleEntity> enemies)
        {
            allEnemies = enemies;
        }

        /// <summary>
        /// Add a delayed effect to the queue
        /// </summary>
        public void AddDelayedEffect(DelayedEffect effect)
        {
            if (effect == null)
            {
                Debug.LogWarning("Attempted to add null delayed effect");
                return;
            }

            queuedEffects.Add(effect);
            Debug.Log($"Added delayed effect: {effect.Damage} damage in {effect.RemainingTurns} turns ({effect.TargetType})");
        }

        /// <summary>
        /// Process delayed effects at turn start
        /// Decrements turn counters and triggers ready effects
        /// </summary>
        public void OnTurnStart()
        {
            // Decrement all effect timers
            foreach (var effect in queuedEffects)
            {
                effect.DecrementTurn();
            }

            // Get effects that should trigger this turn
            var triggeredEffects = queuedEffects.Where(e => e.ShouldTrigger()).ToList();

            // Execute triggered effects
            foreach (var effect in triggeredEffects)
            {
                ExecuteDelayedEffect(effect);
            }

            // Remove triggered effects from queue
            queuedEffects.RemoveAll(e => e.ShouldTrigger());
        }

        /// <summary>
        /// Execute a delayed effect
        /// </summary>
        private void ExecuteDelayedEffect(DelayedEffect effect)
        {
            if (effect.TargetType == DelayedEffect.EffectTargetType.Single)
            {
                ExecuteSingleTargetEffect(effect);
            }
            else if (effect.TargetType == DelayedEffect.EffectTargetType.AOE)
            {
                ExecuteAOEEffect(effect);
            }
        }

        /// <summary>
        /// Execute delayed single-target damage
        /// </summary>
        private void ExecuteSingleTargetEffect(DelayedEffect effect)
        {
            if (effect.SingleTarget == null || effect.SingleTarget.hp.CurrentValue <= 0)
            {
                Debug.Log("Delayed effect target is dead or null - effect fizzled");
                return;
            }

            Debug.Log($"Delayed effect triggered: {effect.Damage} damage to {effect.SingleTarget.GetType().Name}");

            float finalDamage = DamageCalculator.CalculateDamage(
                effect.Damage,
                effect.Caster,
                effect.SingleTarget,
                effect.ElementType
            );

            DamageCalculator.ApplyDamage(
                effect.SingleTarget,
                effect.Caster,
                Mathf.RoundToInt(finalDamage)
            );
        }

        /// <summary>
        /// Execute delayed AOE damage
        /// </summary>
        private void ExecuteAOEEffect(DelayedEffect effect)
        {
            List<BattleEntity> targets;

            if (effect.TargetEnemies)
            {
                // Target all enemies
                targets = GetAllEnemies();
                Debug.Log($"Delayed AOE effect triggered: {effect.Damage} damage to all enemies");
            }
            else
            {
                // Target player (and allies if they exist)
                targets = new List<BattleEntity> { player };
                Debug.Log($"Delayed AOE effect triggered: {effect.Damage} damage to player");
            }

            foreach (var target in targets)
            {
                if (target != null && target.hp.CurrentValue > 0)
                {
                    float finalDamage = DamageCalculator.CalculateDamage(
                        effect.Damage,
                        effect.Caster,
                        target,
                        effect.ElementType
                    );

                    DamageCalculator.ApplyDamage(
                        target,
                        effect.Caster,
                        Mathf.RoundToInt(finalDamage)
                    );
                }
            }
        }

        /// <summary>
        /// Get all alive enemies
        /// </summary>
        private List<BattleEntity> GetAllEnemies()
        {
            if (allEnemies == null)
            {
                return new List<BattleEntity>();
            }

            return allEnemies.Where(e => e != null && e.hp.CurrentValue > 0).ToList();
        }

        /// <summary>
        /// Clear all queued effects (e.g., at end of battle)
        /// </summary>
        public void Clear()
        {
            queuedEffects.Clear();
        }

        /// <summary>
        /// Get number of queued effects
        /// </summary>
        public int GetQueuedEffectCount()
        {
            return queuedEffects.Count;
        }
    }
}
