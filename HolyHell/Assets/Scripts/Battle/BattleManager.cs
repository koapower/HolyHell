using R3;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Main battle controller - orchestrates entire battle flow
/// Implements IGameService for service locator pattern
/// </summary>
public class BattleManager : IGameService
{
    // Battle entities
    public PlayerEntity player;
    public List<EnemyEntity> enemies = new List<EnemyEntity>();

    // Systems
    public TurnSystem turnSystem;
    public CardEffectExecutor cardEffectExecutor;

    // Battle state
    public ReactiveProperty<BattleState> battleState = new ReactiveProperty<BattleState>(BattleState.NotStarted);

    // Table managers
    private ITableManager tableManager;

    /// <summary>
    /// Initialize service (called by ServiceLocator)
    /// </summary>
    public async UniTask Init()
    {
        tableManager = await ServiceLocator.Instance.GetAsync<ITableManager>();
        Debug.Log("BattleManager initialized");
    }

    /// <summary>
    /// Start a new battle
    /// </summary>
    public async UniTask StartBattle(List<string> playerDeckCardIds, List<string> enemyIds)
    {
        Debug.Log("===== BATTLE START =====");

        battleState.Value = BattleState.Initializing;

        // Initialize player
        await InitializePlayer(playerDeckCardIds);

        // Initialize enemies
        await InitializeEnemies(enemyIds);

        // Create turn system
        turnSystem = new TurnSystem(this, player, enemies);

        // Create card executor
        cardEffectExecutor = new CardEffectExecutor(player);

        // Set initial enemy intents
        foreach (var enemy in enemies)
        {
            enemy.DetermineIntent();
        }

        battleState.Value = BattleState.InProgress;

        // Start first turn
        turnSystem.StartPlayerTurn();
    }

    /// <summary>
    /// Initialize player entity
    /// </summary>
    private async UniTask InitializePlayer(List<string> deckCardIds)
    {
        // Create player GameObject if needed (for MonoBehaviour)
        var playerGO = new GameObject("Player");
        player = playerGO.AddComponent<PlayerEntity>();

        // Set initial stats
        player.maxHp.Value = 100;
        player.hp.Value = 100;
        player.shield.Value = 0;
        player.angelGauge.Value = 50;
        player.demonGauge.Value = 50;
        player.maxActionPoint.Value = 99;

        // Initialize deck
        await player.Initialize(deckCardIds);

        Debug.Log($"Player initialized: HP={player.hp.Value}, Angel={player.angelGauge.Value}, Demon={player.demonGauge.Value}");
    }

    /// <summary>
    /// Initialize enemy entities from enemy IDs
    /// </summary>
    private async UniTask InitializeEnemies(List<string> enemyIds)
    {
        var enemyTable = tableManager.GetTable<EnemyRow>();
        var skillTable = tableManager.GetTable<MonsterSkillRow>();

        foreach (var enemyId in enemyIds)
        {
            var enemyData = enemyTable.GetRow(e => e.Id == enemyId);
            if (enemyData == null)
            {
                Debug.LogError($"Enemy data not found: {enemyId}");
                continue;
            }

            // Create enemy GameObject
            var enemyGO = new GameObject($"Enemy_{enemyData.DisplayName}");
            var enemy = enemyGO.AddComponent<EnemyEntity>();

            // Load skills
            var skillIds = enemyData.GetSkillIds();
            var skills = new List<MonsterSkillRow>();
            foreach (var skillId in skillIds)
            {
                var skill = skillTable.GetRow(s => s.Id == skillId.Trim());
                if (skill != null)
                {
                    skills.Add(skill);
                }
            }

            // Initialize enemy
            enemy.Initialize(enemyData, skills);

            enemies.Add(enemy);

            Debug.Log($"Enemy initialized: {enemyData.DisplayName}, HP={enemy.hp.Value}, Skills={skills.Count}");
        }

        await UniTask.Yield(); // Ensure async
    }

    /// <summary>
    /// Player plays a card
    /// </summary>
    public bool PlayCard(CardInstance card, BattleEntity target)
    {
        // Check if it's player's turn
        if (turnSystem.currentPhase.Value != TurnPhase.PlayerTurn)
        {
            Debug.LogWarning("Not player's turn!");
            return false;
        }

        // Check if card is in hand
        if (!player.hand.Contains(card))
        {
            Debug.LogWarning("Card not in hand!");
            return false;
        }

        // Check action point cost
        if (player.actionPoint.Value < card.ActionCost)
        {
            Debug.LogWarning($"Not enough action points! Need {card.ActionCost}, have {player.actionPoint.Value}");
            return false;
        }

        // Execute card
        Debug.Log($"Playing card: {card.DisplayName} (Cost: {card.ActionCost})");

        cardEffectExecutor.ExecuteCard(card, target);

        // Deduct action points
        player.actionPoint.Value -= card.ActionCost;

        // Exhaust card (move to discard)
        player.deckManager.ExhaustCard(card);

        Debug.Log($"Action points remaining: {player.actionPoint.Value}");

        // Check battle end
        CheckBattleEnd();

        return true;
    }

    /// <summary>
    /// Player ends their turn manually
    /// </summary>
    public void EndPlayerTurnManually()
    {
        if (turnSystem.currentPhase.Value == TurnPhase.PlayerTurn)
        {
            turnSystem.EndPlayerTurn();
        }
    }

    /// <summary>
    /// Check if battle has ended (win/lose condition)
    /// </summary>
    public bool CheckBattleEnd()
    {
        // Check player defeat
        if (player.hp.Value <= 0)
        {
            OnBattleEnd(false);
            return true;
        }

        // Check all enemies defeated
        bool allEnemiesDead = true;
        foreach (var enemy in enemies)
        {
            if (enemy.hp.Value > 0)
            {
                allEnemiesDead = false;
                break;
            }
        }

        if (allEnemiesDead)
        {
            OnBattleEnd(true);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Handle battle end
    /// </summary>
    private void OnBattleEnd(bool playerWon)
    {
        battleState.Value = BattleState.Ended;

        if (playerWon)
        {
            Debug.Log("===== VICTORY! =====");
        }
        else
        {
            Debug.Log("===== DEFEAT! =====");
        }
    }

    /// <summary>
    /// Get a random living enemy (for targeting)
    /// </summary>
    public EnemyEntity GetRandomEnemy()
    {
        var livingEnemies = new List<EnemyEntity>();
        foreach (var enemy in enemies)
        {
            if (enemy.hp.Value > 0)
            {
                livingEnemies.Add(enemy);
            }
        }

        if (livingEnemies.Count == 0)
        {
            return null;
        }

        return livingEnemies[Random.Range(0, livingEnemies.Count)];
    }

    /// <summary>
    /// Clean up battle resources
    /// </summary>
    public void Dispose()
    {
        Debug.Log("BattleManager disposing...");

        // Dispose ReactiveProperties
        battleState?.Dispose();

        // Dispose TurnSystem ReactiveProperties
        if (turnSystem != null)
        {
            turnSystem.currentPhase?.Dispose();
            turnSystem.turnNumber?.Dispose();
        }

        // Destroy player GameObject (OnDestroy will handle ReactiveProperty disposal)
        if (player != null)
        {
            Object.Destroy(player.gameObject);
            player = null;
        }

        // Destroy enemy GameObjects (OnDestroy will handle ReactiveProperty disposal)
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Object.Destroy(enemy.gameObject);
            }
        }
        enemies.Clear();

        // Clear systems
        turnSystem = null;
        cardEffectExecutor = null;

        Debug.Log("BattleManager disposed");
    }
}

/// <summary>
/// Battle state enum
/// </summary>
public enum BattleState
{
    NotStarted,
    Initializing,
    InProgress,
    Ended
}
