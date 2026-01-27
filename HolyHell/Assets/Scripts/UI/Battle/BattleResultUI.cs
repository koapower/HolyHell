using UnityEngine;
using TMPro;
using UnityEngine.UI;
using R3;
using HolyHell.Battle;

/// <summary>
/// Displays victory or defeat screen
/// </summary>
public class BattleResultUI : MonoBehaviour
{
    [Header("Result Panel")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Victory")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Color victoryColor = Color.green;

    [Header("Defeat")]
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private Color defeatColor = Color.red;

    [Header("Buttons")]
    [SerializeField] private Button continueButton;

    private CompositeDisposable disposables = new CompositeDisposable();

    public void Initialize(BattleManager battleManager)
    {
        if (battleManager == null)
        {
            Debug.LogError("BattleResultUI: BattleManager is null!");
            return;
        }

        // Hide result panel initially
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }

        // Subscribe to battle state
        battleManager.battleState.Subscribe(state =>
        {
            if (state == BattleState.Ended)
            {
                ShowResult(battleManager);
            }
        }).AddTo(disposables);

        // Setup continue button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        Debug.Log("BattleResultUI initialized");
    }

    private void ShowResult(BattleManager battleManager)
    {
        // Check if player won (all enemies dead)
        bool playerWon = true;
        foreach (var enemy in battleManager.enemies)
        {
            if (enemy.hp.Value > 0)
            {
                playerWon = false;
                break;
            }
        }

        // Show appropriate result
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (playerWon)
        {
            ShowVictory();
        }
        else
        {
            ShowDefeat();
        }
    }

    private void ShowVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (defeatPanel != null)
        {
            defeatPanel.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = "VICTORY!";
            resultText.color = victoryColor;
        }

        Debug.Log("Battle result: VICTORY");
    }

    private void ShowDefeat()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (resultText != null)
        {
            resultText.text = "DEFEAT";
            resultText.color = defeatColor;
        }

        Debug.Log("Battle result: DEFEAT");
    }

    private void OnContinueClicked()
    {
        Debug.Log("Continue button clicked");
        // TODO: Load next scene or return to menu
        // For now, just hide the panel
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    public void Cleanup()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        disposables.Clear();
        Debug.Log("BattleResultUI cleaned up");
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        disposables.Dispose();
    }
}
