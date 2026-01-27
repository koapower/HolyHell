using UnityEngine;
using UnityEngine.UI;
using TMPro;
using R3;
using System.Collections.Generic;
using HolyHell.Battle.Entity;

/// <summary>
/// Displays player action points (energy)
/// </summary>
public class ActionPointUI : MonoBehaviour
{
    [Header("Display Mode")]
    [SerializeField] private bool useIconMode = true; // True = show icons, False = show text

    [Header("Icon Mode")]
    [SerializeField] private Transform iconContainer;
    [SerializeField] private GameObject actionPointIconPrefab;
    [SerializeField] private Color activeColor = Color.yellow;
    [SerializeField] private Color inactiveColor = Color.gray;

    [Header("Text Mode")]
    [SerializeField] private TextMeshProUGUI actionPointText;

    private List<Image> actionPointIcons = new List<Image>();
    private CompositeDisposable disposables = new CompositeDisposable();

    public void Initialize(PlayerEntity player)
    {
        if (player == null)
        {
            Debug.LogError("ActionPointUI: Player is null!");
            return;
        }

        // Subscribe to action point changes
        player.actionPoint.Subscribe(current =>
        {
            UpdateDisplay(current, player.maxActionPoint.Value);
        }).AddTo(disposables);

        player.maxActionPoint.Subscribe(max =>
        {
            UpdateDisplay(player.actionPoint.Value, max);
            if (useIconMode)
            {
                RebuildIcons(max);
            }
        }).AddTo(disposables);

        Debug.Log("ActionPointUI initialized");
    }

    private void UpdateDisplay(int current, int max)
    {
        if (useIconMode)
        {
            UpdateIcons(current, max);
        }
        else
        {
            UpdateText(current, max);
        }
    }

    private void RebuildIcons(int maxActionPoints)
    {
        // Clear existing icons
        foreach (var icon in actionPointIcons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        actionPointIcons.Clear();

        // Create new icons
        if (iconContainer != null && actionPointIconPrefab != null)
        {
            for (int i = 0; i < maxActionPoints; i++)
            {
                var iconObj = Instantiate(actionPointIconPrefab, iconContainer);
                var iconImage = iconObj.GetComponent<Image>();
                if (iconImage != null)
                {
                    actionPointIcons.Add(iconImage);
                }
            }
        }
    }

    private void UpdateIcons(int current, int max)
    {
        // Ensure we have correct number of icons
        if (actionPointIcons.Count != max)
        {
            RebuildIcons(max);
        }

        // Update icon colors
        for (int i = 0; i < actionPointIcons.Count; i++)
        {
            if (actionPointIcons[i] != null)
            {
                actionPointIcons[i].color = i < current ? activeColor : inactiveColor;
            }
        }
    }

    private void UpdateText(int current, int max)
    {
        if (actionPointText != null)
        {
            actionPointText.text = $"{current}";
        }
    }

    /// <summary>
    /// Clean up subscriptions and icon instances
    /// </summary>
    public void Cleanup()
    {
        disposables.Clear();

        // Clear icon instances
        foreach (var icon in actionPointIcons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        actionPointIcons.Clear();

        Debug.Log("ActionPointUI cleaned up");
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }
}
