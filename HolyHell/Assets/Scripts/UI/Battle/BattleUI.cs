using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;
using HolyHell.Battle;
using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;

/// <summary>
/// Main battle UI controller - manages all battle UI components
/// </summary>
public class BattleUI : MonoBehaviour, IUIInitializable
{
    [Header("Player UI")]
    [SerializeField] private EntityStatusUI playerStatusUI;
    [SerializeField] private GaugeDisplayUI gaugeDisplayUI;
    [SerializeField] private ActionPointUI actionPointUI;
    [SerializeField] private HandUI handUI;
    [SerializeField] private DeckCounterUI deckCounterUI;

    [Header("Enemy UI")]
    [SerializeField] private EnemyListUI enemyListUI;

    [Header("Battle Flow UI")]
    [SerializeField] private TurnIndicatorUI turnIndicatorUI;
    [SerializeField] private EndTurnButton endTurnButton;
    [SerializeField] private BattleResultUI battleResultUI;

    [Header("Target Selection")]
    [SerializeField] private TargetSelector targetSelector;

    private BattleManager battleManager;
    private CompositeDisposable disposables = new CompositeDisposable();
    private bool isComponentsInitialized = false;

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

        if (gaugeDisplayUI != null && battleManager.player != null)
            gaugeDisplayUI.Initialize(battleManager.player);

        if (actionPointUI != null && battleManager.player != null)
            actionPointUI.Initialize(battleManager.player);

        if (handUI != null && battleManager.player != null)
            handUI.Initialize(battleManager.player, OnCardClicked);

        if (deckCounterUI != null && battleManager.player != null)
            deckCounterUI.Initialize(battleManager.player);

        // Initialize enemy UI
        if (enemyListUI != null && battleManager.enemies != null)
            enemyListUI.Initialize(battleManager.enemies, OnEnemyClicked);

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

        isComponentsInitialized = false;
        Debug.Log("BattleUI: Components cleaned up");
    }

    /// <summary>
    /// Called when player clicks a card in hand
    /// </summary>
    private void OnCardClicked(CardInstance card)
    {
        if (targetSelector != null)
        {
            targetSelector.StartTargetSelection(card);
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

    /// <summary>
    /// Clean up UI when GameObject is destroyed
    /// </summary>
    private void OnDestroy()
    {
        CleanupComponents();
        disposables.Dispose();
        Debug.Log("BattleUI destroyed");
    }
}
