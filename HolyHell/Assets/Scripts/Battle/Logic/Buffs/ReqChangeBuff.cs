using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// ReqChange buff - Modify next card requirement
    /// This buff needs to be integrated with EffectRequirementEvaluator
    /// </summary>
    public class ReqChangeBuff : BuffBase
    {
        public override bool IsStackable => false;
        public override bool IsPositive => true;

        public int RequirementModifier { get; private set; }

        public ReqChangeBuff(int modifier, int duration = -1)
            : base(BuffType.ReqChange.ToString(), 1, duration)
        {
            RequirementModifier = modifier;
        }
    }
}
