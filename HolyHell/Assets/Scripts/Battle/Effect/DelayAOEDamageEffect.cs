using HolyHell.Battle.Card;
using HolyHell.Battle.Logic;
using HolyHell.Data.Type;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    public class DelayAOEDamageEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.DelayAOEDamage;

        public DelayAOEDamageEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            if (context.DelayedEffectQueue == null)
            {
                Debug.LogWarning("DelayAOEDamageEffect: DelayedEffectQueue is null");
                return false;
            }

            if (EffectValueParser.ParseDelayParams(Value, out int damage, out int delay))
            {
                var delayedEffect = new DelayedEffect(
                    DelayedEffect.EffectTargetType.AOE,
                    context.Caster,
                    damage,
                    delay,
                    ElementType.None,
                    null,
                    true // Target enemies
                );

                context.DelayedEffectQueue.AddDelayedEffect(delayedEffect);
                Debug.Log($"DelayAOEDamage queued: {damage} damage to all enemies after {delay} turns");
            }
            else
            {
                Debug.LogWarning($"DelayAOEDamageEffect: Failed to parse parameters '{Value}'");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new DelayAOEDamageEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            if (EffectValueParser.ParseDelayParams(Value, out int damage, out int delay))
            {
                return $"Deal {damage} damage to all enemies after {delay} turns";
            }
            return $"Delayed AOE damage: {Value}";
        }
    }
}
