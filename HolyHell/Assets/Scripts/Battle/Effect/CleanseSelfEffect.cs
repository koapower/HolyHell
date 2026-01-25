using HolyHell.Battle.Card;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    public class CleanseSelfEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.CleanseSelf;

        public CleanseSelfEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int count = EffectValueParser.ParseInt(Value);
            int removedCount = context.Caster.buffHandler.RemoveDebuffs(count);
            Debug.Log($"Cleansed {removedCount} debuffs from caster");
            return false;
        }

        public override EffectBase Clone()
        {
            return new CleanseSelfEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Remove {Value} debuffs from self";
        }
    }
}
