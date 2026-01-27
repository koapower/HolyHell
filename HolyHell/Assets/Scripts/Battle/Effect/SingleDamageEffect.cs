using HolyHell.Battle.Card;
using HolyHell.Battle.Logic;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Deal damage to a single target
    /// </summary>
    public class SingleDamageEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.SingleDamage;

        public SingleDamageEffect(string value, string requirement = null)
            : base(value, requirement)
        {
        }

        public override bool Execute(EffectContext context)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("SingleDamageEffect: Target is null");
                return false;
            }

            int baseDamage = EffectValueParser.ParseInt(Value);
            float finalDamage = DamageCalculator.CalculateDamage(baseDamage, context.Caster, context.Target);
            bool killed = DamageCalculator.ApplyDamage(context.Target, context.Caster, GameMath.RoundToInt(finalDamage));

            return killed;
        }

        public override EffectBase Clone()
        {
            return new SingleDamageEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Deal {Value} damage to target";
        }
    }
}
