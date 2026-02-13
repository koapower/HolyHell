using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles JSON serialization of deck data to and from persistentDataPath.
/// </summary>
public class SaveManager : MonoSingleton<SaveManager>
{
    private const string DecksFileName = "decks.json";

    private string DecksFilePath => Path.Combine(Application.persistentDataPath, DecksFileName);

    /// <summary>
    /// Saves the given list of decks to disk as JSON.
    /// </summary>
    public void SaveDecks(List<DeckData> decks)
    {
        var saveFile = new DeckSaveFile { decks = decks };
        string json = JsonUtility.ToJson(saveFile, prettyPrint: true);

        try
        {
            File.WriteAllText(DecksFilePath, json);
            Debug.Log($"[SaveManager] Decks saved to {DecksFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to save decks: {e}");
        }
    }

    /// <summary>
    /// Loads decks from disk. Returns an empty list if the file doesn't exist or is malformed.
    /// </summary>
    public List<DeckData> LoadDecks()
    {
        if (!File.Exists(DecksFilePath))
            return new List<DeckData>();

        try
        {
            string json = File.ReadAllText(DecksFilePath);
            var saveFile = JsonUtility.FromJson<DeckSaveFile>(json);
            return saveFile?.decks ?? new List<DeckData>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load decks: {e}");
            return new List<DeckData>();
        }
    }
}
