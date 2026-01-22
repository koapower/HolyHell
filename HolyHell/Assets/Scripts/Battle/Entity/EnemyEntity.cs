using R3;
using System.Collections.Generic;

public class EnemyEntity : BattleEntity
{
    // Enemy data reference
    public EnemyRow enemyData;

    // Current intent (what the enemy will do next turn)
    public ReactiveProperty<MonsterSkillRow> currentIntent = new ReactiveProperty<MonsterSkillRow>();

    // Available skills
    public List<MonsterSkillRow> availableSkills = new List<MonsterSkillRow>();

    // AI reference
    public EnemyAI ai;

    public void Initialize(EnemyRow data, List<MonsterSkillRow> skills)
    {
        enemyData = data;
        availableSkills = skills;

        // Set initial stats
        maxHp.Value = data.Hp;
        hp.Value = data.Hp;
        shield.Value = 0;

        // Initialize AI
        ai = new EnemyAI(this);
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