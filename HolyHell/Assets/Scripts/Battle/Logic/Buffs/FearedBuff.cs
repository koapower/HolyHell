using HolyHell.Battle.Logic;
using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// Feared buff - Takes damage again at turn end (negative)
    /// Requires tracking damage dealt during the turn
    /// </summary>
    public class FearedBuff : BuffBase
    {
        public override bool IsStackable => false;
        public override bool IsPositive => false;

        public int AccumulatedDamage { get; set; }

        public FearedBuff(int duration = -1)
            : base(BuffType.Feared.ToString(), 1, duration)
        {
            AccumulatedDamage = 0;
        }

        public void RecordDamage(int damage)
        {
            AccumulatedDamage += damage;
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();

            if (Owner != null && AccumulatedDamage > 0)
            {
                // Deal accumulated damage again at turn end
                DamageCalculator.ApplyDamage(Owner, Owner, AccumulatedDamage);
                AccumulatedDamage = 0; // Reset for next turn
            }
        }

        public override void OnTurnStart()
        {
            base.OnTurnStart();
            AccumulatedDamage = 0; // Reset at turn start
        }
    }
}
