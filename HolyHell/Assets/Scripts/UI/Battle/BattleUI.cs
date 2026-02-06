using Cysharp.Threading.Tasks;
using HolyHell.Battle;
using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;
using HolyHell.UI.Battle;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main battle UI controller - manages all battle UI components
/// </summary>
public class BattleUI : MonoBehaviour, IUIInitializable
{
    [Header("Player UI")]
    [SerializeField] private EntityStatusUI playerStatusUI;
    [SerializeField] private BuffView playerBuffView;
    [SerializeField] private GaugeDisplayUI gaugeDisplayUI;
    [SerializeField] private ActionPointUI actionPointUI;
    [SerializeField] private HandUI handUI;
    [SerializeField] private DeckCounterUI deckCounterUI;
    [SerializeField] private CardPreviewUI cardPreviewUI;

    [Header("Enemy UI")]
    [SerializeField] private EnemyListUI enemyListUI;

    [Header("Battle Flow UI")]
    [SerializeField] private TurnIndicatorUI turnIndicatorUI;
    [SerializeField] private EndTurnButton endTurnButton;
    [SerializeField] private BattleResultUI battleResultUI;

    [Header("Target Selection")]
    [SerializeField] private TargetSelector targetSelector;

    [Header("Card Interaction")]
    [SerializeField] private CancelUseCardButtonUI cancelUseCardButtonUI;
    [SerializeField] private CardDragHandler cardDragHandler;

    private BattleManager battleManager;
    private InputAction cancelAction;
    private InputAction mouseLeftAction;
    private CompositeDisposable disposables = new CompositeDisposable();
    private bool isComponentsInitialized = false;

    private void OnEnable()
    {
        InputManager.Instance.PushActionMap("Battle");
    }

    private void OnDisable()
    {
        InputManager.Instance?.PopActionMap("Battle");
    }

    public async UniTask Init()
    {
        // Get BattleManager reference
        battleManager = await ServiceLocator.Instance.GetAsync<IBattleManager>() as BattleManager;

        if (battleManager == null)
        {
            Debug.LogError("BattleUI: Failed to get BattleManager!");
            return;
        }

        Debug.Log("BattleUI: waiting for battle to start...");

        // Subscribe to battle state changes
        battleManager.battleState.Subscribe(state =>
        {
            OnBattleStateChanged(state);
        }).AddTo(disposables);

        // Subscribe to card interaction state changes
        battleManager.cardInteractionState.Subscribe(state =>
        {
            OnCardInteractionStateChanged(state);
        }).AddTo(disposables);

        var inputMap = InputSystem.actions.FindActionMap("Battle");
        cancelAction = inputMap.FindAction("Cancel");
        cancelAction.performed += Input_Cancel;
        mouseLeftAction = inputMap.FindAction("MouseLeftButton");
        mouseLeftAction.performed += Input_MouseRaycastCheck;
        mouseLeftAction.canceled += Input_CloseCardPreview;
    }

    /// <summary>
    /// Called when card interaction state changes
    /// </summary>
    private void OnCardInteractionStateChanged(CardInteractionState state)
    {
        //Debug.Log($"BattleUI: Card interaction state changed to {state}");

        switch (state)
        {
            case CardInteractionState.SelectingTarget:
                // Enter target selection mode
                var card = battleManager.cardAwaitingUse.Value;
                if (card != null && targetSelector != null)
                {
                    targetSelector.StartTargetSelection(card);
                }
                break;
        }
    }

    /// <summary>
    /// Called when battle state changes
    /// </summary>
    private void OnBattleStateChanged(BattleState state)
    {
        Debug.Log($"BattleUI: Battle state changed to {state}");

        switch (state)
        {
            case BattleState.InProgress:
                // Battle has started, initialize all UI components
                InitializeComponents();
                break;

            case BattleState.Ended:
                // Battle ended, clean up all UI components
                CleanupComponents();
                break;
        }
    }

    /// <summary>
    /// Initialize all UI components with battle entities and systems
    /// Called when battle state becomes InProgress
    /// </summary>
    private void InitializeComponents()
    {
        if (isComponentsInitialized)
        {
            Debug.LogWarning("BattleUI: Components already initialized, cleaning up first...");
            CleanupComponents();
        }

        Debug.Log("BattleUI: Initializing components...");

        // Initialize player UI
        if (playerStatusUI != null && battleManager.player != null)
            playerStatusUI.Initialize(battleManager.player);

        if (playerBuffView != null && battleManager.player != null)
            playerBuffView.Initialize(battleManager.player);

        if (gaugeDisplayUI != null && battleManager.player != null)
            gaugeDisplayUI.Initialize(battleManager.player);

        if (actionPointUI != null && battleManager.player != null)
            actionPointUI.Initialize(battleManager.player);

        // Initialize enemy UI first (needed by other components)
        if (enemyListUI != null && battleManager.enemies != null)
            enemyListUI.Initialize(battleManager.enemies, OnEnemyClicked);

        // Initialize CardDragHandler (singleton managed by BattleUI)
        if (cardDragHandler != null)
            cardDragHandler.Initialize(battleManager, handUI, enemyListUI);

        if (handUI != null && battleManager.player != null)
            handUI.Initialize(battleManager, cardDragHandler, battleManager.player);

        if (deckCounterUI != null && battleManager.player != null)
            deckCounterUI.Initialize(battleManager.player);

        if (cardPreviewUI != null)
            cardPreviewUI.Initialize(battleManager);

        // Initialize battle flow UI
        if (turnIndicatorUI != null && battleManager.turnSystem != null)
            turnIndicatorUI.Initialize(battleManager.turnSystem);

        if (endTurnButton != null)
            endTurnButton.Initialize(battleManager);

        if (battleResultUI != null)
            battleResultUI.Initialize(battleManager);

        // Initialize target selector
        if (targetSelector != null)
            targetSelector.Initialize(battleManager, enemyListUI);

        // Initialize cancel button (Use button is now handled by CardUI)
        if (cancelUseCardButtonUI != null)
        {
            cancelUseCardButtonUI.Initialize(battleManager);
            cancelUseCardButtonUI.onCancelButtonClicked += OnCancelButtonClicked;
        }

        isComponentsInitialized = true;
        Debug.Log("BattleUI: Components initialized");
    }

    /// <summary>
    /// Clean up all UI components
    /// Called when battle ends or before re-initializing
    /// </summary>
    private void CleanupComponents()
    {
        if (!isComponentsInitialized)
        {
            return;
        }

        Debug.Log("BattleUI: Cleaning up components...");

        // Clean up each UI component that has Cleanup method
        if (playerStatusUI != null)
            playerStatusUI.Cleanup();

        if (gaugeDisplayUI != null)
            gaugeDisplayUI.Cleanup();

        if (actionPointUI != null)
            actionPointUI.Cleanup();

        if (handUI != null)
            handUI.Cleanup();

        if (deckCounterUI != null)
            deckCounterUI.Cleanup();

        if (enemyListUI != null)
            enemyListUI.Cleanup();

        if (turnIndicatorUI != null)
            turnIndicatorUI.Cleanup();

        if (endTurnButton != null)
            endTurnButton.Cleanup();

        if (battleResultUI != null)
            battleResultUI.Cleanup();

        if (targetSelector != null)
            targetSelector.Cleanup();

        if (cardDragHandler != null)
            cardDragHandler.Cleanup();

        isComponentsInitialized = false;
    }

    /// <summary>
    /// Called when Cancel button is clicked
    /// </summary>
    private void OnCancelButtonClicked()
    {
        CancelCardInteraction();
    }

    /// <summary>
    /// Cancel current card interaction
    /// </summary>
    private void CancelCardInteraction()
    {
        battleManager.cardAwaitingUse.Value = null;
        battleManager.currentSelectedCard.Value = null;
        battleManager.cardInteractionState.Value = CardInteractionState.Idle;

        if (targetSelector != null)
        {
            targetSelector.CancelTargetSelection();
        }
    }

    /// <summary>
    /// Called when player clicks an enemy
    /// </summary>
    private void OnEnemyClicked(EnemyEntity enemy)
    {
        if (targetSelector != null)
        {
            targetSelector.OnTargetSelected(enemy);
        }
    }

    private void Input_Cancel(InputAction.CallbackContext ctx)
    {
        CancelCardInteraction();
    }

    private void Input_MouseRaycastCheck(InputAction.CallbackContext ctx)
    {
        if (targetSelector.IsSelectingTarget)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
            int enemyLayer = LayerMask.GetMask("Enemy");
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Camera.main.farClipPlane, enemyLayer);
            Collider2D hitCollider = hit.collider;
            if (hitCollider != null)
            {
                var enemy = hitCollider.GetComponentInParent<EnemyEntity>(); // Assuming collider parent has EnemyEntity
                if (enemy != null && enemy.CanBeTargeted())
                {
                    targetSelector.OnTargetSelected(enemy);
                }
            }
        }
    }

    private void Input_CloseCardPreview(InputAction.CallbackContext ctx)
    {
        battleManager.currentPreviewCard.Value = null;
    }

    /// <summary>
    /// Clean up UI when GameObject is destroyed
    /// </summary>
    private void OnDestroy()
    {
        CleanupComponents();
        cancelAction.Dispose();
        cancelAction = null;
        mouseLeftAction.Dispose();
        mouseLeftAction = null;
        disposables.Dispose();
        Debug.Log("BattleUI destroyed");
    }
}
