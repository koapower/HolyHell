using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// Lifesteel buff - Next attack heals for damage dealt
    /// Note: The healing logic needs to be handled in DamageCalculator
    /// This buff just marks that lifesteal is active
    /// </summary>
    public class LifesteelBuff : BuffBase
    {
        public override bool IsStackable => false;
        public override bool IsPositive => true;

        public bool HasTriggered { get; set; }

        public LifesteelBuff(int duration = -1)
            : base(BuffType.Lifesteel.ToString(), 1, duration)
        {
            HasTriggered = false;
        }

        public void TriggerLifesteal()
        {
            HasTriggered = true;
            Duration.Value = 0; // Remove buff after trigger
        }
    }
}
