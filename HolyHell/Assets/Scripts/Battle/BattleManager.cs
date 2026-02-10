using Cysharp.Threading.Tasks;
using HolyHell.Battle.Card;
using HolyHell.Battle.Enemy;
using HolyHell.Battle.Entity;
using R3;
using System.Collections.Generic;
using UnityEngine;

namespace HolyHell.Battle
{
    /// <summary>
    /// Main battle controller - orchestrates entire battle flow
    /// Implements IGameService for service locator pattern
    /// </summary>
    public class BattleManager : IBattleManager
    {
        // Battle entities
        public PlayerEntity player;
        public List<BattleEntity> allies = new List<BattleEntity>();
        public List<EnemyEntity> enemies = new List<EnemyEntity>();
        public IReadOnlyList<BattleEntity> Allies => allies;
        public IReadOnlyList<EnemyEntity> Enemies => enemies;

        // Systems
        public TurnSystem turnSystem;
        public CardEffectExecutor cardEffectExecutor;

        // Battle variables
        public ReactiveProperty<BattleState> battleState = new ReactiveProperty<BattleState>(BattleState.NotStarted);
        public ReactiveProperty<CardInstance> currentPreviewCard = new ReactiveProperty<CardInstance>();
        public ReactiveProperty<CardInstance> currentSelectedCard = new ReactiveProperty<CardInstance>();

        // Card interaction state
        public ReactiveProperty<CardInteractionState> cardInteractionState = new ReactiveProperty<CardInteractionState>(CardInteractionState.Idle);
        public ReactiveProperty<CardInstance> cardAwaitingUse = new ReactiveProperty<CardInstance>();

        // managers
        private IAssetLoader assetLoader;
        private ITableManager tableManager;

        public BattleManager(IAssetLoader assetLoader, ITableManager tableManager)
        {
            this.assetLoader = assetLoader;
            this.tableManager = tableManager;
        }

        /// <summary>
        /// Initialize service (called by ServiceLocator)
        /// </summary>
        public UniTask Init()
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Start a new battle
        /// </summary>
        public async UniTask StartBattle(List<string> playerDeckCardIds, List<EnemySetupInfo> enemyInfos)
        {
            Debug.Log("===== BATTLE START =====");

            // Clean up previous battle if any
            CleanupBattle();

            battleState.Value = BattleState.Initializing;

            // Initialize player
            await InitializePlayer(playerDeckCardIds);

            // Initialize enemies
            await InitializeEnemies(enemyInfos);

            // Create turn system
            turnSystem = new TurnSystem(this, player, enemies);

            // Create card executor
            cardEffectExecutor = new CardEffectExecutor(this, player);

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
        /// Stop the current battle (forced end)
        /// </summary>
        public void StopBattle()
        {
            Debug.Log("===== BATTLE STOPPED =====");
            battleState.Value = BattleState.Ended;
            CleanupBattle();
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
        /// Initialize enemy entities from enemy setup infos.
        /// Skills are loaded from EnemyBehavior.csv (referenced by EnemyRow.BehaviorId).
        /// </summary>
        private async UniTask InitializeEnemies(List<EnemySetupInfo> enemyInfos)
        {
            var enemyTable    = tableManager.GetTable<EnemyRow>();
            var behaviorTable = tableManager.GetTable<EnemyBehaviorRow>();
            var skillTable    = tableManager.GetTable<MonsterSkillRow>();

            foreach (var enemyInfo in enemyInfos)
            {
                var enemyData = enemyTable.GetRow(e => e.Id == enemyInfo.Id);
                if (enemyData == null)
                {
                    Debug.LogError($"[Battle] Enemy data not found for ID: {enemyInfo.Id}");
                    continue;
                }

                // Look up behavior row
                var behaviorRow = behaviorTable.GetRow(b => b.Id == enemyData.BehaviorId);
                if (behaviorRow == null)
                    Debug.LogError($"[Battle] EnemyBehavior row not found: BehaviorID={enemyData.BehaviorId} (enemy: {enemyData.DisplayName})");

                // Resolve skills from behavior row (up to 6 slots)
                var skills = new List<EnemySkill>();
                if (behaviorRow != null)
                {
                    for (int i = 1; i <= 6; i++)
                    {
                        var (skillId, _) = behaviorRow.GetSkillEntry(i);
                        if (string.IsNullOrEmpty(skillId)) continue;

                        var skillRow = skillTable.GetRow(s => s.Id == skillId);
                        if (skillRow != null)
                            skills.Add(new EnemySkill(skillId, skillRow));
                        else
                            Debug.LogWarning($"[Battle] MonsterSkill not found: '{skillId}' (enemy: {enemyData.DisplayName}, slot {i})");
                    }
                }

                // Instantiate enemy prefab
                var enemyGO = await assetLoader.InstaniateAsync("Res:/Characters/EnemyEntity.prefab", null);
                enemyGO.name = $"Enemy_{enemyData.DisplayName}";
                enemyGO.transform.position = enemyInfo.worldPosition;
                var enemy = enemyGO.GetComponent<EnemyEntity>();

                // Initialize with behavior and skills
                enemy.Initialize(this, enemyData, behaviorRow, skills);

                enemies.Add(enemy);
                Debug.Log($"[Battle] Enemy ready: {enemyData.DisplayName} | HP={enemy.hp.Value} | BehaviorID={enemyData.BehaviorId} | Skills={skills.Count}");
            }

            await UniTask.Yield();
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

            // Clean up after battle ends
            CleanupBattle();
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
        /// Clean up current battle (entities and systems)
        /// Called before starting a new battle or when battle ends
        /// </summary>
        private void CleanupBattle()
        {
            Debug.Log("BattleManager cleaning up battle...");

            // Dispose TurnSystem ReactiveProperties
            if (turnSystem != null)
            {
                turnSystem.currentPhase?.Dispose();
                turnSystem.turnNumber?.Dispose();
                turnSystem = null;
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
            cardEffectExecutor = null;

            Debug.Log("BattleManager battle cleanup complete");
        }

        /// <summary>
        /// Clean up battle resources (called by ServiceLocator on shutdown)
        /// </summary>
        public void Dispose()
        {
            Debug.Log("BattleManager disposing...");

            // Clean up current battle
            CleanupBattle();

            // Dispose ReactiveProperties
            battleState?.Dispose();
            battleState = null;

            currentPreviewCard?.Dispose();
            currentPreviewCard = null;

            currentSelectedCard?.Dispose();
            currentSelectedCard = null;

            cardInteractionState?.Dispose();
            cardInteractionState = null;

            cardAwaitingUse?.Dispose();
            cardAwaitingUse = null;

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

    /// <summary>
    /// Card interaction state enum
    /// </summary>
    public enum CardInteractionState
    {
        Idle,           // No interaction
        Pressing,       // Holding down (within HandUI)
        Dragging,       // Dragging (left HandUI)
        AwaitingUse,    // Waiting for Use button click
        SelectingTarget // Selecting target
    }
}
