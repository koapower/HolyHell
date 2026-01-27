using HolyHell.Battle.Card;
using HolyHell.Battle.Logic.Buffs;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Apply buff to caster
    /// </summary>
    public class SelfBuffEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.SelfBuff;

        public SelfBuffEffect(string value, string requirement = null)
            : base(value, requirement)
        {
        }

        public override bool Execute(EffectContext context)
        {
            var buffDef = EffectValueParser.ParseBuffDefinition(Value);
            var buff = BuffFactory.CreateBuffFromId(buffDef.Id, buffDef.Parameter, buffDef.StackCount, buffDef.Duration);

            if (buff != null)
            {
                context.Caster.buffHandler.AddBuff(buff);
                Debug.Log($"Applied {buff.Id} buff to caster");
            }
            else
            {
                Debug.LogWarning($"Failed to create buff: {buff.Id}");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new SelfBuffEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Apply {Value} buff to self";
        }
    }
}
