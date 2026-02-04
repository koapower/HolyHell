using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Increase the Demon Gauge (D-meter) value
    /// </summary>
    public class DvalueIncEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.DvalueInc;

        public DvalueIncEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int amount = EffectValueParser.ParseInt(Value);

            if (context.Caster != null && context.Caster is PlayerEntity playerEntity)
            {
                playerEntity.demonGauge.Value += amount;
                Debug.Log($"DvalueInc: Increased Demon Gauge by {amount} (now {playerEntity.demonGauge.CurrentValue})");
            }
            else
            {
                Debug.LogWarning("DvalueIncEffect: Caster is null");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new DvalueIncEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Increase Demon Gauge by {Value}";
        }
    }
}
