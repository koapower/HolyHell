using HolyHell.Battle.Card;
using HolyHell.Battle.Effect;
using HolyHell.Battle.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace HolyHell.Battle.Enemy
{
    /// <summary>
    /// Simple AI for enemy decision making
    /// Uses the new Effect system
    /// </summary>
    public class EnemyAI
{
    private EnemyEntity enemy;
    private CardEffectExecutor effectExecutor;

    public EnemyAI(EnemyEntity owner)
    {
        enemy = owner;
        effectExecutor = new CardEffectExecutor(owner);
    }

    /// <summary>
    /// Update battle context (should be called at battle start and when entities change)
    /// </summary>
    public void UpdateBattleContext(List<BattleEntity> enemies, List<BattleEntity> allies)
    {
        effectExecutor.UpdateBattleLists(enemies, allies);
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
    /// Execute a skill on target using the new Effect system
    /// </summary>
    public void ExecuteSkill(MonsterSkillRow skill, BattleEntity target)
    {
        if (skill == null)
        {
            Debug.LogWarning("Skill is null, cannot execute");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("Target is null, cannot execute skill");
            return;
        }

        Debug.Log($"Enemy {enemy.enemyData?.DisplayName} uses {skill.DisplayName}!");

        // Apply base attack modifier to damage effects
        var modifiedEffects = ApplyBaseAttackModifier(skill.Effects);

        // Execute all effects
        effectExecutor.ExecuteEffects(modifiedEffects, target);
    }

    /// <summary>
    /// Apply base attack modifier from enemy data to damage effects
    /// </summary>
    private List<EffectBase> ApplyBaseAttackModifier(List<EffectBase> effects)
    {
        if (enemy.enemyData == null || effects == null)
        {
            return effects;
        }

        float baseAtkMultiplier = enemy.enemyData.BaseAtk / 100f;
        var modifiedEffects = new List<EffectBase>();

        foreach (var effect in effects)
        {
            if (effect == null)
                continue;

            // Clone the effect to avoid modifying the original
            var clonedEffect = effect.Clone();

            // Apply base attack modifier to damage effects
            if (clonedEffect is SingleDamageEffect ||
                clonedEffect is AOEDamageEffect ||
                clonedEffect is SelfDamageEffect ||
                clonedEffect is DelaySingleDamageEffect ||
                clonedEffect is DelayAOEDamageEffect)
            {
                // Parse current value and apply modifier
                int originalDamage = int.Parse(clonedEffect.Value);
                int modifiedDamage = Mathf.RoundToInt(originalDamage * baseAtkMultiplier);
                clonedEffect.Value = modifiedDamage.ToString();
            }

            modifiedEffects.Add(clonedEffect);
        }

        return modifiedEffects;
    }

    /// <summary>
    /// Get CardEffectExecutor for external use (if needed)
    /// </summary>
    public CardEffectExecutor GetEffectExecutor()
    {
        return effectExecutor;
    }
}
}
