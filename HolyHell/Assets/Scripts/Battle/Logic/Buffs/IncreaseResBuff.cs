using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// IncreaseRes buff - Increase resistance for specific element types (stackable)
    /// </summary>
    public class IncreaseResBuff : BuffBase
    {
        public override bool IsStackable => true;
        public override bool IsPositive => true;

        public ElementType ElementType { get; private set; }
        private float resistancePercentage;

        public IncreaseResBuff(ElementType elementType, float resistancePercentage, int stackCount = 1, int duration = -1)
            : base(BuffType.IncreaseRes.ToString(), stackCount, duration)
        {
            ElementType = elementType;
            this.resistancePercentage = resistancePercentage;
        }

        public float GetResistanceModifier()
        {
            // Returns resistance reduction multiplier (e.g., 0.1 = 10% damage reduction per stack)
            return resistancePercentage / 100f * StackCount;
        }
    }
}
