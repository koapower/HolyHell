using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Decrease the Demon Gauge (D-meter) value
    /// </summary>
    public class DvalueDecEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.DvalueDec;

        public DvalueDecEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int amount = EffectValueParser.ParseInt(Value);
            var player = context.Caster as PlayerEntity;
            int newValue = System.Math.Max(0, player.demonGauge.CurrentValue - amount);
            int actualDecrease = player.demonGauge.CurrentValue - newValue;
            player.demonGauge.Value = newValue;
            Debug.Log($"DvalueDec: Decreased Demon Gauge by {actualDecrease} (now {player.demonGauge.CurrentValue})");

            return false;
        }

        public override EffectBase Clone()
        {
            return new DvalueDecEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Decrease Demon Gauge by {Value}";
        }
    }
}
