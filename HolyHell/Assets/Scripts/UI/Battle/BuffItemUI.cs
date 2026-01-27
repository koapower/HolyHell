using HolyHell.Battle.Logic.Buffs;
using TMPro;
using UnityEngine;
using R3;
using UnityEngine.UI;

public class BuffItemUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI stackCountText;
    [SerializeField] private TextMeshProUGUI durationText;
    private BuffBase buff;

    public void Initialize(BuffBase buff)
    {
        this.buff = buff;
        UpdateUI();
        // Subscribe to stack count changes
        buff.StackCount.Subscribe(_ => UpdateUI()).AddTo(this);
        buff.Duration.Subscribe(_ => UpdateUI()).AddTo(this);
        //TODO : Set iconImage.sprite based on buff.Id 
    }

    private void UpdateUI()
    {
        if (buff == null) return;
        stackCountText.text = buff.IsStackable ? $"{buff.StackCount.Value}" : string.Empty;
        durationText.text = buff.Duration.Value >= 0 ? $"{buff.Duration.Value}" : string.Empty;
    }
}