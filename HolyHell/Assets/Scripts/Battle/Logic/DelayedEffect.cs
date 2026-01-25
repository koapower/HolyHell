using HolyHell.Battle.Entity;
using HolyHell.Data.Type;

namespace HolyHell.Battle.Logic
{
    /// <summary>
    /// Represents a delayed effect that will trigger after a certain number of turns
    /// </summary>
    public class DelayedEffect
    {
        public enum EffectTargetType
        {
            Single,
            AOE
        }

        public EffectTargetType TargetType { get; set; }
        public BattleEntity Caster { get; set; }
        public BattleEntity SingleTarget { get; set; }
        public int Damage { get; set; }
        public int RemainingTurns { get; set; }
        public ElementType ElementType { get; set; }
        public bool TargetEnemies { get; set; } // True if targeting enemies, false if targeting allies

        public DelayedEffect(
            EffectTargetType targetType,
            BattleEntity caster,
            int damage,
            int delay,
            ElementType elementType = ElementType.None,
            BattleEntity singleTarget = null,
            bool targetEnemies = true)
        {
            TargetType = targetType;
            Caster = caster;
            Damage = damage;
            RemainingTurns = delay;
            ElementType = elementType;
            SingleTarget = singleTarget;
            TargetEnemies = targetEnemies;
        }

        /// <summary>
        /// Decrease remaining turns by 1
        /// </summary>
        public void DecrementTurn()
        {
            RemainingTurns--;
        }

        /// <summary>
        /// Check if effect should trigger this turn
        /// </summary>
        public bool ShouldTrigger()
        {
            return RemainingTurns <= 0;
        }
    }
}
