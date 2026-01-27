using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// Guard buff - Immune to first attack
    /// </summary>
    public class GuardBuff : BuffBase
    {
        public override bool IsStackable => false;
        public override bool IsPositive => true;

        public GuardBuff(int duration = -1)
            : base(BuffType.Guard.ToString(), 1, duration)
        {
        }

        public override float OnReceiveDamage(float incomingDamage)
        {
            // Block the first attack completely
            if (incomingDamage > 0)
            {
                // Buff will be removed after blocking (set duration to 0)
                Duration.Value = 0;
                return 0f; // No damage taken
            }

            return incomingDamage;
        }
    }
}
