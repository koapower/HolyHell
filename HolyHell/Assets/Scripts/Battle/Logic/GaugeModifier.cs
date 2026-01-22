using UnityEngine;

/// <summary>
/// Handles Angel/Demon gauge modifications with 0-100 clamping
/// </summary>
public class GaugeModifier
{
    private PlayerEntity player;

    public GaugeModifier(PlayerEntity owner)
    {
        player = owner;
    }

    /// <summary>
    /// Modify angel gauge (clamped to 0-100)
    /// </summary>
    public void ModifyAngelGauge(int delta)
    {
        int newValue = player.angelGauge.Value + delta;
        player.angelGauge.Value = Mathf.Clamp(newValue, 0, 100);
    }

    /// <summary>
    /// Modify demon gauge (clamped to 0-100)
    /// </summary>
    public void ModifyDemonGauge(int delta)
    {
        int newValue = player.demonGauge.Value + delta;
        player.demonGauge.Value = Mathf.Clamp(newValue, 0, 100);
    }

    /// <summary>
    /// Modify both gauges at once (for card effects)
    /// </summary>
    public void ModifyGauges(int angelDelta, int demonDelta)
    {
        ModifyAngelGauge(angelDelta);
        ModifyDemonGauge(demonDelta);
    }

    /// <summary>
    /// Set angel gauge to specific value (clamped)
    /// </summary>
    public void SetAngelGauge(int value)
    {
        player.angelGauge.Value = Mathf.Clamp(value, 0, 100);
    }

    /// <summary>
    /// Set demon gauge to specific value (clamped)
    /// </summary>
    public void SetDemonGauge(int value)
    {
        player.demonGauge.Value = Mathf.Clamp(value, 0, 100);
    }
}
