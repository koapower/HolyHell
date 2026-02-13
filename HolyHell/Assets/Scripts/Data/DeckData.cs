using System;
using System.Collections.Generic;

/// <summary>
/// Serializable data for a single deck.
/// </summary>
[Serializable]
public class DeckData
{
    public string deckName = "New Deck";
    public List<string> cardIds = new List<string>();
}

/// <summary>
/// Top-level save file containing all decks.
/// </summary>
[Serializable]
public class DeckSaveFile
{
    public List<DeckData> decks = new List<DeckData>();
}
