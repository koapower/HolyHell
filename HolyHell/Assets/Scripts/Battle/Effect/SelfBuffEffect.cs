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
            string buffId = EffectValueParser.ParseBuffId(Value);
            var buff = BuffFactory.CreateBuffFromId(buffId);

            if (buff != null)
            {
                context.Caster.buffHandler.AddBuff(buff);
                Debug.Log($"Applied {buffId} buff to caster");
            }
            else
            {
                Debug.LogWarning($"Failed to create buff: {buffId}");
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
