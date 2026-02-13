using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Root controller for the deck-building screen.
/// Owns the current deck being edited, the card database, and coordinates all sub-panels.
/// </summary>
public class DeckBuildUI : MonoBehaviour
{
    [Header("Sub-panels")]
    [SerializeField] private CardDetailPanelUI cardDetailPanel;
    [SerializeField] private DeckPanelUI deckPanel;
    [SerializeField] private CardDbPanelUI cardDbPanel;
    [SerializeField] private FilterWindowUI filterWindow;
    [SerializeField] private SortWindowUI sortWindow;

    [Header("Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button closeButton;

    // Deck state
    private DeckData currentDeck;
    private List<DeckData> allDecks = new List<DeckData>();

    // Database and search/filter/sort state
    private CardDatabase cardDatabase;
    private string searchQuery = string.Empty;
    private DeckBuildFilter currentFilter = new DeckBuildFilter();
    public SortField CurrentSortField { get; private set; } = SortField.Id;
    public bool SortAscending { get; private set; } = true;

    private async void Start()
    {
        // Wait for the table manager service to be ready before querying cards
        var tableManager = await ServiceLocator.Instance.GetAsync<ITableManager>();
        cardDatabase = new CardDatabase(tableManager);

        // Load saved decks; create a default deck if none exist
        allDecks = SaveManager.Instance.LoadDecks();
        if (allDecks.Count == 0)
        {
            allDecks.Add(new DeckData());
        }

        Open(allDecks[0]);

        saveButton?.onClick.AddListener(Save);
        closeButton?.onClick.AddListener(Close);
    }

    /// <summary>
    /// Opens the deck builder with the given deck.
    /// </summary>
    public void Open(DeckData deck)
    {
        currentDeck = deck;
        gameObject.SetActive(true);
        RefreshDeckDisplay();
        RefreshDbDisplay();
    }

    /// <summary>
    /// Saves all decks to disk.
    /// </summary>
    public void Save()
    {
        // Sync the current deck name from the panel's input field
        if (deckPanel != null)
            currentDeck.deckName = deckPanel.GetDeckName();

        SaveManager.Instance.SaveDecks(allDecks);
    }

    /// <summary>
    /// Closes the deck builder panel.
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // DB panel callbacks

    /// <summary>
    /// Called by CardDbPanelUI when the search query changes.
    /// </summary>
    public void OnSearchQueryChanged(string query)
    {
        searchQuery = query;
        RefreshDbDisplay();
    }

    /// <summary>
    /// Applies search + filter + sort and refreshes the card database panel.
    /// </summary>
    public void RefreshDbDisplay()
    {
        if (cardDatabase == null || cardDbPanel == null) return;

        List<CardRow> cards = cardDatabase.SearchAndFilter(searchQuery, currentFilter);
        cards = cardDatabase.Sort(cards, CurrentSortField, SortAscending);
        cardDbPanel.RefreshCards(cards, OnCardDbClicked);
    }

    /// <summary>
    /// Called when the user clicks a card in the database panel (adds it to the current deck).
    /// </summary>
    public void OnCardDbClicked(CardRow cardRow)
    {
        if (currentDeck == null || cardRow == null) return;
        currentDeck.cardIds.Add(cardRow.Id);
        RefreshDeckDisplay();
    }

    // -----------------------------------------------------------------------
    // Deck panel callbacks

    private void RefreshDeckDisplay()
    {
        deckPanel?.RefreshDeck(currentDeck, OnDeckCardClicked);
    }

    /// <summary>
    /// Called when the user clicks a card in the deck panel (removes it from the deck).
    /// </summary>
    public void OnDeckCardClicked(string cardId)
    {
        if (currentDeck == null) return;
        currentDeck.cardIds.Remove(cardId);
        RefreshDeckDisplay();
    }

    // -----------------------------------------------------------------------
    // Hover callback (updates the detail panel)

    /// <summary>
    /// Called when the user hovers over any card in the db or deck panel.
    /// </summary>
    public void OnCardHovered(CardRow cardRow)
    {
        cardDetailPanel?.ShowCard(cardRow);
    }

    // -----------------------------------------------------------------------
    // Filter / Sort

    public void OpenFilterWindow()
    {
        filterWindow?.Open(currentFilter, ApplyFilter);
    }

    private void ApplyFilter(DeckBuildFilter newFilter)
    {
        currentFilter = newFilter;
        RefreshDbDisplay();
    }

    public void ClearFilter()
    {
        currentFilter.Clear();
        RefreshDbDisplay();
    }

    public void OpenSortWindow()
    {
        sortWindow?.Open(CurrentSortField, SortAscending, ApplySort);
    }

    public void ApplySort(SortField field, bool ascending)
    {
        CurrentSortField = field;
        SortAscending = ascending;
        RefreshDbDisplay();
    }
}
