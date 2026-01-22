using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

/// <summary>
/// Main battle UI controller - manages all battle UI components
/// </summary>
public class BattleUI : MonoBehaviour
{
    [Header("Player UI")]
    [SerializeField] private PlayerStatusUI playerStatusUI;
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

    /// <summary>
    /// Initialize all UI components with battle manager
    /// </summary>
    public void Initialize(BattleManager manager)
    {
        battleManager = manager;

        Debug.Log("BattleUI initializing...");

        // Initialize player UI
        if (playerStatusUI != null)
            playerStatusUI.Initialize(battleManager.player);

        if (gaugeDisplayUI != null)
            gaugeDisplayUI.Initialize(battleManager.player);

        if (actionPointUI != null)
            actionPointUI.Initialize(battleManager.player);

        if (handUI != null)
            handUI.Initialize(battleManager.player, OnCardClicked);

        if (deckCounterUI != null)
            deckCounterUI.Initialize(battleManager.player);

        // Initialize enemy UI
        if (enemyListUI != null)
            enemyListUI.Initialize(battleManager.enemies, OnEnemyClicked);

        // Initialize battle flow UI
        if (turnIndicatorUI != null)
            turnIndicatorUI.Initialize(battleManager.turnSystem);

        if (endTurnButton != null)
            endTurnButton.Initialize(battleManager);

        if (battleResultUI != null)
            battleResultUI.Initialize(battleManager);

        // Initialize target selector
        if (targetSelector != null)
            targetSelector.Initialize(battleManager, enemyListUI);

        Debug.Log("BattleUI initialized");
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
    /// Clean up UI
    /// </summary>
    private void OnDestroy()
    {
        disposables.Dispose();
        Debug.Log("BattleUI destroyed");
    }
}
