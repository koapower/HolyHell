using HolyHell.Battle.Card;
using HolyHell.Battle.Logic.Buffs;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    public class TargetAOEBuffEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.TargetAOEBuff;

        public TargetAOEBuffEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            var buffDef = EffectValueParser.ParseBuffDefinition(Value);

            var targets = context.GetAliveEnemies();
            foreach (var enemy in targets)
            {
                var buff = BuffFactory.CreateBuffFromId(buffDef.Id, buffDef.Parameter, buffDef.StackCount, buffDef.Duration);
                if (buff != null)
                {
                    enemy.buffHandler.AddBuff(buff);
                }
            }

            Debug.Log($"Applied {buffDef.Id} buff to all enemies");
            return false;
        }

        public override EffectBase Clone()
        {
            return new TargetAOEBuffEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Apply {Value} buff to all enemies";
        }
    }
}
