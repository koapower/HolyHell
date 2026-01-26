using UnityEngine;
using System;
using System.Collections.Generic;
using HolyHell.Battle.Entity;
using HolyHell.Battle.Card;

/// <summary>
/// Displays and manages player's hand of cards
/// </summary>
public class HandUI : MonoBehaviour
{
    [SerializeField] private FanLayoutGroup fanLayoutGroup;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject cardUIPrefab;

    private PlayerEntity player;
    private Action<CardInstance> onCardClickCallback;
    private List<CardUI> cardUIList = new List<CardUI>();

    // Polling for hand changes (simple approach)
    private int lastHandCount = 0;

    public void Initialize(PlayerEntity playerEntity, Action<CardInstance> onCardClick)
    {
        player = playerEntity;
        onCardClickCallback = onCardClick;

        if (player == null)
        {
            Debug.LogError("HandUI: Player is null!");
            return;
        }

        RebuildHand();
        Debug.Log("HandUI initialized");
    }

    private void Update()
    {
        // Simple polling to detect hand changes
        // TODO use events or ReactiveCollection
        if (player != null && player.hand.Count != lastHandCount)
        {
            lastHandCount = player.hand.Count;
            RebuildHand();
            fanLayoutGroup.LayoutCards();
        }

        // Update playability based on action points
        if (player != null)
        {
            UpdateCardPlayability();
        }
    }

    /// <summary>
    /// Rebuild entire hand UI
    /// </summary>
    private void RebuildHand()
    {
        ClearHand();

        if (player == null || player.hand == null) return;

        foreach (var card in player.hand)
        {
            CreateCardUI(card);
        }

        Debug.Log($"HandUI rebuilt with {player.hand.Count} cards");
    }

    private void CreateCardUI(CardInstance card)
    {
        if (cardUIPrefab == null || cardContainer == null)
        {
            Debug.LogError("HandUI: Prefab or container is null!");
            return;
        }

        var cardUIObj = Instantiate(cardUIPrefab, cardContainer);
        var cardUI = cardUIObj.GetComponent<CardUI>();

        if (cardUI != null)
        {
            cardUI.Initialize(card, OnCardClicked);
            cardUIList.Add(cardUI);
        }
        else
        {
            Debug.LogError("HandUI: CardUI component not found on prefab!");
        }
    }

    private void OnCardClicked(CardInstance card)
    {
        onCardClickCallback?.Invoke(card);
    }

    /// <summary>
    /// Update which cards can be played based on action points
    /// </summary>
    private void UpdateCardPlayability()
    {
        int currentAP = player.actionPoint.Value;

        for (int i = 0; i < cardUIList.Count && i < player.hand.Count; i++)
        {
            var cardUI = cardUIList[i];
            var card = player.hand[i];

            if (cardUI != null && card != null)
            {
                bool playable = currentAP >= card.ActionCost;
                cardUI.SetPlayable(playable);
            }
        }
    }

    private void ClearHand()
    {
        foreach (var cardUI in cardUIList)
        {
            if (cardUI != null)
            {
                Destroy(cardUI.gameObject);
            }
        }
        cardUIList.Clear();
    }

    private void OnDestroy()
    {
        ClearHand();
    }
}
