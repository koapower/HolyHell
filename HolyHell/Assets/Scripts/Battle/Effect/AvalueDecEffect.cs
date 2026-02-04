using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Decrease the Angel Gauge (A-meter) value
    /// </summary>
    public class AvalueDecEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.AvalueDec;

        public AvalueDecEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int amount = EffectValueParser.ParseInt(Value);
            var player = context.Caster as PlayerEntity;
            int newValue = System.Math.Max(0, player.angelGauge.CurrentValue - amount);
            int actualDecrease = player.angelGauge.CurrentValue - newValue;
            player.angelGauge.Value = newValue;
            Debug.Log($"AvalueDec: Decreased Angel Gauge by {actualDecrease} (now {player.angelGauge.CurrentValue})");

            return false;
        }

        public override EffectBase Clone()
        {
            return new AvalueDecEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Decrease Angel Gauge by {Value}";
        }
    }
}
