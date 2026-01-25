using HolyHell.Battle.Logic;
using HolyHell.Data.Type;
using UnityEngine;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// Bleeding buff - Takes X% HP damage at turn end (stackable, negative)
    /// </summary>
    public class BleedingBuff : BuffBase
    {
        public override bool IsStackable => true;
        public override bool IsPositive => false;

        private float damagePercentage;

        public BleedingBuff(float damagePercentage, int stackCount = 1, int duration = -1)
            : base(BuffType.Bleeding.ToString(), stackCount, duration)
        {
            this.damagePercentage = damagePercentage;
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();

            if (Owner != null)
            {
                // Calculate damage based on percentage of max HP per stack
                int damageAmount = Mathf.RoundToInt(Owner.maxHp.CurrentValue * damagePercentage / 100f * StackCount);

                if (damageAmount > 0)
                {
                    DamageCalculator.ApplyDamage(Owner, Owner, damageAmount);
                }
            }
        }
    }
}
