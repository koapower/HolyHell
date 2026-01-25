using HolyHell.Battle.Card;
using HolyHell.Battle.Logic;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Deal damage to caster (self-harm)
    /// </summary>
    public class SelfDamageEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.SelfDamage;

        public SelfDamageEffect(string value, string requirement = null)
            : base(value, requirement)
        {
        }

        public override bool Execute(EffectContext context)
        {
            int damage = EffectValueParser.ParseInt(Value);
            bool killed = DamageCalculator.ApplyDamage(context.Caster, context.Caster, damage);
            return killed;
        }

        public override EffectBase Clone()
        {
            return new SelfDamageEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Deal {Value} damage to self";
        }
    }
}
