using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// BoostDmg buff - Flat damage increase (stackable)
    /// </summary>
    public class BoostDmgBuff : BuffBase
    {
        public override bool IsStackable => true;
        public override bool IsPositive => true;

        private int flatDamageBoost;

        public BoostDmgBuff(int flatDamageBoost, int stackCount = 1, int duration = -1)
            : base(BuffType.BoostDmg.ToString(), stackCount, duration)
        {
            this.flatDamageBoost = flatDamageBoost;
        }

        public override float OnCalculateDamage(float currentDamage)
        {
            return currentDamage + (flatDamageBoost * StackCount.Value);
        }
    }
}
