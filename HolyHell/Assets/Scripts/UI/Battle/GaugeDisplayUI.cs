using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using HolyHell.Battle.Entity;

/// <summary>
/// Displays Angel and Demon gauges (0-100)
/// </summary>
public class GaugeDisplayUI : MonoBehaviour
{
    [Header("Angel Gauge (Light)")]
    [SerializeField] private Slider angelGaugeSlider;
    [SerializeField] private TextMeshProUGUI angelGaugeText;
    [SerializeField] private Image angelGaugeFill;

    [Header("Demon Gauge (Dark)")]
    [SerializeField] private Slider demonGaugeSlider;
    [SerializeField] private TextMeshProUGUI demonGaugeText;
    [SerializeField] private Image demonGaugeFill;

    [Header("Colors")]
    [SerializeField] private Color angelColor = new Color(1f, 0.9f, 0.5f); // Light gold
    [SerializeField] private Color demonColor = new Color(0.6f, 0.2f, 0.8f); // Dark purple

    private CompositeDisposable disposables = new CompositeDisposable();

    public void Initialize(PlayerEntity player)
    {
        if (player == null)
        {
            Debug.LogError("GaugeDisplayUI: Player is null!");
            return;
        }

        // Setup sliders
        if (angelGaugeSlider != null)
        {
            angelGaugeSlider.minValue = 0;
            angelGaugeSlider.maxValue = 100;
        }

        if (demonGaugeSlider != null)
        {
            demonGaugeSlider.minValue = 0;
            demonGaugeSlider.maxValue = 100;
        }

        // Apply colors
        if (angelGaugeFill != null)
            angelGaugeFill.color = angelColor;

        if (demonGaugeFill != null)
            demonGaugeFill.color = demonColor;

        // Subscribe to angel gauge changes
        player.angelGauge.Subscribe(value =>
        {
            UpdateAngelGauge(value);
        }).AddTo(disposables);

        // Subscribe to demon gauge changes
        player.demonGauge.Subscribe(value =>
        {
            UpdateDemonGauge(value);
        }).AddTo(disposables);
    }

    private void UpdateAngelGauge(int value)
    {
        if (angelGaugeSlider != null)
        {
            angelGaugeSlider.value = value;
        }

        if (angelGaugeText != null)
        {
            angelGaugeText.text = $"Light: {value}";
        }
    }

    private void UpdateDemonGauge(int value)
    {
        if (demonGaugeSlider != null)
        {
            demonGaugeSlider.value = value;
        }

        if (demonGaugeText != null)
        {
            demonGaugeText.text = $"Dark: {value}";
        }
    }

    /// <summary>
    /// Clean up subscriptions
    /// </summary>
    public void Cleanup()
    {
        disposables.Clear();
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }
}
