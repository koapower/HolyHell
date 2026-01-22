using System.Collections.Generic;

public class BuffHandler
{
    public List<BuffBase> activeBuffs = new List<BuffBase>();

    public float GetModifiedDamage(float baseDamage)
    {
        float finalDamage = baseDamage;
        foreach (var buff in activeBuffs)
        {
            finalDamage = buff.OnCalculateDamage(finalDamage);
        }
        return finalDamage;
    }
}