using HolyHell.Battle.Card;
using HolyHell.Battle.Effect;
using HolyHell.Battle.Entity;
using System.Collections.Generic;
using UnityEngine;

namespace HolyHell.Battle.Enemy
{
    /// <summary>
    /// Enemy AI driven by EnemyBehavior.csv data.
    ///
    /// Skill selection algorithm:
    ///   1. Each skill slot starts with its BaseWeight from the CSV ("SkillID,BaseWeight").
    ///   2. Each Condition/Result pair is evaluated; when the condition is true, the
    ///      referenced skill slot receives an additional BonusWeight ("SkillN,BonusWeight").
    ///   3. A weighted random draw picks the final skill from all slots with weight > 0.
    ///
    /// Intent phase vs Execute phase:
    ///   - SelectSkill()  : called during DetermineIntent (end of previous turn / battle start)
    ///   - ExecuteSkill() : called during ExecuteIntent (enemy's action turn)
    /// </summary>
    public class EnemyAI
    {
        private const int MAX_SKILL_SLOTS = 6;
        private const int MAX_CONDITION_SLOTS = 4;

        private EnemyEntity enemy;
        private CardEffectExecutor effectExecutor;
        private IBattleManager battleManager;

        // Runtime state tracked per combat
        private int turnNumber = 0;
        private int castCountThisCombat = 0;

        // Cache: skill slot index (1-based) -> EnemySkill resolved from MonsterSkill table
        private Dictionary<int, EnemySkill> slotToSkill = new Dictionary<int, EnemySkill>();

        public EnemyAI(IBattleManager battleManager, EnemyEntity owner)
        {
            this.battleManager = battleManager;
            enemy = owner;
            effectExecutor = new CardEffectExecutor(battleManager, owner);
        }

        /// <summary>
        /// Cache skills resolved from the behavior row (called once after initialization).
        /// </summary>
        public void CacheSkillSlots(EnemyBehaviorRow behaviorRow, List<EnemySkill> resolvedSkills)
        {
            slotToSkill.Clear();

            // Build a lookup from skill ID -> EnemySkill
            var idToSkill = new Dictionary<string, EnemySkill>();
            foreach (var s in resolvedSkills)
            {
                if (s?.DataRow?.Id != null)
                    idToSkill[s.DataRow.Id] = s;
            }

            for (int i = 1; i <= MAX_SKILL_SLOTS; i++)
            {
                var (skillId, _) = behaviorRow.GetSkillEntry(i);
                if (!string.IsNullOrEmpty(skillId) && idToSkill.TryGetValue(skillId, out var skill))
                    slotToSkill[i] = skill;
            }
        }

        /// <summary>
        /// Increment the internal turn counter (call each time enemy's turn starts).
        /// </summary>
        public void OnTurnStart()
        {
            turnNumber++;
        }

        /// <summary>
        /// Select which skill to use next turn using condition-weighted random.
        /// Requires the enemy's EnemyBehaviorRow to be set on the EnemyEntity.
        /// </summary>
        public EnemySkill SelectSkill()
        {
            if (enemy.behaviorRow == null)
            {
                Debug.LogWarning($"[EnemyAI] {enemy.enemyData?.DisplayName} has no behaviorRow – falling back to random");
                return FallbackRandomSkill();
            }

            var behavior = enemy.behaviorRow;

            // --- Step 1: collect base weights for each skill slot ---
            var weights = new float[MAX_SKILL_SLOTS + 1]; // 1-indexed
            for (int i = 1; i <= MAX_SKILL_SLOTS; i++)
            {
                var (_, baseWeight) = behavior.GetSkillEntry(i);
                if (slotToSkill.ContainsKey(i))
                    weights[i] = baseWeight;
            }

            // --- Step 2: apply condition bonuses ---
            // Use player as the primary target for condition evaluation
            BattleEntity targetForConditions = battleManager.Enemies.Count > 0
                ? (BattleEntity)null   // will be resolved below
                : null;

            // Resolve a sensible "target" entity for condition checks
            // (same player reference used during execution)
            BattleEntity condTarget = null;
            foreach (var ally in battleManager.Allies)
            {
                if (ally != null && ally.hp.Value > 0) { condTarget = ally; break; }
            }

            for (int c = 1; c <= MAX_CONDITION_SLOTS; c++)
            {
                var (condStr, resultStr) = behavior.GetConditionEntry(c);
                if (string.IsNullOrEmpty(condStr) || string.IsNullOrEmpty(resultStr))
                    continue;

                bool condMet = EnemyBehaviorCondition.Evaluate(
                    condStr, enemy, condTarget, battleManager, turnNumber, castCountThisCombat);

                if (!condMet) continue;

                // Parse result: "SkillN,BonusWeight"
                int resultComma = resultStr.IndexOf(',');
                if (resultComma < 0) continue;

                string slotStr = resultStr.Substring(0, resultComma).Trim(); // e.g. "Skill2"
                string bonusStr = resultStr.Substring(resultComma + 1).Trim();

                if (!slotStr.StartsWith("Skill")) continue;
                if (!int.TryParse(slotStr.Substring(5), out int slotIdx)) continue;
                if (!float.TryParse(bonusStr, out float bonus)) continue;

                if (slotIdx >= 1 && slotIdx <= MAX_SKILL_SLOTS)
                    weights[slotIdx] += bonus;
            }

            // --- Step 3: weighted random draw ---
            float totalWeight = 0f;
            for (int i = 1; i <= MAX_SKILL_SLOTS; i++)
                if (slotToSkill.ContainsKey(i)) totalWeight += weights[i];

            if (totalWeight <= 0f)
            {
                Debug.LogWarning($"[EnemyAI] {enemy.enemyData?.DisplayName}: all skill weights are 0, falling back to random");
                return FallbackRandomSkill();
            }

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            EnemySkill chosen = null;
            int chosenSlot = -1;
            for (int i = 1; i <= MAX_SKILL_SLOTS; i++)
            {
                if (!slotToSkill.ContainsKey(i)) continue;
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    chosen = slotToSkill[i];
                    chosenSlot = i;
                    break;
                }
            }

            // Fallback in case of floating point edge case
            if (chosen == null)
            {
                for (int i = MAX_SKILL_SLOTS; i >= 1; i--)
                {
                    if (slotToSkill.TryGetValue(i, out chosen)) { chosenSlot = i; break; }
                }
            }

            // Build a summary of active condition bonuses for the log (designer-friendly)
            var conditionLog = BuildConditionLog(behavior, condTarget);

            Debug.Log($"[EnemyAI] {enemy.enemyData?.DisplayName} → intends Skill{chosenSlot} [{chosen?.DataRow?.DisplayName}] " +
                      $"(roll={roll:F1}/{totalWeight:F1}){conditionLog}");

            return chosen;
        }

        /// <summary>
        /// Execute a previously selected skill against the target.
        /// </summary>
        public void ExecuteSkill(EnemySkill skill, BattleEntity target)
        {
            if (skill == null)
            {
                Debug.LogWarning($"[EnemyAI] {enemy.enemyData?.DisplayName}: skill is null, skipping turn");
                return;
            }

            if (target == null)
            {
                Debug.LogWarning($"[EnemyAI] {enemy.enemyData?.DisplayName}: no target, skipping turn");
                return;
            }

            castCountThisCombat++;

            Debug.Log($"[EnemyAI] {enemy.enemyData?.DisplayName} executes [{skill.DataRow.DisplayName}] " +
                      $"on [{target.name}]  (cast #{castCountThisCombat})");

            var modifiedEffects = ApplyBaseAttackModifier(skill.DataRow.Effects);
            effectExecutor.ExecuteEffects(modifiedEffects, target);
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private EnemySkill FallbackRandomSkill()
        {
            if (enemy.availableSkills == null || enemy.availableSkills.Count == 0)
            {
                Debug.LogWarning($"[EnemyAI] {enemy.enemyData?.Id} has no available skills!");
                return null;
            }
            return enemy.availableSkills[Random.Range(0, enemy.availableSkills.Count)];
        }

        /// <summary>
        /// Build a compact log string listing which conditions were satisfied (designer debug).
        /// </summary>
        private string BuildConditionLog(EnemyBehaviorRow behavior, BattleEntity condTarget)
        {
            var sb = new System.Text.StringBuilder();
            for (int c = 1; c <= MAX_CONDITION_SLOTS; c++)
            {
                var (condStr, resultStr) = behavior.GetConditionEntry(c);
                if (string.IsNullOrEmpty(condStr)) continue;

                bool met = EnemyBehaviorCondition.Evaluate(
                    condStr, enemy, condTarget, battleManager, turnNumber, castCountThisCombat);

                sb.Append($"\n  Cond{c} [{condStr}] = {(met ? "TRUE → +" + resultStr : "false")}");
            }
            return sb.Length > 0 ? sb.ToString() : string.Empty;
        }

        private List<EffectBase> ApplyBaseAttackModifier(List<EffectBase> effects)
        {
            if (enemy.enemyData == null || effects == null)
                return effects;

            var modifiedEffects = new List<EffectBase>();

            foreach (var effect in effects)
            {
                if (effect == null) continue;

                var cloned = effect.Clone();

                if (cloned is SingleDamageEffect ||
                    cloned is AOEDamageEffect ||
                    cloned is SelfDamageEffect ||
                    cloned is DelaySingleDamageEffect ||
                    cloned is DelayAOEDamageEffect)
                {
                    if (float.TryParse(cloned.Value, out float original))
                    {
                        int modified = GameMath.RoundToInt(original * enemy.enemyData.BaseAtk);
                        cloned.Value = modified.ToString();
                    }
                }

                modifiedEffects.Add(cloned);
            }

            return modifiedEffects;
        }

        /// <summary>
        /// Get CardEffectExecutor for external use.
        /// </summary>
        public CardEffectExecutor GetEffectExecutor() => effectExecutor;
    }
}
