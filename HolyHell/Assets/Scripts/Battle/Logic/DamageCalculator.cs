using HolyHell.Battle.Entity;
using HolyHell.Battle.Logic.Buffs;
using HolyHell.Data.Type;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HolyHell.Battle.Logic
{
    /// <summary>
    /// Calculates and applies damage / healing with full resistance and buff pipeline.
    ///
    /// Damage pipeline:
    ///   1. baseDamage
    ///   2. × attacker outgoing buff modifiers (BoostDmg, ReduceAtk, …)
    ///   3. - element resistance  (defender base resistance from table + IncreaseResBuff - ReduceResBuff)
    ///   4. × defender incoming buff modifiers (Fragile, Guard, …)
    ///   → clamped to ≥ 0
    ///
    /// All intermediate values are written to Debug.Log for designer review.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// Calculate final damage and return it.
        /// Outputs a full breakdown log each time it is called.
        /// </summary>
        public static float CalculateDamage(
            float baseDamage,
            BattleEntity attacker,
            BattleEntity defender,
            ElementType elementType = ElementType.None)
        {
            float damage = baseDamage;
            string attackerName = attacker != null ? GetEntityName(attacker) : "unknown";
            string defenderName = defender != null ? GetEntityName(defender) : "unknown";

            var log = new StringBuilder();
            log.Append($"[Damage Calc] {attackerName} → {defenderName} | element={elementType} | base={baseDamage}");

            // ── Step 1: attacker outgoing buff modifiers ──────────────────────
            if (attacker?.buffHandler != null)
            {
                float prev = damage;
                damage = attacker.buffHandler.GetModifiedDamage(damage);
                if (damage != prev)
                    log.Append($"  +atk_buffs({damage - prev:+0.##;-0.##})={damage}");
            }

            // ── Step 2: element resistance ────────────────────────────────────
            if (elementType != ElementType.None && defender != null)
            {
                damage = ApplyElementResistance(damage, defender, elementType, log);
            }
            else
            {
                log.Append("  res=0(untyped)");
            }

            // ── Step 3: defender incoming buff modifiers ──────────────────────
            if (defender?.buffHandler != null)
            {
                float prev = damage;
                damage = defender.buffHandler.GetModifiedIncomingDamage(damage);
                if (damage != prev)
                    log.Append($"  +def_buffs({damage - prev:+0.##;-0.##})={damage}");
            }

            float finalDamage = Mathf.Max(0, damage);
            log.Append($"  → FINAL={finalDamage}");
            Debug.Log(log.ToString());

            return finalDamage;
        }

        /// <summary>
        /// Apply all element resistance sources and append details to the log.
        /// Formula: damage -= (baseResistance + allResBuff + elementResBuff)
        /// </summary>
        private static float ApplyElementResistance(
            float damage,
            BattleEntity defender,
            ElementType elementType,
            StringBuilder log)
        {
            float totalResistance = 0f;

            // ── Base resistance from entity data (e.g. EnemyRow.BliRes) ───────
            int baseRes = defender.GetBaseResistance(elementType);
            totalResistance += baseRes;

            // ── AllRes buff bonus (applies to every typed element) ────────────
            float allIncRes = defender.buffHandler.activeBuffs
                .OfType<IncreaseResBuff>()
                .Where(b => b.ElementType == ElementType.All)
                .Sum(b => b.GetResistanceModifier());
            totalResistance += allIncRes;

            float allDecRes = defender.buffHandler.activeBuffs
                .OfType<ReduceResBuff>()
                .Where(b => b.ElementType == ElementType.All)
                .Sum(b => b.GetResistanceModifier());
            totalResistance -= allDecRes;

            // ── Element-specific buff bonus ───────────────────────────────────
            float elemIncRes = defender.buffHandler.activeBuffs
                .OfType<IncreaseResBuff>()
                .Where(b => b.ElementType == elementType)
                .Sum(b => b.GetResistanceModifier());
            totalResistance += elemIncRes;

            float elemDecRes = defender.buffHandler.activeBuffs
                .OfType<ReduceResBuff>()
                .Where(b => b.ElementType == elementType)
                .Sum(b => b.GetResistanceModifier());
            totalResistance -= elemDecRes;

            log.Append($"  res=[base={baseRes} allBuff={allIncRes - allDecRes:+0.##;-0.##;0} elemBuff={elemIncRes - elemDecRes:+0.##;-0.##;0} total={totalResistance}]");

            return damage - totalResistance;
        }

        /// <summary>
        /// Apply final damage to target (shield absorbs first, then HP).
        /// Returns true if the target was killed.
        /// </summary>
        public static bool ApplyDamage(BattleEntity target, BattleEntity attacker, int damage)
        {
            if (target == null)
                return false;

            int damageInt = damage;
            string targetName   = GetEntityName(target);
            string attackerName = attacker != null ? GetEntityName(attacker) : "unknown";

            // Shield absorbs first
            if (target.shield.CurrentValue > 0)
            {
                int shieldDamage = Mathf.Min(target.shield.CurrentValue, damageInt);
                target.shield.Value -= shieldDamage;
                damageInt -= shieldDamage;
                Debug.Log($"[Damage] {targetName} shield absorbed {shieldDamage}  (shield remaining: {target.shield.CurrentValue})");
            }

            // Remaining damage to HP
            if (damageInt > 0)
            {
                int oldHp = target.hp.CurrentValue;
                target.hp.Value = Mathf.Max(0, target.hp.CurrentValue - damageInt);
                Debug.Log($"[Damage] {attackerName} → {targetName}: {damageInt} dmg  (HP {oldHp} → {target.hp.CurrentValue}/{target.maxHp.CurrentValue})");

                // Track damage for Feared buff
                var fearedBuff = target.buffHandler.GetBuff(BuffType.Feared.ToString()) as FearedBuff;
                fearedBuff?.RecordDamage(damageInt);

                // Handle Lifesteal buff on attacker
                if (attacker?.buffHandler != null)
                {
                    var lifesteelBuff = attacker.buffHandler.GetBuff(BuffType.LifeSteal.ToString()) as LifesteelBuff;
                    if (lifesteelBuff != null && !lifesteelBuff.HasTriggered)
                    {
                        ApplyHealing(attacker, damageInt);
                        lifesteelBuff.TriggerLifesteal();
                    }
                }
            }

            bool wasKilled = target.hp.CurrentValue <= 0;
            if (wasKilled)
                Debug.Log($"[Damage] {targetName} has been defeated!");

            return wasKilled;
        }

        /// <summary>
        /// Apply healing to target (capped at max HP, blocked by Cursed).
        /// </summary>
        public static void ApplyHealing(BattleEntity target, int healAmount)
        {
            if (target == null)
                return;

            string targetName = GetEntityName(target);

            if (target.buffHandler != null && target.buffHandler.HasBuff(BuffType.Cursed.ToString()))
            {
                Debug.Log($"[Heal] {targetName} is Cursed – healing nullified!");
                return;
            }

            int actualHeal = Mathf.Min(healAmount, target.maxHp.CurrentValue - target.hp.CurrentValue);
            if (actualHeal > 0)
            {
                int oldHp = target.hp.CurrentValue;
                target.hp.Value += actualHeal;
                Debug.Log($"[Heal] {targetName}: +{actualHeal} HP  (HP {oldHp} → {target.hp.CurrentValue}/{target.maxHp.CurrentValue})");
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string GetEntityName(BattleEntity entity)
        {
            if (entity is HolyHell.Battle.Entity.EnemyEntity enemy && enemy.enemyData != null)
                return enemy.enemyData.DisplayName;
            return entity.name;
        }
    }
}
