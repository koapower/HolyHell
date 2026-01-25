using HolyHell.Battle.Effect;
using System.Collections.Generic;

/// <summary>
/// Monster skill data from MonsterSkill.csv
/// CSV headers: ID, DisplayName, Type, Effect1, EffectAtr1, Effect2, EffectAtr2, Effect3, EffectAtr3, VFX ID
/// </summary>
public class MonsterSkillRow
{
    [Column("ID")]
    public string Id;

    [Column("DisplayName")]
    public string DisplayName;

    [Column("Type")]
    public string Type; // Attack, Buff, Debuff, etc.

    // Effect 1
    [Column("Effect1")]
    public CardEffectType Effect1Type;

    [Column("EffectAtr1")]
    public float Effect1Value;

    // Effect 2
    [Column("Effect2")]
    public CardEffectType Effect2Type;

    [Column("EffectAtr2")]
    public float Effect2Value;

    // Effect 3
    [Column("Effect3")]
    public CardEffectType Effect3Type;

    [Column("EffectAtr3")]
    public float Effect3Value;

    // VFX
    [Column("VFX ID")]
    public string VfxId;

    // Runtime effects (lazy initialized)
    private List<EffectBase> effects;
    public List<EffectBase> Effects
    {
        get
        {
            if (effects == null)
            {
                effects = CreateEffects();
            }
            return effects;
        }
    }

    /// <summary>
    /// Create effect instances from skill data
    /// </summary>
    private List<EffectBase> CreateEffects()
    {
        var effectList = new List<EffectBase>();

        // Effect 1
        if (Effect1Type != CardEffectType.None)
        {
            var effect = EffectFactory.CreateEffect(Effect1Type, Effect1Value.ToString());
            if (effect != null) effectList.Add(effect);
        }

        // Effect 2
        if (Effect2Type != CardEffectType.None)
        {
            var effect = EffectFactory.CreateEffect(Effect2Type, Effect2Value.ToString());
            if (effect != null) effectList.Add(effect);
        }

        // Effect 3
        if (Effect3Type != CardEffectType.None)
        {
            var effect = EffectFactory.CreateEffect(Effect3Type, Effect3Value.ToString());
            if (effect != null) effectList.Add(effect);
        }

        return effectList;
    }

    /// <summary>
    /// Get effect type by index (1-3)
    /// </summary>
    public CardEffectType GetEffectType(int index)
    {
        return index switch
        {
            1 => Effect1Type,
            2 => Effect2Type,
            3 => Effect3Type,
            _ => CardEffectType.None
        };
    }

    /// <summary>
    /// Get effect value by index (1-3)
    /// </summary>
    public float GetEffectValue(int index)
    {
        return index switch
        {
            1 => Effect1Value,
            2 => Effect2Value,
            3 => Effect3Value,
            _ => 0f
        };
    }
}
