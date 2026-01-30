using UnityEngine;
using UnityEngine.UI;
using R3;
using HolyHell.Battle;

/// <summary>
/// Button to end player's turn
/// </summary>
public class EndTurnButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private BattleManager battleManager;
    private CompositeDisposable disposables = new CompositeDisposable();

    public void Initialize(BattleManager manager)
    {
        battleManager = manager;

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        // Subscribe to turn phase to enable/disable button
        if (battleManager != null && battleManager.turnSystem != null)
        {
            battleManager.turnSystem.currentPhase.Subscribe(phase =>
            {
                UpdateButtonState(phase);
            }).AddTo(disposables);
        }
    }

    private void OnButtonClicked()
    {
        if (battleManager != null)
        {
            Debug.Log("End Turn button clicked");
            battleManager.EndPlayerTurnManually();
        }
    }

    private void UpdateButtonState(TurnPhase phase)
    {
        if (button == null) return;

        // Only enable during player's turn
        button.interactable = (phase == TurnPhase.PlayerTurn);
    }

    public void Cleanup()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }

        disposables.Clear();
        battleManager = null;
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }

        disposables.Dispose();
    }
}
