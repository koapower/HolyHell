using HolyHell.Battle.Card;
using HolyHell.Battle.Logic.Buffs;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    public class TargetSingleBuffEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.TargetSingleBuff;

        public TargetSingleBuffEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("TargetSingleBuffEffect: Target is null");
                return false;
            }

            var buffDef = EffectValueParser.ParseBuffDefinition(Value);
            var buff = BuffFactory.CreateBuffFromId(buffDef.Id, buffDef.Parameter, buffDef.StackCount, buffDef.Duration);

            if (buff != null)
            {
                context.Target.buffHandler.AddBuff(buff);
                Debug.Log($"Applied {buff.Id} buff to target");
            }
            else
            {
                Debug.LogWarning($"Failed to create buff: {buff.Id}");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new TargetSingleBuffEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Apply {Value} buff to target";
        }
    }
}
