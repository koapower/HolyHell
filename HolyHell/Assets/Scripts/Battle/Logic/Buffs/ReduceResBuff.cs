using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// ReduceRes buff - Reduce resistance for specific element types (stackable, negative)
    /// </summary>
    public class ReduceResBuff : BuffBase
    {
        public override bool IsStackable => true;
        public override bool IsPositive => false;

        public ElementType ElementType { get; private set; }
        private float resistanceReduction;

        public ReduceResBuff(ElementType elementType, float resistanceReduction, int stackCount = 1, int duration = -1)
            : base(BuffType.ReduceRes.ToString(), stackCount, duration)
        {
            ElementType = elementType;
            this.resistanceReduction = resistanceReduction;
        }

        public float GetResistanceModifier()
        {
            // Returns resistance increase multiplier (e.g., 0.1 = 10% more damage per stack)
            return resistanceReduction / 100f * StackCount;
        }
    }
}
