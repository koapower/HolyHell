using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// Fragile buff - Takes X% increased damage (stackable, negative)
    /// </summary>
    public class FragileBuff : BuffBase
    {
        public override bool IsStackable => true;
        public override bool IsPositive => false;

        private float damageIncreasePercentage;

        public FragileBuff(float damageIncreasePercentage, int stackCount = 1, int duration = -1)
            : base(BuffType.Fragile.ToString(), stackCount, duration)
        {
            this.damageIncreasePercentage = damageIncreasePercentage;
        }

        public override float OnReceiveDamage(float incomingDamage)
        {
            // Increase damage taken by percentage per stack
            return incomingDamage * (1f + (damageIncreasePercentage / 100f) * StackCount.Value);
        }
    }
}
