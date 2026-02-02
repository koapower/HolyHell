using HolyHell.Battle;
using HolyHell.Battle.Card;
using HolyHell.UI.Battle;
using R3;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Displays a single card in hand
/// Handles click, hover, and drag events
/// </summary>
public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Card Info")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image elementImage;
    [SerializeField] private Color angelColor = new Color(1f, 0.9f, 0.5f); // Light gold
    [SerializeField] private Color demonColor = new Color(0.6f, 0.2f, 0.8f); // Dark purple]
    [SerializeField] private Color blissColor = Color.purple;
    [SerializeField] private Color dominationColor = Color.red;
    [SerializeField] private Color enlightendColor = Color.yellow;
    [SerializeField] private Color despairColor = Color.cyan;
    [SerializeField] private Color ravenousColor = Color.green;

    [Header("Gauge Display")]
    [SerializeField] private Image angelBg;
    [SerializeField] private Image demonBg;
    [SerializeField] private TextMeshProUGUI angelGaugeText;
    [SerializeField] private TextMeshProUGUI demonGaugeText;

    [Header("Visual")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.8f);
    [SerializeField] private Color unplayableColor = Color.gray;

    [Header("Animation")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverTransitionSpeed = 20f;
    [SerializeField] private Vector2 selectedPosition = new Vector2(0, 50);
    [SerializeField] private float selectedTransitionSpeed = 20f;

    [Header("Use Button")]
    [SerializeField] private GameObject useButtonObject; // The Use button UI (child of this card)
    [SerializeField] private Button useButton; // Reference to the button component


    private BattleManager battleManager;
    private HandUI handUI;
    private CardInstance card;
    private CardDragHandler dragHandler; // Get from BattleUI
    private bool isPlayable = true;
    private bool isHovered = false;
    private bool isSelected = false;
    private Vector3 originalScale;
    private Vector2 originalAnchoredPosition;
    private IDisposable _delayLongTapSubscription;
    private CompositeDisposable disposables = new CompositeDisposable();

    // State machine variables
    private CardInteractionState currentState = CardInteractionState.Idle;
    private bool hasMovedOutOfHandUI = false;
    private Transform originalParent;
    private int originalSiblingIndex;
    private RectTransform rectTransform;

    private void Awake()
    {
        originalScale = transform.localScale;
        rectTransform = GetComponent<RectTransform>();
        originalAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void Initialize(BattleManager battleManager, CardInstance cardInstance, CardDragHandler dragHandler, HandUI handUI)
    {
        this.battleManager = battleManager;
        this.dragHandler = dragHandler;
        this.handUI = handUI;
        card = cardInstance;

        // Subscribe to selection state
        battleManager.currentSelectedCard.Subscribe(card =>
        {
            isSelected = card != null && card.instanceId == this.card.instanceId;
        }).AddTo(disposables);

        Observable
           .CombineLatest(
               battleManager.currentSelectedCard,
               battleManager.cardInteractionState,
               (selectedCard, state) =>
                   selectedCard != null &&
                   selectedCard.instanceId == this.card.instanceId &&
                   state == CardInteractionState.AwaitingUse
           )
           .DistinctUntilChanged()
           .Subscribe(ShowUseButton)
           .AddTo(disposables);

        // Setup Use button click handler
        if (useButton != null)
        {
            useButton.onClick.AddListener(OnUseButtonClicked);
        }

        // Initially hide use button
        if (useButtonObject != null)
        {
            useButtonObject.SetActive(false);
        }

        if (card == null || card.cardData == null)
        {
            Debug.LogError("CardUI: Card or cardData is null!");
            return;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Card name
        if (cardNameText != null)
        {
            cardNameText.text = card.DisplayName;
            cardNameText.color = card.cardData.Faction switch
            {
                Faction.Angel => angelColor,
                Faction.Demon => demonColor,
                _ => Color.white
            };
        }

        // Cost
        if (costText != null)
        {
            costText.text = card.ActionCost.ToString();
        }

        // Description
        if (descriptionText != null)
        {
            descriptionText.text = card.cardData.Description;
        }

        // Element
        if (elementImage != null)
        {
            elementImage.color = card.cardData.ElementType switch
            {
                ElementType.Bliss => blissColor,
                ElementType.Domination => dominationColor,
                ElementType.Enlightened => enlightendColor,
                ElementType.Despair => despairColor,
                ElementType.Ravenous => ravenousColor,
                _ => Color.white
            };
        }

        // Gauge changes
        if (angelGaugeText != null)
        {
            if (card.AngelGaugeIncrease != 0)
            {
                angelGaugeText.text = card.AngelGaugeIncrease > 0
                    ? $"+{card.AngelGaugeIncrease}"
                    : $"{card.AngelGaugeIncrease}";
            }
            else
            {
                angelGaugeText.text = "";
            }
        }

        if (demonGaugeText != null)
        {
            if (card.DemonGaugeIncrease != 0)
            {
                demonGaugeText.text = card.DemonGaugeIncrease > 0
                    ? $"+{card.DemonGaugeIncrease}"
                    : $"{card.DemonGaugeIncrease}";
            }
            else
            {
                demonGaugeText.text = "";
            }
        }

        if (angelBg != null)
        {
            angelBg.gameObject.SetActive(card.AngelGaugeIncrease != 0);
        }

        if (demonBg != null)
        {
            demonBg.gameObject.SetActive(card.DemonGaugeIncrease != 0);
        }

        // Set card frame color based on faction
        if (cardFrame != null)
        {
            cardFrame.color = GetFactionColor(card.cardData.Faction);
        }
    }

    private Color GetFactionColor(Faction faction)
    {
        return faction switch
        {
            Faction.Angel => new Color(1f, 0.9f, 0.5f), // Light gold
            Faction.Demon => new Color(0.6f, 0.2f, 0.8f), // Dark purple
            _ => Color.white // Neutral
        };
    }

    /// <summary>
    /// Set whether this card can be played
    /// </summary>
    public void SetPlayable(bool playable)
    {
        isPlayable = playable;
        UpdateVisual();
    }

    public void SetCardInteractability(bool canInteract)
    {
        // default is true
        canvasGroup.blocksRaycasts = canInteract;
        canvasGroup.interactable = canInteract;
    }

    private void UpdateVisual()
    {
        if (backgroundImage == null) return;

        if (!isPlayable)
        {
            backgroundImage.color = unplayableColor;
        }
        else if (isHovered)
        {
            backgroundImage.color = hoverColor;
        }
        else
        {
            backgroundImage.color = normalColor;
        }
    }

    private void Update()
    {
        // Handle state machine updates
        UpdateStateMachine();

        // Smooth hover scale animation
        Vector3 targetScale = (isHovered && currentState == CardInteractionState.Idle) ? originalScale * hoverScale : originalScale;
        Vector2 targetPosition = isSelected ? selectedPosition : originalAnchoredPosition;

        // Don't animate if dragging
        if (currentState != CardInteractionState.Dragging)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * hoverTransitionSpeed);
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * selectedTransitionSpeed);
        }
    }

    private void UpdateStateMachine()
    {
        switch (currentState)
        {
            case CardInteractionState.Pressing:
                // Check if pointer has moved out of HandUI
                if (handUI != null && Mouse.current != null && !handUI.IsPositionInHandUI(Mouse.current.position.ReadValue()))
                {
                    // Transition to Dragging state
                    EnterDraggingState();
                }
                break;

            case CardInteractionState.Dragging:
                // Update card position to follow mouse
                UpdateDragPosition();

                // Check if dragged back into HandUI
                if (handUI != null && Mouse.current != null && handUI.IsPositionInHandUI(Mouse.current.position.ReadValue()))
                {
                    // Cancel dragging, return to Pressing state
                    ExitDraggingState();
                    currentState = CardInteractionState.Idle; // Back to idle since we're no longer pressing
                }
                break;
        }
    }

    private void EnterDraggingState()
    {
        currentState = CardInteractionState.Dragging;
        hasMovedOutOfHandUI = true;

        // Cancel long-tap preview
        CancelLongTapPreview();
        battleManager.currentPreviewCard.Value = null;

        // Update BattleManager state
        battleManager.currentSelectedCard.Value = card;
        battleManager.cardInteractionState.Value = CardInteractionState.Dragging;

        // Store original parent and move card to higher layer
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // Move to end of parent to render on top (or move to HandUI if needed)
        transform.SetAsLastSibling();

        Debug.Log($"Entered dragging state for card: {card.DisplayName}");
    }

    private void ExitDraggingState()
    {
        currentState = CardInteractionState.Idle;
        hasMovedOutOfHandUI = false;

        // Restore original parent and sibling index
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
        }

        // Reset position and rotation
        rectTransform.anchoredPosition = originalAnchoredPosition;
        transform.localRotation = Quaternion.identity;
        transform.localScale = originalScale;

        // Update BattleManager state
        battleManager.cardInteractionState.Value = CardInteractionState.Idle;

        Debug.Log($"Exited dragging state for card: {card.DisplayName}");
    }

    private void UpdateDragPosition()
    {
        if (Mouse.current == null) return;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        bool requiresTarget = dragHandler != null && dragHandler.DoesCardRequireTarget(card);

        if (!requiresTarget)
        {
            // No target required - card follows mouse
            Vector3 worldPos;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                rectTransform,
                mouseScreenPos,
                dragHandler.GetCanvasCamera(),
                out worldPos
            );
            rectTransform.position = worldPos;
            rectTransform.localRotation = Quaternion.identity;
        }

        // Delegate drag line and target detection to drag handler
        if (dragHandler != null)
        {
            dragHandler.UpdateDrag(rectTransform);
        }
    }

    private void CancelLongTapPreview()
    {
        _delayLongTapSubscription?.Dispose();
        _delayLongTapSubscription = null;
    }

    private void ShowUseButton(bool show)
    {
        if (useButtonObject != null)
        {
            useButtonObject.SetActive(show);
        }
    }

    private void OnUseButtonClicked()
    {
        Debug.Log($"Use button clicked for card: {card.DisplayName}");

        // Check if card requires target
        bool requiresTarget = dragHandler != null && dragHandler.DoesCardRequireTarget(card);

        if (!requiresTarget)
        {
            // Play card immediately without target
            battleManager.PlayCard(card, null);
            battleManager.cardAwaitingUse.Value = null;
            battleManager.currentSelectedCard.Value = null;
            battleManager.cardInteractionState.Value = CardInteractionState.Idle;
        }
        else
        {
            // Enter target selection mode
            battleManager.cardInteractionState.Value = CardInteractionState.SelectingTarget;
            // Note: BattleUI will handle calling TargetSelector.StartTargetSelection
            // through its subscription to cardInteractionState
        }
    }

    // IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        // Click is only processed if we didn't drag
        if (hasMovedOutOfHandUI)
        {
            hasMovedOutOfHandUI = false;
            return;
        }

        if (!isPlayable)
        {
            Debug.Log($"Card {card.DisplayName} is not playable!");
            return;
        }

        // This will be triggered after OnPointerUp if it's a valid click
        // We'll handle this in OnPointerUp instead
    }

    // IPointerEnterHandler
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentState == CardInteractionState.Idle)
        {
            isHovered = true;
            UpdateVisual();
        }
    }

    // IPointerExitHandler
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisual();
    }

    // IPointerDownHandler
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPlayable) return;

        currentState = CardInteractionState.Pressing;

        // Start long tap timer for preview (0.3 seconds)
        _delayLongTapSubscription = Observable.Timer(TimeSpan.FromSeconds(0.3f))
            .Subscribe(_ =>
            {
                // Only show preview if still in Pressing state (haven't started dragging)
                if (currentState == CardInteractionState.Pressing)
                {
                    battleManager.currentPreviewCard.Value = card;
                }
            }).AddTo(this);
    }

    // IPointerUpHandler
    public void OnPointerUp(PointerEventData eventData)
    {
        CancelLongTapPreview();

        if (!isPlayable) return;

        // Determine what to do based on current state
        switch (currentState)
        {
            case CardInteractionState.Pressing:
                // Released within HandUI without dragging - this is a click
                HandleCardClick();
                currentState = CardInteractionState.Idle;
                break;

            case CardInteractionState.Dragging:
                // Released while dragging
                HandleDragRelease();
                break;

            case CardInteractionState.Idle:
                // Shouldn't happen, but handle it
                break;
        }
    }

    private void HandleCardClick()
    {
        Debug.Log($"Card clicked: {card.DisplayName}");

        // Set as awaiting use button
        battleManager.cardAwaitingUse.Value = card;
        battleManager.currentSelectedCard.Value = card;
        battleManager.cardInteractionState.Value = CardInteractionState.AwaitingUse;
    }

    private void HandleDragRelease()
    {
        if (Mouse.current == null)
        {
            ExitDraggingState();
            return;
        }

        bool requiresTarget = dragHandler != null && dragHandler.DoesCardRequireTarget(card);
        bool inHandUI = handUI != null && handUI.IsPositionInHandUI(Mouse.current.position.ReadValue());

        if (inHandUI)
        {
            // Released back in HandUI - cancel
            ExitDraggingState();
            return;
        }

        if (!requiresTarget)
        {
            // No target required - play card immediately
            Debug.Log($"Playing card without target via drag: {card.DisplayName}");
            bool success = battleManager.PlayCard(card, null);
            ExitDraggingState();
        }
        else
        {
            // Requires target - get locked target from drag handler
            var targetEnemy = dragHandler?.GetLockedTarget();
            bool isLocked = dragHandler != null && dragHandler.IsLockedToTarget;

            if (isLocked && targetEnemy != null)
            {
                // Play card on the target
                Debug.Log($"Playing card on target: {card.DisplayName} -> {targetEnemy.enemyData?.DisplayName}");
                bool success = battleManager.PlayCard(card, targetEnemy);
                ExitDraggingState();
            }
            else
            {
                // No valid target - return card to hand with animation
                Debug.Log($"No valid target - returning card to hand");
                ExitDraggingState();
            }
        }
    }

    private void OnDestroy()
    {
        CancelLongTapPreview();

        if (useButton != null)
        {
            useButton.onClick.RemoveListener(OnUseButtonClicked);
        }

        // Note: dragHandler is managed by BattleUI, don't cleanup here

        disposables.Dispose();
    }
}
