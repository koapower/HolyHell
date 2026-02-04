using HolyHell.Battle.Card;
using HolyHell.Battle.Logic;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Heal caster for a percentage of max HP
    /// </summary>
    public class SelfPercHealEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.SelfPercHeal;

        public SelfPercHealEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            float percent = EffectValueParser.ParseFloat(Value);
            int healAmount = GameMath.RoundToInt(context.Caster.maxHp.CurrentValue * percent / 100f);
            DamageCalculator.ApplyHealing(context.Caster, healAmount);
            Debug.Log($"SelfPercHeal: Healed caster for {healAmount} HP ({percent}% of max)");
            return false;
        }

        public override EffectBase Clone()
        {
            return new SelfPercHealEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Heal for {Value}% of max HP";
        }
    }
}
