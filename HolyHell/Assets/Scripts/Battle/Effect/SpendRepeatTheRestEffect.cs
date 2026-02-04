using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Repeat all effects that come after this one in the card's effect list,
    /// spending a meter/resource each iteration until the max value is reached or the resource runs out.
    ///
    /// Parameter format: "meterType, costPerRepeat, maxValue"
    ///   meterType     : Ametervalue | Dmetervalue | ActionPoint
    ///   costPerRepeat : amount spent per repetition
    ///   maxValue      : maximum total amount that can be spent
    ///
    /// Example: "ActionPoint, 1, 3" -> spend 1 AP per repeat, up to 3 AP total (max 3 repeats)
    /// </summary>
    public class SpendRepeatTheRestEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.SpendRepeatTheRest;

        public SpendRepeatTheRestEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            if (!EffectValueParser.ParseSpendRepeatParams(Value, out string meterType, out int costPerRepeat, out int maxValue))
            {
                Debug.LogWarning($"SpendRepeatTheRestEffect: Failed to parse params '{Value}'");
                return false;
            }

            if (context.AllCardEffects == null || context.SpendRepeatIndex < 0)
            {
                Debug.LogWarning("SpendRepeatTheRestEffect: AllCardEffects or SpendRepeatIndex not set in context");
                return false;
            }

            // Collect effects that come after this one
            int startIndex = context.SpendRepeatIndex + 1;
            if (startIndex >= context.AllCardEffects.Count)
            {
                Debug.LogWarning("SpendRepeatTheRestEffect: No effects after SpendRepeatTheRest to repeat");
                return false;
            }

            bool anyKilled = false;
            int totalSpent = 0;

            while (totalSpent + costPerRepeat <= maxValue)
            {
                // Check if we can afford one more iteration
                int currentResource = GetResourceValue(context, meterType);
                if (currentResource < costPerRepeat)
                {
                    Debug.Log($"SpendRepeatTheRest: Not enough {meterType} to repeat (have {currentResource}, need {costPerRepeat})");
                    break;
                }

                // Spend the resource
                SpendResource(context, meterType, costPerRepeat);
                totalSpent += costPerRepeat;
                Debug.Log($"SpendRepeatTheRest: Spent {costPerRepeat} {meterType} (total spent: {totalSpent}/{maxValue})");

                // Execute all effects after this one
                for (int i = startIndex; i < context.AllCardEffects.Count; i++)
                {
                    var effect = context.AllCardEffects[i];
                    if (effect != null && effect.Execute(context))
                    {
                        anyKilled = true;
                    }
                }
            }

            return anyKilled;
        }

        private int GetResourceValue(EffectContext context, string meterType)
        {
            var playerEntity = context.Caster as PlayerEntity;
            switch (meterType.Trim().ToLower())
            {
                case "ametervalue":
                case "ameter":
                    return playerEntity.angelGauge.CurrentValue;
                case "dmetervalue":
                case "dmeter":
                    return playerEntity.demonGauge.CurrentValue;
                case "actionpoint":
                    return playerEntity.actionPoint.CurrentValue;
                default:
                    Debug.LogWarning($"SpendRepeatTheRestEffect: Unknown meter type '{meterType}'");
                    return 0;
            }
        }

        private void SpendResource(EffectContext context, string meterType, int amount)
        {
            var playerEntity = context.Caster as PlayerEntity;
            switch (meterType.Trim().ToLower())
            {
                case "ametervalue":
                case "ameter":
                    playerEntity.angelGauge.Value -= amount;
                    break;
                case "dmetervalue":
                case "dmeter":
                    playerEntity.demonGauge.Value -= amount;
                    break;
                case "actionpoint":
                    playerEntity.actionPoint.Value -= amount;
                    break;
                default:
                    Debug.LogWarning($"SpendRepeatTheRestEffect: Unknown meter type '{meterType}'");
                    break;
            }
        }

        public override EffectBase Clone()
        {
            return new SpendRepeatTheRestEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            if (EffectValueParser.ParseSpendRepeatParams(Value, out string meterType, out int costPerRepeat, out int maxValue))
            {
                return $"Repeat remaining effects by spending {costPerRepeat} {meterType} per repeat (max {maxValue})";
            }
            return $"SpendRepeatTheRest: {Value}";
        }
    }
}
