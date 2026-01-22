using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;

/// <summary>
/// Displays player HP and shield
/// </summary>
public class PlayerStatusUI : MonoBehaviour
{
    [Header("HP Display")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Shield Display")]
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private GameObject shieldIcon;

    private CompositeDisposable disposables = new CompositeDisposable();

    public void Initialize(PlayerEntity player)
    {
        if (player == null)
        {
            Debug.LogError("PlayerStatusUI: Player is null!");
            return;
        }

        // Subscribe to HP changes
        player.hp.Subscribe(hp =>
        {
            UpdateHP(hp, player.maxHp.Value);
        }).AddTo(disposables);

        player.maxHp.Subscribe(maxHp =>
        {
            UpdateHP(player.hp.Value, maxHp);
        }).AddTo(disposables);

        // Subscribe to shield changes
        player.shield.Subscribe(shield =>
        {
            UpdateShield(shield);
        }).AddTo(disposables);

        Debug.Log("PlayerStatusUI initialized");
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

    private void OnDestroy()
    {
        disposables.Dispose();
    }
}
