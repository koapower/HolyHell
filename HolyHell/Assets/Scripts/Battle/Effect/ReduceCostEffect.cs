using HolyHell.Battle.Card;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Reduce the action cost of the current card being played
    /// </summary>
    public class ReduceCostEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.ReduceCost;

        public ReduceCostEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int reduction = EffectValueParser.ParseInt(Value);

            if (context.CurrentCard != null)
            {
                context.CurrentCard.ActionCost = System.Math.Max(0, context.CurrentCard.ActionCost - reduction);
                Debug.Log($"ReduceCost: Reduced current card cost by {reduction} (now {context.CurrentCard.ActionCost})");
            }
            else
            {
                Debug.LogWarning("ReduceCostEffect: CurrentCard is null");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new ReduceCostEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Reduce this card's cost by {Value}";
        }
    }
}
