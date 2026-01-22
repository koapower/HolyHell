using UnityEngine;

/// <summary>
/// Calculates and applies damage with buff modifications
/// </summary>
public class DamageCalculator
{
    /// <summary>
    /// Calculate final damage with attacker/defender buff modifications
    /// </summary>
    public float CalculateDamage(float baseDamage, BattleEntity attacker, BattleEntity defender)
    {
        float damage = baseDamage;

        // Apply attacker's buffs (e.g., Strength increases damage)
        if (attacker != null && attacker.buffHandler != null)
        {
            damage = attacker.buffHandler.GetModifiedDamage(damage);
        }

        // Apply defender's buffs (e.g., Vulnerable increases damage taken)
        // TODO: Add defender damage modification when buff system is expanded

        return Mathf.Max(0, damage); // Damage can't be negative
    }

    /// <summary>
    /// Apply damage to target (shield absorbs first, then HP)
    /// </summary>
    public void ApplyDamage(BattleEntity target, float damage)
    {
        if (target == null)
        {
            return;
        }

        int damageInt = Mathf.RoundToInt(damage);

        // Shield absorbs damage first
        if (target.shield.Value > 0)
        {
            int shieldDamage = Mathf.Min(target.shield.Value, damageInt);
            target.shield.Value -= shieldDamage;
            damageInt -= shieldDamage;

            Debug.Log($"{target.GetType().Name} shield absorbed {shieldDamage} damage");
        }

        // Remaining damage goes to HP
        if (damageInt > 0)
        {
            target.hp.Value = Mathf.Max(0, target.hp.Value - damageInt);
            Debug.Log($"{target.GetType().Name} took {damageInt} HP damage (HP: {target.hp.Value}/{target.maxHp.Value})");
        }

        // Check for death
        if (target.hp.Value <= 0)
        {
            Debug.Log($"{target.GetType().Name} has been defeated!");
        }
    }

    /// <summary>
    /// Apply healing to target (can't exceed max HP)
    /// </summary>
    public void ApplyHealing(BattleEntity target, float healAmount)
    {
        if (target == null)
        {
            return;
        }

        int healInt = Mathf.RoundToInt(healAmount);
        int actualHeal = Mathf.Min(healInt, target.maxHp.Value - target.hp.Value);

        target.hp.Value += actualHeal;

        Debug.Log($"{target.GetType().Name} healed for {actualHeal} HP (HP: {target.hp.Value}/{target.maxHp.Value})");
    }
}
