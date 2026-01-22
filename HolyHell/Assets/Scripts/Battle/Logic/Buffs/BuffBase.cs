public abstract class BuffBase
{
    public string Id;
    public int StackCount;
    public int Duration; // -1 = permanent, 0+ = turns remaining

    protected BuffBase(string id, int stackCount = 1, int duration = -1)
    {
        Id = id;
        StackCount = stackCount;
        Duration = duration;
    }

    // Damage modification hook
    public virtual float OnCalculateDamage(float currentDamage) => currentDamage;

    // Turn start hook
    public virtual void OnTurnStart()
    {
        // Decrease duration at turn start (can override this behavior)
        if (Duration > 0)
        {
            Duration--;
        }
    }

    // Turn end hook
    public virtual void OnTurnEnd()
    {
        // Override for effects that trigger at turn end
    }
}
