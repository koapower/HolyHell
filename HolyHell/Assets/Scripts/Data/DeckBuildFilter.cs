using System.Collections.Generic;

/// <summary>
/// Represents the active filter state for the deck-building card database.
/// An empty set for any category means "show all" for that category.
/// </summary>
public class DeckBuildFilter
{
    public HashSet<ElementType> elementTypes = new HashSet<ElementType>();
    public HashSet<Faction> factions = new HashSet<Faction>();
    public HashSet<GodType> godTypes = new HashSet<GodType>();
    public HashSet<int> rarities = new HashSet<int>();

    public bool IsEmpty =>
        elementTypes.Count == 0 &&
        factions.Count == 0 &&
        godTypes.Count == 0 &&
        rarities.Count == 0;

    /// <summary>
    /// Returns true if the card passes all active filter criteria.
    /// An empty set for a category is treated as "no filter" (passes all).
    /// </summary>
    public bool Matches(CardRow card)
    {
        if (elementTypes.Count > 0 && !elementTypes.Contains(card.ElementType))
            return false;
        if (factions.Count > 0 && !factions.Contains(card.Faction))
            return false;
        if (godTypes.Count > 0 && !godTypes.Contains(card.GodType))
            return false;
        if (rarities.Count > 0 && !rarities.Contains(card.Rarity))
            return false;
        return true;
    }

    /// <summary>
    /// Creates a deep copy of this filter.
    /// </summary>
    public DeckBuildFilter Clone()
    {
        return new DeckBuildFilter
        {
            elementTypes = new HashSet<ElementType>(elementTypes),
            factions = new HashSet<Faction>(factions),
            godTypes = new HashSet<GodType>(godTypes),
            rarities = new HashSet<int>(rarities)
        };
    }

    public void Clear()
    {
        elementTypes.Clear();
        factions.Clear();
        godTypes.Clear();
        rarities.Clear();
    }
}
