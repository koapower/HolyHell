using System.Collections.Generic;
using System.Linq;
using HolyHell.Battle.Logic.Buffs;
using ObservableCollections;
using UnityEngine;

namespace HolyHell.Battle.Entity
{
    public class BuffHandler
    {
        public ObservableList<BuffBase> activeBuffs = new ObservableList<BuffBase>();
        private BattleEntity owner;

        public BuffHandler(BattleEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Add a buff (or increase stack/refresh duration if already exists)
        /// </summary>
        public void AddBuff(BuffBase buff)
        {
            if (buff == null)
            {
                Debug.LogWarning("Attempted to add null buff");
                return;
            }

            // Set owner reference
            buff.SetOwner(owner);

            // Check if buff already exists
            var existingBuff = activeBuffs.Find(b => b.Id == buff.Id);

            if (existingBuff != null)
            {
                // Check if buff is stackable
                if (existingBuff.IsStackable)
                {
                    // Stack the buff (increase stack count)
                    existingBuff.StackCount.Value += buff.StackCount.Value;
                    Debug.Log($"Buff {buff.Id} stacked to {existingBuff.StackCount}");
                }

                // refresh duration
                existingBuff.Duration.Value = buff.Duration.Value;
                Debug.Log($"Buff {buff.Id} duration refreshed to {buff.Duration}");
            }
            else
            {
                // Add new buff
                activeBuffs.Add(buff);
                buff.OnApplied();
                Debug.Log($"Buff {buff.Id} added with {buff.StackCount} stacks and {buff.Duration} duration");
            }
        }

        /// <summary>
        /// Remove a buff
        /// </summary>
        public void RemoveBuff(string buffId)
        {
            activeBuffs.RemoveAll(b => b.Id == buffId);
            Debug.Log($"Buff {buffId} removed");
        }

        /// <summary>
        /// Get modified damage (outgoing)
        /// </summary>
        public float GetModifiedDamage(float baseDamage)
        {
            float finalDamage = baseDamage;
            foreach (var buff in activeBuffs)
            {
                finalDamage = buff.OnCalculateDamage(finalDamage);
            }
            return finalDamage;
        }

        /// <summary>
        /// Get modified damage (incoming)
        /// </summary>
        public float GetModifiedIncomingDamage(float baseDamage)
        {
            float finalDamage = baseDamage;
            foreach (var buff in activeBuffs)
            {
                finalDamage = buff.OnReceiveDamage(finalDamage);
            }
            return finalDamage;
        }

        /// <summary>
        /// Check if entity has a specific buff
        /// </summary>
        public bool HasBuff(string buffId)
        {
            return activeBuffs.Any(b => b.Id == buffId);
        }

        /// <summary>
        /// Get a specific buff by ID
        /// </summary>
        public BuffBase GetBuff(string buffId)
        {
            return activeBuffs.Find(b => b.Id == buffId);
        }

        /// <summary>
        /// Remove specified number of debuffs (for CleanseSelf effect)
        /// </summary>
        public int RemoveDebuffs(int count)
        {
            var debuffs = activeBuffs.Where(b => !b.IsPositive).ToList();
            int removedCount = 0;

            for (int i = 0; i < count && i < debuffs.Count; i++)
            {
                var debuff = debuffs[i];
                debuff.OnRemoved();
                activeBuffs.Remove(debuff);
                removedCount++;
                Debug.Log($"Debuff {debuff.Id} removed by cleanse");
            }

            return removedCount;
        }

        /// <summary>
        /// Trigger at start of turn - update durations and apply effects
        /// </summary>
        public void OnTurnStart()
        {
            foreach (var buff in activeBuffs)
            {
                buff.OnTurnStart();
            }

            // Remove expired buffs
            RemoveExpiredBuffs();
        }

        /// <summary>
        /// Trigger at end of turn - update durations
        /// </summary>
        public void OnTurnEnd()
        {
            foreach (var buff in activeBuffs)
            {
                buff.OnTurnEnd();
            }

            // Remove expired buffs
            RemoveExpiredBuffs();
        }

        /// <summary>
        /// Remove buffs with 0 or negative duration
        /// </summary>
        private void RemoveExpiredBuffs()
        {
            var expiredBuffs = activeBuffs.Where(b => b.Duration.Value <= 0 && b.Duration.Value != -1).ToList();

            foreach (var buff in expiredBuffs)
            {
                buff.OnRemoved();
                activeBuffs.Remove(buff);
                Debug.Log($"Buff {buff.Id} expired");
            }
        }
    }
}