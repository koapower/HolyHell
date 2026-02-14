using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current deck being built.
/// Each card slot shows a CardUI in DeckBuilding mode; clicking a card removes it from the deck.
/// </summary>
public class DeckPanelUI : MonoBehaviour, IUIInitializable
{
    [Header("Deck Name")]
    [SerializeField] private TMP_InputField deckNameInput;

    [Header("Card List")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardSlotUI cardSlotPrefab;

    [Header("Deck Build Controller (for hover)")]
    [SerializeField] private DeckBuildUI deckBuildUI;

    private readonly List<CardSlotUI> activeSlots = new List<CardSlotUI>();
    private ITableManager tableManager;

    public async UniTask Init()
    {
        tableManager = await ServiceLocator.Instance.GetAsync<ITableManager>();
        cardSlotPrefab.gameObject.SetActive(false);
    }

    /// <summary>
    /// Rebuilds the card slot list to match the given deck.
    /// </summary>
    /// <param name="deck">Deck data to display.</param>
    /// <param name="onCardClicked">Called with the card ID when a card slot is clicked (remove).</param>
    public void RefreshDeck(DeckData deck, Action<string> onCardClicked)
    {
        if (deck == null) return;

        if (deckNameInput != null)
            deckNameInput.text = deck.deckName;

        ClearSlots();

        if (tableManager == null || cardSlotPrefab == null || cardContainer == null) return;

        var cardTable = tableManager.GetTable<CardRow>();

        foreach (string cardId in deck.cardIds)
        {
            var cardRow = cardTable.GetRow(r => r.Id == cardId);
            if (cardRow == null) continue;

            var slot = Instantiate(cardSlotPrefab, cardContainer);
            slot.gameObject.SetActive(true);
            activeSlots.Add(slot);

            // Capture for closure
            string capturedId = cardId;
            CardRow capturedRow = cardRow;

            if (slot.cardUI != null)
            {
                slot.cardUI.InitializeDeckBuild(cardRow, clickCallback: () => onCardClicked?.Invoke(capturedId));

                var cardUIGo = slot.cardUI.gameObject;
                var hoverTrigger = cardUIGo.AddComponent<CardDbHoverTrigger>();
                hoverTrigger.Setup(cardRow, deckBuildUI);
            }
        }
    }

    /// <summary>
    /// Returns the current deck name from the input field.
    /// </summary>
    public string GetDeckName()
    {
        return deckNameInput != null ? deckNameInput.text : string.Empty;
    }

    private void ClearSlots()
    {
        foreach (var slot in activeSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        activeSlots.Clear();
    }
}
