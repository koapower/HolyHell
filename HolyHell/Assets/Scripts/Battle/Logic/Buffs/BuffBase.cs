public abstract class BuffBase
{
    public string Id;
    public int StackCount;

    public virtual float OnCalculateDamage(float currentDamage) => currentDamage;
}
