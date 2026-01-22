using System.Collections.Generic;
using UnityEngine;

public class BuffHandler
{
    public List<BuffBase> activeBuffs = new List<BuffBase>();

    /// <summary>
    /// Add a buff (or increase stack if already exists)
    /// </summary>
    public void AddBuff(BuffBase buff)
    {
        // Check if buff already exists
        var existingBuff = activeBuffs.Find(b => b.Id == buff.Id);

        if (existingBuff != null)
        {
            // Stack the buff
            existingBuff.StackCount += buff.StackCount;
            Debug.Log($"Buff {buff.Id} stacked to {existingBuff.StackCount}");
        }
        else
        {
            // Add new buff
            activeBuffs.Add(buff);
            Debug.Log($"Buff {buff.Id} added with {buff.StackCount} stacks");
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
        activeBuffs.RemoveAll(b =>
        {
            if (b.Duration <= 0 && b.Duration != -1) // -1 = permanent
            {
                Debug.Log($"Buff {b.Id} expired");
                return true;
            }
            return false;
        });
    }
}