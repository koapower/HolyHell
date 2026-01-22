using UnityEngine;
using TMPro;
using R3;

/// <summary>
/// Displays current turn phase and turn number
/// </summary>
public class TurnIndicatorUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnPhaseText;
    [SerializeField] private TextMeshProUGUI turnNumberText;

    private CompositeDisposable disposables = new CompositeDisposable();

    public void Initialize(TurnSystem turnSystem)
    {
        if (turnSystem == null)
        {
            Debug.LogError("TurnIndicatorUI: TurnSystem is null!");
            return;
        }

        // Subscribe to turn phase changes
        turnSystem.currentPhase.Subscribe(phase =>
        {
            UpdatePhase(phase);
        }).AddTo(disposables);

        // Subscribe to turn number changes
        turnSystem.turnNumber.Subscribe(turnNum =>
        {
            UpdateTurnNumber(turnNum);
        }).AddTo(disposables);

        Debug.Log("TurnIndicatorUI initialized");
    }

    private void UpdatePhase(TurnPhase phase)
    {
        if (turnPhaseText == null) return;

        turnPhaseText.text = phase switch
        {
            TurnPhase.BattleStart => "Battle Start",
            TurnPhase.PlayerTurn => "Your Turn",
            TurnPhase.EnemyTurn => "Enemy Turn",
            TurnPhase.BattleEnd => "Battle End",
            _ => "Unknown"
        };
    }

    private void UpdateTurnNumber(int turnNumber)
    {
        if (turnNumberText != null)
        {
            turnNumberText.text = $"Turn {turnNumber}";
        }
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }
}
