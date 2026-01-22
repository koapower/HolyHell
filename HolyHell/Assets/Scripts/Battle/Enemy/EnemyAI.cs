using UnityEngine;

/// <summary>
/// Simple AI for enemy decision making
/// </summary>
public class EnemyAI
{
    private EnemyEntity enemy;
    private DamageCalculator damageCalculator;

    public EnemyAI(EnemyEntity owner)
    {
        enemy = owner;
        damageCalculator = new DamageCalculator();
    }

    /// <summary>
    /// Select a random skill from available skills
    /// </summary>
    public MonsterSkillRow SelectSkill()
    {
        if (enemy.availableSkills == null || enemy.availableSkills.Count == 0)
        {
            Debug.LogWarning($"Enemy {enemy.enemyData?.Id} has no available skills!");
            return null;
        }

        // Simple random selection
        int randomIndex = Random.Range(0, enemy.availableSkills.Count);
        var selectedSkill = enemy.availableSkills[randomIndex];

        Debug.Log($"Enemy {enemy.enemyData?.DisplayName} selected skill: {selectedSkill.DisplayName}");

        return selectedSkill;
    }

    /// <summary>
    /// Execute a skill on target
    /// </summary>
    public void ExecuteSkill(MonsterSkillRow skill, BattleEntity target)
    {
        if (skill == null)
        {
            Debug.LogWarning("Skill is null, cannot execute");
            return;
        }

        Debug.Log($"Enemy {enemy.enemyData?.DisplayName} uses {skill.DisplayName}!");

        // Execute up to 3 effects
        for (int i = 1; i <= 3; i++)
        {
            ExecuteEffect(skill, target, i);
        }
    }

    /// <summary>
    /// Execute a specific effect from skill
    /// </summary>
    private void ExecuteEffect(MonsterSkillRow skill, BattleEntity target, int effectIndex)
    {
        var effectType = skill.GetEffectType(effectIndex);
        if (effectType == CardEffectType.None)
        {
            return;
        }

        float value = skill.GetEffectValue(effectIndex);

        switch (effectType)
        {
            case CardEffectType.SingleDamage:
                ExecuteDamage(target, value);
                break;

            //case CardEffectType.Shield:
            //    ExecuteShield(value);
            //    break;

            //case CardEffectType.Buff:
            //    // TODO: Implement buff application
            //    Debug.Log($"Enemy buff not yet implemented: {value}");
            //    break;

            //case CardEffectType.Debuff:
            //    // TODO: Implement debuff application
            //    Debug.Log($"Enemy debuff not yet implemented: {value}");
            //    break;

            //case CardEffectType.Heal:
            //    ExecuteHeal(value);
            //    break;

            default:
                Debug.LogWarning($"Enemy effect type not supported: {effectType}");
                break;
        }
    }

    private void ExecuteDamage(BattleEntity target, float baseDamage)
    {
        if (target == null)
        {
            Debug.LogWarning("Target is null for damage");
            return;
        }

        // Apply base attack modifier from enemy data
        float modifiedDamage = baseDamage;
        if (enemy.enemyData != null)
        {
            modifiedDamage = baseDamage * (enemy.enemyData.BaseAtk / 100f);
        }

        float finalDamage = damageCalculator.CalculateDamage(modifiedDamage, enemy, target);
        damageCalculator.ApplyDamage(target, finalDamage);
    }

    private void ExecuteShield(float shieldAmount)
    {
        enemy.shield.Value += (int)shieldAmount;
        Debug.Log($"Enemy gained {shieldAmount} shield");
    }

    private void ExecuteHeal(float healAmount)
    {
        int newHp = Mathf.Min(enemy.hp.Value + (int)healAmount, enemy.maxHp.Value);
        enemy.hp.Value = newHp;
        Debug.Log($"Enemy healed for {healAmount} HP");
    }
}
