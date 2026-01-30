using Cysharp.Threading.Tasks;
using HolyHell.Battle;
using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;
using HolyHell.UI.Battle;
using ObservableCollections;
using R3;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

/// <summary>
/// Displays and manages player's hand of cards
/// </summary>
public class HandUI : MonoBehaviour
{
    [SerializeField] private FanLayoutGroup fanLayoutGroup;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private CardSlotUI cardSlotPrefab;
    [SerializeField] private RectTransform handRectTransform;

    private BattleManager battleManager;
    private CardDragHandler dragHandler;
    private PlayerEntity player;
    private List<CardSlotUI> cardSlotUIList = new List<CardSlotUI>();
    private List<CardUI> cardUIList = new List<CardUI>();
    private ISynchronizedView<CardInstance, GameObject> view;
    private bool _isLayoutDirty = false;
    private Camera cachedCamera;

    public void Initialize(BattleManager battleManager, CardDragHandler dragHandler, PlayerEntity playerEntity)
    {
        cardSlotPrefab.gameObject.SetActive(false);

        this.battleManager = battleManager;
        this.dragHandler = dragHandler;
        player = playerEntity;

        // Get or create handRectTransform if not assigned
        if (handRectTransform == null)
        {
            handRectTransform = GetComponent<RectTransform>();
        }

        // Cache the camera for RectTransform checks
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            cachedCamera = canvas.worldCamera;
        }
        else
        {
            cachedCamera = null; // For ScreenSpaceOverlay
        }

        if (player == null)
        {
            Debug.LogError("HandUI: Player is null!");
            return;
        }

        view = player.hand.CreateView(card =>
        {
            var slotObj = CreateCardSlotUI(card);
            return slotObj.gameObject;
        }).AddTo(this);
        view.ViewChanged += View_ViewChanged;

        //RebuildHand();
    }

    void View_ViewChanged(in SynchronizedViewChangedEventArgs<CardInstance, GameObject> eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Remove:
                var slot = eventArgs.OldItem.View.GetComponent<CardSlotUI>();
                if (slot != null)
                    DestroyCardSlotUI(slot);
                break;
            case NotifyCollectionChangedAction.Replace:
                break;
            case NotifyCollectionChangedAction.Reset:
                ClearHand();
                break;
            default:
                break;
        }

        if (player != null)
        {
            UpdateCardPlayability();
            _isLayoutDirty = true;
            UpdateLayoutAsync().Forget();
        }
    }

    private async UniTaskVoid UpdateLayoutAsync()
    {
        try
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            if (_isLayoutDirty && fanLayoutGroup != null)
                fanLayoutGroup.LayoutCards();
        }
        finally
        {
            // set it back to false regradless of success or failure
            _isLayoutDirty = false;
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
            CreateCardSlotUI(card);
        }

        Debug.Log($"HandUI rebuilt with {player.hand.Count} cards");
    }

    private CardSlotUI CreateCardSlotUI(CardInstance card)
    {
        if (cardSlotPrefab == null || cardContainer == null || cardSlotPrefab.cardUI == null)
        {
            Debug.LogError("HandUI: Prefab or container or cardUI is null!");
            return null;
        }

        var cardSlotUIObj = Instantiate(cardSlotPrefab, cardContainer);
        cardSlotUIObj.gameObject.SetActive(true);

        if (cardSlotUIObj != null)
        {
            cardSlotUIObj.cardUI.Initialize(battleManager, card, dragHandler, this);
            cardSlotUIList.Add(cardSlotUIObj);
            cardUIList.Add(cardSlotUIObj.cardUI);
        }
        else
        {
            Debug.LogError("HandUI: CardUI component not found on prefab!");
        }

        return cardSlotUIObj;
    }

    private void DestroyCardSlotUI(CardSlotUI slot)
    {
        cardSlotUIList.Remove(slot);
        cardUIList.Remove(slot.cardUI);
        Destroy(slot.gameObject);
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
        // cardUI is a child of cardSlotUI, so destroying cardSlotUI is sufficient
        cardUIList.Clear();
        foreach (var cardSlotUI in cardSlotUIList)
        {
            if (cardSlotUI != null)
            {
                Destroy(cardSlotUI.gameObject);
            }
        }
        cardSlotUIList.Clear();
    }

    /// <summary>
    /// Check if a screen position is within the HandUI bounds
    /// </summary>
    public bool IsPositionInHandUI(Vector2 screenPosition)
    {
        if (handRectTransform == null)
            return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            handRectTransform,
            screenPosition,
            cachedCamera
        );
    }

    /// <summary>
    /// Get the RectTransform of HandUI
    /// </summary>
    public RectTransform GetRectTransform()
    {
        return handRectTransform;
    }

    /// <summary>
    /// Clean up hand UI
    /// </summary>
    public void Cleanup()
    {
        ClearHand();
        player = null;
    }

    private void OnDestroy()
    {
        ClearHand();
        view?.Dispose();
    }
}
