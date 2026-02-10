using HolyHell.Battle.Entity;
using System;
using UnityEngine;

namespace HolyHell.Battle.Enemy
{
    /// <summary>
    /// Evaluates condition expressions from EnemyBehavior.csv.
    ///
    /// Single condition format: "ConditionType,Value"
    ///   e.g. "SelfHP<=,50"   "NoBuff,Guard"   "Turnpassed,5"
    ///
    /// Compound expressions (AND / OR):
    ///   Supported operators: &  &&  AND  |  ||  OR   (case-insensitive, spaces trimmed)
    ///   Mixing AND and OR in the same expression is NOT supported and will produce a Debug.LogError.
    ///   Use multiple Condition slots instead.
    ///   e.g.  "SelfHP<=,50 & NoBuff,Guard"
    ///         "SelfHP<=,30 | TargetHP<=,20"
    ///
    /// Supported condition types (from README_Condition.csv):
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
        // Separator tokens recognized as AND / OR operators
        private static readonly string[] AND_TOKENS = { "&&", "&", "AND" };
        private static readonly string[] OR_TOKENS  = { "||", "|", "OR"  };

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Evaluate a condition expression that may contain AND / OR operators.
        /// Call this everywhere instead of Evaluate() directly.
        /// </summary>
        public static bool EvaluateExpression(
            string conditionRaw,
            EnemyEntity self,
            BattleEntity target,
            IBattleManager battleManager,
            int turnNumber,
            int castCountThisCombat)
        {
            if (string.IsNullOrWhiteSpace(conditionRaw))
                return false;

            string expr = conditionRaw.Trim();

            // ── Detect operator type ──────────────────────────────────────────
            bool hasAnd = ContainsOperator(expr, AND_TOKENS, out string andSep);
            bool hasOr  = ContainsOperator(expr, OR_TOKENS,  out string orSep);

            if (hasAnd && hasOr)
            {
                Debug.LogError($"[EnemyAI] Condition expression mixes AND and OR without parentheses: '{conditionRaw}'" +
                               "\n  Split into multiple Condition slots or add parentheses support.");
                // Evaluate as single condition to avoid silent failure
                return Evaluate(expr, self, target, battleManager, turnNumber, castCountThisCombat);
            }

            if (hasAnd)
            {
                // ALL sub-conditions must be true
                string[] parts = SplitByOperator(expr, andSep);
                foreach (var part in parts)
                {
                    if (!Evaluate(part.Trim(), self, target, battleManager, turnNumber, castCountThisCombat))
                        return false;
                }
                return true;
            }

            if (hasOr)
            {
                // ANY sub-condition must be true
                string[] parts = SplitByOperator(expr, orSep);
                foreach (var part in parts)
                {
                    if (Evaluate(part.Trim(), self, target, battleManager, turnNumber, castCountThisCombat))
                        return true;
                }
                return false;
            }

            // Single condition
            return Evaluate(expr, self, target, battleManager, turnNumber, castCountThisCombat);
        }

        // ── Single-condition evaluator ──────────────────────────────────────────

        /// <summary>
        /// Evaluate a single atomic condition string "ConditionType,Value".
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
                    if (target is PlayerEntity playerA)
                        return (float)playerA.angelGauge.Value <= threshold;
                    return false;
                }
                case "TagetAmeterVal>=":
                case "TargetAmeterVal>=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    if (target is PlayerEntity playerA)
                        return (float)playerA.angelGauge.Value >= threshold;
                    return false;
                }
                case "TagetDmeterVal<=":
                case "TargetDmeterVal<=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    if (target is PlayerEntity playerD)
                        return (float)playerD.demonGauge.Value <= threshold;
                    return false;
                }
                case "TagetDmeterVal>=":
                case "TargetDmeterVal>=":
                {
                    if (!float.TryParse(valueStr, out float threshold)) return false;
                    if (target is PlayerEntity playerD)
                        return (float)playerD.demonGauge.Value >= threshold;
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

        // ── Helpers ────────────────────────────────────────────────────────────

        private static int CountLivingEnemies(IBattleManager battleManager)
        {
            int count = 0;
            foreach (var e in battleManager.Enemies)
                if (e.hp.Value > 0) count++;
            return count;
        }

        /// <summary>
        /// Check if the expression contains any of the given operator tokens (word-boundary aware for AND/OR).
        /// Sets matchedSep to the first found separator string.
        /// </summary>
        private static bool ContainsOperator(string expr, string[] tokens, out string matchedSep)
        {
            foreach (var token in tokens)
            {
                // For word-form operators (AND/OR) require surrounding whitespace or expression boundary
                if (token.Length > 1 && char.IsLetter(token[0]))
                {
                    // Case-insensitive word search surrounded by whitespace
                    int idx = expr.IndexOf(token, StringComparison.OrdinalIgnoreCase);
                    while (idx >= 0)
                    {
                        bool leftOk  = idx == 0 || char.IsWhiteSpace(expr[idx - 1]);
                        bool rightOk = idx + token.Length >= expr.Length || char.IsWhiteSpace(expr[idx + token.Length]);
                        if (leftOk && rightOk) { matchedSep = token; return true; }
                        idx = expr.IndexOf(token, idx + 1, StringComparison.OrdinalIgnoreCase);
                    }
                }
                else
                {
                    if (expr.Contains(token)) { matchedSep = token; return true; }
                }
            }
            matchedSep = null;
            return false;
        }

        /// <summary>
        /// Split expression by the given separator, case-insensitive for word operators.
        /// </summary>
        private static string[] SplitByOperator(string expr, string sep)
        {
            if (sep.Length > 1 && char.IsLetter(sep[0]))
            {
                // Word operator: split case-insensitively (rebuild around whitespace)
                // Normalize: replace " AND " / " OR " with a sentinel then split
                string sentinel = "\x01";
                // Replace all case-variants surrounded by whitespace
                string normalized = System.Text.RegularExpressions.Regex.Replace(
                    expr, $@"\s+{System.Text.RegularExpressions.Regex.Escape(sep)}\s+",
                    sentinel, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                return normalized.Split(new[] { sentinel }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                return expr.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
