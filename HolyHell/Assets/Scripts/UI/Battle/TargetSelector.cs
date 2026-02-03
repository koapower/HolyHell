using HolyHell.Battle;
using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles target selection for cards
/// </summary>
public class TargetSelector : MonoBehaviour
{
    private BattleManager battleManager;
    private EnemyListUI enemyListUI;

    private CardInstance currentCard;
    private bool isSelectingTarget = false;
    public bool IsSelectingTarget => isSelectingTarget;

    public void Initialize(BattleManager manager, EnemyListUI enemyList)
    {
        battleManager = manager;
        enemyListUI = enemyList;
    }

    /// <summary>
    /// Start target selection for a card
    /// </summary>
    public void StartTargetSelection(CardInstance card)
    {
        if (card == null)
        {
            Debug.LogError("TargetSelector: Card is null!");
            return;
        }

        currentCard = card;
        isSelectingTarget = true;

        // Enter selection mode for all enemies
        // Each enemy will decide if it can be targeted
        if (battleManager != null && battleManager.enemies != null)
        {
            foreach (var enemy in battleManager.enemies)
            {
                if (enemy != null)
                {
                    enemy.EnterSelectionMode();
                }
            }
        }

        Debug.Log($"Target selection started for card: {card.DisplayName}");
    }

    /// <summary>
    /// Called when player selects a target
    /// </summary>
    public void OnTargetSelected(EnemyEntity target)
    {
        if (!isSelectingTarget || currentCard == null)
        {
            Debug.LogWarning("TargetSelector: Not currently selecting target!");
            return;
        }

        if (target == null)
        {
            Debug.LogError("TargetSelector: Target is null!");
            CancelTargetSelection();
            return;
        }

        // Check if target is alive
        if (target.hp.Value <= 0)
        {
            Debug.LogWarning("TargetSelector: Cannot target dead enemy!");
            return;
        }

        // Play the card
        bool success = battleManager.PlayCard(currentCard, target);

        if (success)
        {
            Debug.Log($"Card {currentCard.DisplayName} played on {target.enemyData?.DisplayName}");
        }
        else
        {
            Debug.LogWarning($"Failed to play card {currentCard.DisplayName}");
        }

        // End target selection
        EndTargetSelection();

        // Update interaction state
        battleManager.cardAwaitingUse.Value = null;
        battleManager.currentSelectedCard.Value = null;
        battleManager.cardInteractionState.Value = CardInteractionState.Idle;
    }

    /// <summary>
    /// Cancel current target selection
    /// </summary>
    public void CancelTargetSelection()
    {
        Debug.Log("Target selection cancelled");
        EndTargetSelection();
    }

    private void EndTargetSelection()
    {
        isSelectingTarget = false;
        currentCard = null;

        // Exit selection mode for all enemies
        if (battleManager != null && battleManager.enemies != null)
        {
            foreach (var enemy in battleManager.enemies)
            {
                if (enemy != null)
                {
                    enemy.ExitSelectionMode();
                }
            }
        }
    }

    public void Input_Cancel()
    {
        if (isSelectingTarget) CancelTargetSelection();
    }

    public void Cleanup()
    {
        EndTargetSelection();
        battleManager = null;
        enemyListUI = null;
    }
}
