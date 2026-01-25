using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// Cursed buff - Reduce all healing to 0 (negative)
    /// Healing reduction is handled in DamageCalculator.ApplyHealing
    /// </summary>
    public class CursedBuff : BuffBase
    {
        public override bool IsStackable => false;
        public override bool IsPositive => false;

        public CursedBuff(int duration = -1)
            : base(BuffType.Cursed.ToString(), 1, duration)
        {
        }
    }
}
