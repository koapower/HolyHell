namespace HolyHell.Data.Type
{
    /// <summary>
    /// Enum representing all buff types in the game
    /// Corresponds to buff IDs in the CSV specification
    /// </summary>
    public enum BuffType
    {
        None,

        // Positive Buffs
        Guard,              // Immune to first attack
        Blessed,            // Heal X% HP at turn end
        LifeSteal,          // Next attack heals for damage dealt
        ReqChange,          // Modify next card requirement
        IncreaseDmg,        // X% damage increase
        BoostDmg,           // Flat damage increase (stackable)

        // Increase Resistance Buffs (positive, stackable)
        IncreaseAllRes,     // Increase all resistance
        IncreaseEnlRes,     // Increase Enlightened resistance
        IncreaseDomRes,     // Increase Domination resistance
        IncreaseBliRes,     // Increase Bliss resistance
        IncreaseRavRes,     // Increase Ravenous resistance
        IncreaseDesRes,     // Increase Despair resistance

        // Decrease Resistance Buffs (negative, stackable)
        DecreaseAllRes,     // Decrease all resistance
        DecreaseEnlRes,     // Decrease Enlightened resistance
        DecreaseDomRes,     // Decrease Domination resistance
        DecreaseBliRes,     // Decrease Bliss resistance
        DecreaseRavRes,     // Decrease Ravenous resistance
        DecreaseDesRes,     // Decrease Despair resistance

        // Negative Buffs
        Fragile,            // Takes X% increased damage (stackable)
        Bleeding,           // Takes X% HP damage at turn end (stackable)
        Feared,             // Takes damage again at turn end
        ReduceAtk,          // Reduce attack by X% (enemies only, stackable)
        Cursed,             // Reduce all healing to 0

        // Mixed/Special Buffs
        Gifted,             // Receive 1 random card at turn start (stackable)
        Swift,              // Next played card also triggers adjacent card(s)
    }
}
