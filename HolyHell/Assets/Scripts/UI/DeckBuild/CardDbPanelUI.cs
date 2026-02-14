using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The card database panel in deck building.
/// Shows all cards with search, filter, and sort controls.
/// </summary>
public class CardDbPanelUI : MonoBehaviour, IUIInitializable
{
    [Header("Search")]
    [SerializeField] private TMP_InputField searchInputField;

    [Header("Buttons")]
    [SerializeField] private Button filterButton;
    [SerializeField] private Button sortButton;
    [SerializeField] private Button clearFilterButton;

    [Header("Card List")]
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardSlotUI cardSlotPrefab;

    [Header("Controller")]
    [SerializeField] private DeckBuildUI deckBuildUI;

    private readonly List<CardSlotUI> activeSlots = new List<CardSlotUI>();

    public UniTask Init()
    {
        if (searchInputField != null)
            searchInputField.onValueChanged.AddListener(OnSearchChanged);

        if (filterButton != null)
            filterButton.onClick.AddListener(() => deckBuildUI?.OpenFilterWindow());

        if (sortButton != null)
            sortButton.onClick.AddListener(() => deckBuildUI?.OpenSortWindow());

        if (clearFilterButton != null)
            clearFilterButton.onClick.AddListener(() => deckBuildUI?.ClearFilter());

        cardSlotPrefab.gameObject.SetActive(false);

        return UniTask.CompletedTask;
    }

    private void OnSearchChanged(string query)
    {
        deckBuildUI?.OnSearchQueryChanged(query);
    }

    /// <summary>
    /// Rebuilds the displayed card list.
    /// </summary>
    /// <param name="cards">Pre-filtered and sorted list of cards to show.</param>
    /// <param name="onCardClicked">Called when a card slot is clicked (add to deck).</param>
    public void RefreshCards(IReadOnlyList<CardRow> cards, Action<CardRow> onCardClicked)
    {
        ClearSlots();

        if (cards == null || cardSlotPrefab == null || cardContainer == null) return;

        foreach (var cardRow in cards)
        {
            var slot = Instantiate(cardSlotPrefab, cardContainer);
            slot.gameObject.SetActive(true);
            activeSlots.Add(slot);

            CardRow captured = cardRow;

            if (slot.cardUI != null)
            {
                slot.cardUI.InitializeDeckBuild(cardRow, clickCallback: () => onCardClicked?.Invoke(captured));

                // Hover: update detail panel via DeckBuildUI
                var cardUIGo = slot.cardUI.gameObject;
                var hoverTrigger = cardUIGo.AddComponent<CardDbHoverTrigger>();
                hoverTrigger.Setup(cardRow, deckBuildUI);
            }
        }
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
