using HolyHell.Battle.Card;
using HolyHell.Battle.Logic;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Deal damage to all enemies
    /// </summary>
    public class AOEDamageEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.AOEDamage;

        public AOEDamageEffect(string value, string requirement = null)
            : base(value, requirement)
        {
        }

        public override bool Execute(EffectContext context)
        {
            int baseDamage = EffectValueParser.ParseInt(Value);
            bool anyKilled = false;

            var targets = context.GetAliveEnemies();
            foreach (var enemy in targets)
            {
                float finalDamage = DamageCalculator.CalculateDamage(baseDamage, context.Caster, enemy);
                bool killed = DamageCalculator.ApplyDamage(enemy, context.Caster, Mathf.RoundToInt(finalDamage));
                if (killed) anyKilled = true;
            }

            return anyKilled;
        }

        public override EffectBase Clone()
        {
            return new AOEDamageEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Deal {Value} damage to all enemies";
        }
    }
}
