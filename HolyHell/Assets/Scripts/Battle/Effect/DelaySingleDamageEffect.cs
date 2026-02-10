using HolyHell.Battle.Card;
using HolyHell.Battle.Logic;
using HolyHell.Data.Type;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    public class DelaySingleDamageEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.DelaySingleDamage;

        public DelaySingleDamageEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("DelaySingleDamageEffect: Target is null");
                return false;
            }

            if (context.DelayedEffectQueue == null)
            {
                Debug.LogWarning("DelaySingleDamageEffect: DelayedEffectQueue is null");
                return false;
            }

            if (EffectValueParser.ParseDelayParams(Value, out int damage, out int delay))
            {
                var delayedEffect = new DelayedEffect(
                    DelayedEffect.EffectTargetType.Single,
                    context.Caster,
                    damage,
                    delay,
                    context.SkillElementType,
                    context.Target,
                    true
                );

                context.DelayedEffectQueue.AddDelayedEffect(delayedEffect);
                Debug.Log($"DelaySingleDamage queued: {damage} damage after {delay} turns");
            }
            else
            {
                Debug.LogWarning($"DelaySingleDamageEffect: Failed to parse parameters '{Value}'");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new DelaySingleDamageEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            if (EffectValueParser.ParseDelayParams(Value, out int damage, out int delay))
            {
                return $"Deal {damage} damage after {delay} turns";
            }
            return $"Delayed damage: {Value}";
        }
    }
}
