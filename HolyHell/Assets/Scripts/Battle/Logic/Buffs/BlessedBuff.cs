using HolyHell.Battle.Logic;
using HolyHell.Data.Type;
using UnityEngine;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// Blessed buff - Heal X% HP at turn end
    /// </summary>
    public class BlessedBuff : BuffBase
    {
        public override bool IsStackable => false;
        public override bool IsPositive => true;

        private float healPercentage;

        public BlessedBuff(float healPercentage, int duration = -1)
            : base(BuffType.Blessed.ToString(), 1, duration)
        {
            this.healPercentage = healPercentage;
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();

            if (Owner != null)
            {
                // Calculate heal amount based on percentage of max HP
                int healAmount = GameMath.RoundToInt(Owner.maxHp.CurrentValue * healPercentage / 100f);

                if (healAmount > 0)
                {
                    DamageCalculator.ApplyHealing(Owner, healAmount);
                }
            }
        }
    }
}
