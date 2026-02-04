using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Increase the Angel Gauge (A-meter) value
    /// </summary>
    public class AvalueIncEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.AvalueInc;

        public AvalueIncEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int amount = EffectValueParser.ParseInt(Value);

            if (context.Caster != null && context.Caster is PlayerEntity player)
            {
                player.angelGauge.Value += amount;
                Debug.Log($"AvalueInc: Increased Angel Gauge by {amount} (now {player.angelGauge.CurrentValue})");
            }
            else
            {
                Debug.LogWarning("AvalueIncEffect: Caster is null");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new AvalueIncEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Increase Angel Gauge by {Value}";
        }
    }
}
