using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Gain action points for the caster (player)
    /// </summary>
    public class GainActionEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.GainAction;

        public GainActionEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int amount = EffectValueParser.ParseInt(Value);

            if (context.Caster != null && context.Caster is PlayerEntity player)
            {
                player.actionPoint.Value += amount;
                Debug.Log($"GainAction: Gained {amount} action point(s) (now {player.actionPoint.CurrentValue})");
            }
            else
            {
                Debug.LogWarning("GainActionEffect: Caster is null");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new GainActionEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Gain {Value} action point(s)";
        }
    }
}
