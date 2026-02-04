using HolyHell.Battle.Card;
using HolyHell.Battle.Logic;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Heal caster for a flat HP amount
    /// </summary>
    public class SelfFlatHealEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.SelfFlatHeal;

        public SelfFlatHealEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int healAmount = EffectValueParser.ParseInt(Value);
            DamageCalculator.ApplyHealing(context.Caster, healAmount);
            Debug.Log($"SelfFlatHeal: Healed caster for {healAmount} HP");
            return false;
        }

        public override EffectBase Clone()
        {
            return new SelfFlatHealEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Heal for {Value} HP";
        }
    }
}
