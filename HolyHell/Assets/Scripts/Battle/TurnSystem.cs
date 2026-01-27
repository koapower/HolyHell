using R3;
using System.Collections.Generic;
using UnityEngine;
using HolyHell.Battle.Entity;

namespace HolyHell.Battle
{
    /// <summary>
    /// Manages turn flow in battle
    /// </summary>
    public class TurnSystem
    {
        private PlayerEntity player;
        private List<EnemyEntity> enemies;
        private BattleManager battleManager;

        // Turn state
        public ReactiveProperty<TurnPhase> currentPhase = new ReactiveProperty<TurnPhase>(TurnPhase.BattleStart);
        public ReactiveProperty<int> turnNumber = new ReactiveProperty<int>(0);

        public TurnSystem(BattleManager manager, PlayerEntity playerEntity, List<EnemyEntity> enemyEntities)
        {
            battleManager = manager;
            player = playerEntity;
            enemies = enemyEntities;
        }

        /// <summary>
        /// Start a new player turn
        /// </summary>
        public void StartPlayerTurn()
        {
            turnNumber.Value++;
            currentPhase.Value = TurnPhase.PlayerTurn;

            Debug.Log($"=== Turn {turnNumber.Value} - Player Turn ===");

            // Reset action points
            player.actionPoint.Value = player.maxActionPoint.Value;

            // Trigger OnTurnStart for player buffs
            player.buffHandler.OnTurnStart();

            // Draw cards
            player.deckManager.DrawCards(5);

            Debug.Log($"Player: {player.actionPoint.Value} action points, {player.hand.Count} cards in hand");
        }

        /// <summary>
        /// End player turn
        /// </summary>
        public void EndPlayerTurn()
        {
            Debug.Log("=== Player Turn End ===");

            // Trigger OnTurnEnd for player buffs
            player.buffHandler.OnTurnEnd();

            // Discard hand
            player.deckManager.DiscardHand();

            // Reset shield at end of turn (optional, depends on game design)
            // player.shield.Value = 0;

            // Check if battle is over
            if (battleManager.CheckBattleEnd())
            {
                return;
            }

            // Move to enemy turn
            StartEnemyTurn();
        }

        /// <summary>
        /// Start enemy turn
        /// </summary>
        public void StartEnemyTurn()
        {
            currentPhase.Value = TurnPhase.EnemyTurn;

            Debug.Log("=== Enemy Turn ===");

            // Each enemy executes their intent
            foreach (var enemy in enemies)
            {
                if (enemy.hp.Value <= 0)
                {
                    continue; // Skip dead enemies
                }

                // Trigger OnTurnStart for enemy buffs
                enemy.buffHandler.OnTurnStart();

                // Execute intent
                enemy.ExecuteIntent(player);
            }

            EndEnemyTurn();
        }

        /// <summary>
        /// End enemy turn
        /// </summary>
        public void EndEnemyTurn()
        {
            Debug.Log("=== Enemy Turn End ===");

            // Trigger OnTurnEnd for enemy buffs
            foreach (var enemy in enemies)
            {
                if (enemy.hp.Value > 0)
                {
                    enemy.buffHandler.OnTurnEnd();

                    // Determine next intent
                    enemy.DetermineIntent();
                }
            }

            // Check if battle is over
            if (battleManager.CheckBattleEnd())
            {
                return;
            }

            // Start next player turn
            StartPlayerTurn();
        }
    }

    /// <summary>
    /// Battle turn phases
    /// </summary>
    public enum TurnPhase
    {
        BattleStart,
        PlayerTurn,
        EnemyTurn,
        BattleEnd
    }
}
