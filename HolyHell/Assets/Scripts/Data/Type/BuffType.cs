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
        Lifesteel,          // Next attack heals for damage dealt
        ReqChange,          // Modify next card requirement
        IncreaseDmg,        // X% damage increase
        BoostDmg,           // Flat damage increase (stackable)
        IncreaseRes,        // Increase resistance for types (stackable)

        // Negative Buffs
        ReduceRes,          // Reduce resistance for types (stackable)
        Fragile,            // Takes X% increased damage (stackable)
        Bleeding,           // Takes X% HP damage at turn end (stackable)
        Feared,             // Takes damage again at turn end
        ReduceAtk,          // Reduce attack by X% (enemies only, stackable)
        Cursed,             // Reduce all healing to 0

        // Mixed/Special Buffs
        Gifted,             // Receive 1 random card at turn start (stackable)
    }
}
