using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// Gifted buff - Receive 1 random card at turn start (stackable)
    /// Card generation needs to be integrated with CardDeckManager
    /// </summary>
    public class GiftedBuff : BuffBase
    {
        public override bool IsStackable => true;
        public override bool IsPositive => true;

        public GiftedBuff(int stackCount = 1, int duration = -1)
            : base(BuffType.Gifted.ToString(), stackCount, duration)
        {
        }

        public override void OnTurnStart()
        {
            base.OnTurnStart();

            // Note: Actual card generation logic needs to be implemented in the battle system
            // This would typically involve accessing CardDeckManager to add random cards
            // For now, this just marks when the effect should trigger
        }

        public int GetCardsToGenerate()
        {
            return StackCount.Value; // 1 card per stack
        }
    }
}
