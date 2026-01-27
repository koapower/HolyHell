using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using HolyHell.Battle.Entity;

/// <summary>
/// Displays entity HP and shield
/// </summary>
public class EntityStatusUI : MonoBehaviour
{
    [Header("HP Display")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Shield Display")]
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private GameObject shieldIcon;

    [Header("Buff Display")]
    [SerializeField] private Transform buffContainer;

    private CompositeDisposable disposables = new CompositeDisposable();

    public void Initialize(BattleEntity entity)
    {
        if (entity == null)
        {
            Debug.LogError("EntityStatusUI: entity is null!");
            return;
        }

        // Subscribe to HP changes
        entity.hp.Subscribe(hp =>
        {
            UpdateHP(hp, entity.maxHp.Value);
        }).AddTo(disposables);

        entity.maxHp.Subscribe(maxHp =>
        {
            UpdateHP(entity.hp.Value, maxHp);
        }).AddTo(disposables);

        // Subscribe to shield changes
        entity.shield.Subscribe(shield =>
        {
            UpdateShield(shield);
        }).AddTo(disposables);

        Debug.Log("EntityStatusUI initialized");
    }

    private void UpdateHP(int hp, int maxHp)
    {
        if (hpText != null)
        {
            hpText.text = $"{hp} / {maxHp}";
        }

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = hp;
        }
    }

    private void UpdateShield(int shield)
    {
        if (shieldText != null)
        {
            shieldText.text = shield > 0 ? shield.ToString() : "";
        }

        if (shieldIcon != null)
        {
            shieldIcon.SetActive(shield > 0);
        }
    }

    /// <summary>
    /// Clean up subscriptions
    /// </summary>
    public void Cleanup()
    {
        disposables.Clear();
        Debug.Log("EntityStatusUI cleaned up");
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }
}
