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

    private InputAction cancelAction; //TODO set it in input map instead, this is just for testing

    public void Initialize(BattleManager manager, EnemyListUI enemyList)
    {
        battleManager = manager;
        enemyListUI = enemyList;

        cancelAction = new InputAction("TempCancel", binding: "<Keyboard>/escape");
        cancelAction.AddBinding("<Mouse>/rightButton");
        cancelAction.performed += _ => { if (isSelectingTarget) CancelTargetSelection(); };
        cancelAction.Enable();
        Debug.Log("TargetSelector initialized");
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

        // Highlight all living enemies as targetable
        if (enemyListUI != null)
        {
            enemyListUI.SetAllTargetable(true);
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

        // Remove targetable highlights
        if (enemyListUI != null)
        {
            enemyListUI.SetAllTargetable(false);
        }
    }

    //private void Update()
    //{
    //    // Cancel target selection on right click or ESC
    //    if (isSelectingTarget)
    //    {
    //        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
    //        {
    //            CancelTargetSelection();
    //        }
    //    }
    //}

    public void Cleanup()
    {
        EndTargetSelection();
        battleManager = null;
        enemyListUI = null;
        cancelAction.Dispose();
        cancelAction = null;
        Debug.Log("TargetSelector cleaned up");
    }
}
