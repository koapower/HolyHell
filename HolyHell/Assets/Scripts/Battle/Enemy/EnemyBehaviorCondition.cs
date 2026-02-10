using HolyHell.Battle.Entity;
using UnityEngine;

namespace HolyHell.Battle.Enemy
{
    /// <summary>
    /// Evaluates a single condition string from EnemyBehavior.csv against battle state.
    ///
    /// Condition format: "ConditionType,Value"
    ///   e.g. "SelfHP<=,50"   "NoBuff,Guard"   "Turnpassed,5"
    ///
    /// Supported types (from README_Condition.csv):
    ///   SelfHP<=         SelfHP>=
    ///   SelfBuffCount<=  SelfBuffCount>=
    ///   HasBuff          NoBuff
    ///   TargetHP<=       TargetHP>=
    ///   TargetBuffCount<= TargetBuffCount>=
    ///   TargetAmeterVal<= TargetAmeterVal>=
    ///   TargetDmeterVal<= TargetDmeterVal>=
    ///   Turnpassed
    ///   Casted
    ///   EnemyCount>=     EnemyCount<=
    /// </summary>
    public static class EnemyBehaviorCondition
    {
        /// <summary>
        /// Evaluate a raw condition string.
        /// Returns true if the condition is satisfied, false otherwise.
        /// An empty/null condition string always returns false.
        /// </summary>
        public static bool Evaluate(
            string conditionRaw,
            EnemyEntity self,
            BattleEntity target,
            IBattleManager battleManager,
            int turnNumber,
            int castCountThisCombat)
        {
            if (string.IsNullOrWhiteSpace(conditionRaw))
                return false;

            // Split on the first comma → "ConditionType" and "Value"
            int commaIdx = conditionRaw.IndexOf(',');
            if (commaIdx < 0)
            {
                Debug.LogWarning($"[EnemyAI] Malformed condition (no comma): '{conditionRaw}'");
                return false;
            }

            string condType = conditionRaw.Substring(0, commaIdx).Trim();
            string valueStr = conditionRaw.Substring(commaIdx + 1).Trim();

            switch (condType)
            {
                // ---- Self HP ----
                case "SelfHP<=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    float hpPct = self.maxHp.Value > 0
                        ? (float)self.hp.Value / self.maxHp.Value * 100f
                        : 0f;
                    return hpPct <= threshold;
                }
                case "SelfHP>=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    float hpPct = self.maxHp.Value > 0
                        ? (float)self.hp.Value / self.maxHp.Value * 100f
                        : 0f;
                    return hpPct >= threshold;
                }

                // ---- Self buff count ----
                case "SelfBuffCount<=":
                {
                    if (!int.TryParse(valueStr, out int threshold)) return false;
                    return self.buffHandler.activeBuffs.Count <= threshold;
                }
                case "SelfBuffCount>=":
                {
                    if (!int.TryParse(valueStr, out int threshold)) return false;
                    return self.buffHandler.activeBuffs.Count >= threshold;
                }

                // ---- Self buff presence ----
                case "HasBuff":
                    return self.buffHandler.HasBuff(valueStr);

                case "NoBuff":
                    return !self.buffHandler.HasBuff(valueStr);

                // ---- Target HP ----
                case "TargetHP<=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    float hpPct = target.maxHp.Value > 0
                        ? (float)target.hp.Value / target.maxHp.Value * 100f
                        : 0f;
                    return hpPct <= threshold;
                }
                case "TargetHP>=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    float hpPct = target.maxHp.Value > 0
                        ? (float)target.hp.Value / target.maxHp.Value * 100f
                        : 0f;
                    return hpPct >= threshold;
                }

                // ---- Target buff count ----
                case "TargetBuffCount<=":
                {
                    if (!int.TryParse(valueStr, out int threshold)) return false;
                    return target.buffHandler.activeBuffs.Count <= threshold;
                }
                case "TargetBuffCount>=":
                {
                    if (!int.TryParse(valueStr, out int threshold)) return false;
                    return target.buffHandler.activeBuffs.Count >= threshold;
                }

                // ---- Target gauge meters (PlayerEntity only) ----
                case "TagetAmeterVal<=":
                case "TargetAmeterVal<=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    if (target is HolyHell.Battle.Entity.PlayerEntity player)
                        return (float)player.angelGauge.Value <= threshold;
                    return false;
                }
                case "TagetAmeterVal>=":
                case "TargetAmeterVal>=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    if (target is HolyHell.Battle.Entity.PlayerEntity player)
                        return (float)player.angelGauge.Value >= threshold;
                    return false;
                }
                case "TagetDmeterVal<=":
                case "TargetDmeterVal<=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    if (target is HolyHell.Battle.Entity.PlayerEntity player)
                        return (float)player.demonGauge.Value <= threshold;
                    return false;
                }
                case "TagetDmeterVal>=":
                case "TargetDmeterVal>=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    if (target is HolyHell.Battle.Entity.PlayerEntity player)
                        return (float)player.demonGauge.Value >= threshold;
                    return false;
                }

                // ---- Turn counter ----
                case "Turnpassed":
                {
                    if (!int.TryParse(valueStr, out int threshold)) return false;
                    return turnNumber >= threshold;
                }

                // ---- Cast count ----
                case "Casted":
                {
                    if (!int.TryParse(valueStr, out int threshold)) return false;
                    return castCountThisCombat >= threshold;
                }

                // ---- Enemy count on field ----
                case "EnemyCount>=":
                {
                    if (!int.TryParse(valueStr, out int threshold)) return false;
                    return CountLivingEnemies(battleManager) >= threshold;
                }
                case "EnemyCount<=":
                {
                    if (!int.TryParse(valueStr, out int threshold)) return false;
                    return CountLivingEnemies(battleManager) <= threshold;
                }

                default:
                    Debug.LogWarning($"[EnemyAI] Unknown condition type: '{condType}'");
                    return false;
            }
        }

        private static int CountLivingEnemies(IBattleManager battleManager)
        {
            int count = 0;
            foreach (var e in battleManager.Enemies)
            {
                if (e.hp.Value > 0) count++;
            }
            return count;
        }
    }
}
