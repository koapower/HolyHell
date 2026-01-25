using HolyHell.Battle.Entity;
using HolyHell.Battle.Logic.Buffs;
using HolyHell.Data.Type;
using System.Linq;
using UnityEngine;

namespace HolyHell.Battle.Logic
{
    /// <summary>
    /// Calculates and applies damage with buff modifications
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// Calculate final damage with attacker/defender buff modifications and element resistance
        /// </summary>
        public static float CalculateDamage(float baseDamage, BattleEntity attacker, BattleEntity defender, ElementType elementType = ElementType.None)
        {
            float damage = baseDamage;

            // Apply attacker's buffs (e.g., IncreaseDmg, BoostDmg, ReduceAtk)
            if (attacker != null && attacker.buffHandler != null)
            {
                damage = attacker.buffHandler.GetModifiedDamage(damage);
            }

            // Apply element resistance modifications
            if (defender != null && defender.buffHandler != null)
            {
                damage = ApplyElementResistance(damage, defender, elementType);
            }

            // Apply defender's incoming damage buffs (e.g., Fragile, Guard)
            if (defender != null && defender.buffHandler != null)
            {
                damage = defender.buffHandler.GetModifiedIncomingDamage(damage);
            }

            return Mathf.Max(0, damage); // Damage can't be negative
        }

        /// <summary>
        /// Apply element resistance from buffs
        /// </summary>
        private static float ApplyElementResistance(float damage, BattleEntity defender, ElementType elementType)
        {
            float resistanceModifier = 0f;

            // Check for IncreaseRes buffs (reduce damage)
            var increaseResBuffs = defender.buffHandler.activeBuffs
                .OfType<IncreaseResBuff>()
                .Where(b => b.ElementType == elementType);

            foreach (var buff in increaseResBuffs)
            {
                resistanceModifier -= buff.GetResistanceModifier();
            }

            // Check for ReduceRes buffs (increase damage)
            var reduceResBuffs = defender.buffHandler.activeBuffs
                .OfType<ReduceResBuff>()
                .Where(b => b.ElementType == elementType);

            foreach (var buff in reduceResBuffs)
            {
                resistanceModifier += buff.GetResistanceModifier();
            }

            // Apply resistance modifier
            return damage * (1f + resistanceModifier);
        }

        /// <summary>
        /// Apply damage to target (shield absorbs first, then HP)
        /// Returns true if target was killed
        /// </summary>
        public static bool ApplyDamage(BattleEntity target, BattleEntity attacker, int damage)
        {
            if (target == null)
            {
                return false;
            }

            int damageInt = damage;

            // Shield absorbs damage first
            if (target.shield.CurrentValue > 0)
            {
                int shieldDamage = Mathf.Min(target.shield.CurrentValue, damageInt);
                target.shield.Value -= shieldDamage;
                damageInt -= shieldDamage;

                Debug.Log($"{target.GetType().Name} shield absorbed {shieldDamage} damage");
            }

            // Remaining damage goes to HP
            if (damageInt > 0)
            {
                int oldHp = target.hp.CurrentValue;
                target.hp.Value = Mathf.Max(0, target.hp.CurrentValue - damageInt);
                Debug.Log($"{target.GetType().Name} took {damageInt} HP damage (HP: {target.hp.CurrentValue}/{target.maxHp.CurrentValue})");

                // Track damage for Feared buff
                var fearedBuff = target.buffHandler.GetBuff(BuffType.Feared.ToString()) as FearedBuff;
                if (fearedBuff != null)
                {
                    fearedBuff.RecordDamage(damageInt);
                }

                // Handle Lifesteel buff on attacker
                if (attacker != null && attacker.buffHandler != null)
                {
                    var lifesteelBuff = attacker.buffHandler.GetBuff(BuffType.Lifesteel.ToString()) as LifesteelBuff;
                    if (lifesteelBuff != null && !lifesteelBuff.HasTriggered)
                    {
                        ApplyHealing(attacker, damageInt);
                        lifesteelBuff.TriggerLifesteal();
                    }
                }
            }

            // Check for death
            bool wasKilled = target.hp.CurrentValue <= 0;
            if (wasKilled)
            {
                Debug.Log($"{target.GetType().Name} has been defeated!");
            }

            return wasKilled;
        }

        /// <summary>
        /// Apply healing to target (can't exceed max HP)
        /// Checks for Cursed buff
        /// </summary>
        public static void ApplyHealing(BattleEntity target, int healAmount)
        {
            if (target == null)
            {
                return;
            }

            // Check for Cursed buff (nullifies all healing)
            if (target.buffHandler != null && target.buffHandler.HasBuff(BuffType.Cursed.ToString()))
            {
                Debug.Log($"{target.GetType().Name} is Cursed - healing nullified!");
                return;
            }

            int actualHeal = Mathf.Min(healAmount, target.maxHp.CurrentValue - target.hp.CurrentValue);

            if (actualHeal > 0)
            {
                target.hp.Value += actualHeal;
                Debug.Log($"{target.GetType().Name} healed for {actualHeal} HP (HP: {target.hp.CurrentValue}/{target.maxHp.CurrentValue})");
            }
        }
    }
}
