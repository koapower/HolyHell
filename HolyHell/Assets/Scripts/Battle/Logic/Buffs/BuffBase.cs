using HolyHell.Battle.Entity;
using R3;

namespace HolyHell.Battle.Logic.Buffs
{
    public abstract class BuffBase
    {
        public string Id;
        public ReactiveProperty<int> StackCount = new ReactiveProperty<int>();
        public ReactiveProperty<int> Duration = new ReactiveProperty<int>(); // -1 = permanent, 0+ = turns remaining
        public abstract bool IsStackable { get; }
        public abstract bool IsPositive { get; }

        // Reference to the entity this buff is attached to
        protected BattleEntity Owner;

        protected BuffBase(string id, int stackCount = 1, int duration = -1)
        {
            Id = id;
            StackCount.Value = stackCount;
            Duration.Value = duration;
        }

        // Set the owner of this buff
        public void SetOwner(BattleEntity owner)
        {
            Owner = owner;
        }

        // Damage modification hook (when owner deals damage)
        public virtual float OnCalculateDamage(float currentDamage) => currentDamage;

        // Damage received modification hook (when owner takes damage)
        public virtual float OnReceiveDamage(float incomingDamage) => incomingDamage;

        // Turn start hook
        public virtual void OnTurnStart()
        {
            // Decrease duration at turn start (can override this behavior)
            if (Duration.Value > 0)
            {
                Duration.Value--;
            }
        }

        // Turn end hook
        public virtual void OnTurnEnd()
        {
            // Override for effects that trigger at turn end
        }

        // Called when this buff is first applied
        public virtual void OnApplied() { }

        // Called when this buff is removed
        public virtual void OnRemoved() { }
    }
}
