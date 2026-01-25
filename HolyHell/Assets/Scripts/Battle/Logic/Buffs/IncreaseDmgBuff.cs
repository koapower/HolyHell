using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// IncreaseDmg buff - X% damage increase
    /// </summary>
    public class IncreaseDmgBuff : BuffBase
    {
        public override bool IsStackable => false;
        public override bool IsPositive => true;

        private float damagePercentage;

        public IncreaseDmgBuff(float damagePercentage, int duration = -1)
            : base(BuffType.IncreaseDmg.ToString(), 1, duration)
        {
            this.damagePercentage = damagePercentage;
        }

        public override float OnCalculateDamage(float currentDamage)
        {
            return currentDamage * (1f + damagePercentage / 100f);
        }
    }
}
