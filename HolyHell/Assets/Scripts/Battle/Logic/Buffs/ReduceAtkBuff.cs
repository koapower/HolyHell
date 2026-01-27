using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// ReduceAtk buff - Reduce attack by X% (enemies only, stackable, negative)
    /// </summary>
    public class ReduceAtkBuff : BuffBase
    {
        public override bool IsStackable => true;
        public override bool IsPositive => false;

        private float attackReductionPercentage;

        public ReduceAtkBuff(float attackReductionPercentage, int stackCount = 1, int duration = -1)
            : base(BuffType.ReduceAtk.ToString(), stackCount, duration)
        {
            this.attackReductionPercentage = attackReductionPercentage;
        }

        public override float OnCalculateDamage(float currentDamage)
        {
            // Reduce damage dealt by percentage per stack
            return currentDamage * (1f - (attackReductionPercentage / 100f) * StackCount.Value);
        }
    }
}
