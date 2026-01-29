using HolyHell.Battle;
using R3;
using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

/// <summary>
/// Manages the "Cancel" button that appears during target selection
/// </summary>
public class CancelUseCardButtonUI : MonoBehaviour
{
    [SerializeField] private Button cancelButton;

    private BattleManager battleManager;
    private CompositeDisposable disposables = new CompositeDisposable();

    public Action onCancelButtonClicked;

    public void Initialize(BattleManager battleManager)
    {
        this.battleManager = battleManager;

        // Subscribe to cardInteractionState to show/hide cancel button
        battleManager.cardInteractionState.Subscribe(state =>
        {
            if (state == CardInteractionState.SelectingTarget)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }).AddTo(disposables);

        // Button click handler
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        // Initially hidden
        Hide();
    }

    private void OnCancelButtonClick()
    {
        onCancelButtonClicked?.Invoke();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        disposables.Dispose();

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(OnCancelButtonClick);
        }
    }
}
