using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup window for choosing the card database sort order.
/// Sort field and direction are applied live whenever a toggle changes.
/// The Cancel button closes the window without reverting the sort.
/// </summary>
public class SortWindowUI : MonoBehaviour
{
    [Header("Sort Field Toggle Group")]
    [SerializeField] private ToggleGroup sortFieldGroup;
    [SerializeField] private Toggle toggleSortId;
    [SerializeField] private Toggle toggleSortName;
    [SerializeField] private Toggle toggleSortAngelValue;
    [SerializeField] private Toggle toggleSortDemonValue;
    [SerializeField] private Toggle toggleSortRarity;
    [SerializeField] private Toggle toggleSortCost;

    [Header("Direction Toggle Group")]
    [SerializeField] private ToggleGroup directionGroup;
    [SerializeField] private Toggle toggleAscending;   // Low to high
    [SerializeField] private Toggle toggleDescending;  // High to low

    [Header("Buttons")]
    [SerializeField] private Button cancelButton;

    private Action<SortField, bool> onSortChanged;

    private void Start()
    {
        cancelButton?.onClick.AddListener(OnCancelClicked);

        // Sort field toggles
        AddFieldListener(toggleSortId,         SortField.Id);
        AddFieldListener(toggleSortName,        SortField.Name);
        AddFieldListener(toggleSortAngelValue,  SortField.AngelValue);
        AddFieldListener(toggleSortDemonValue,  SortField.DemonValue);
        AddFieldListener(toggleSortRarity,      SortField.Rarity);
        AddFieldListener(toggleSortCost,        SortField.Cost);

        // Direction toggles
        if (toggleAscending  != null) toggleAscending.onValueChanged.AddListener(_  => NotifySortChanged());
        if (toggleDescending != null) toggleDescending.onValueChanged.AddListener(_ => NotifySortChanged());
    }

    /// <summary>
    /// Opens the sort window and syncs toggles to the current sort state.
    /// </summary>
    /// <param name="currentField">Currently active sort field.</param>
    /// <param name="ascending">True = ascending (low to high).</param>
    /// <param name="sortChangedCallback">Called whenever the user changes any toggle.</param>
    public void Open(SortField currentField, bool ascending, Action<SortField, bool> sortChangedCallback)
    {
        onSortChanged = sortChangedCallback;
        SyncToggles(currentField, ascending);
        gameObject.SetActive(true);
    }

    private void SyncToggles(SortField field, bool ascending)
    {
        // Disable listeners temporarily to avoid triggering callbacks while syncing
        SetToggleWithoutNotify(toggleSortId,        field == SortField.Id);
        SetToggleWithoutNotify(toggleSortName,       field == SortField.Name);
        SetToggleWithoutNotify(toggleSortAngelValue, field == SortField.AngelValue);
        SetToggleWithoutNotify(toggleSortDemonValue, field == SortField.DemonValue);
        SetToggleWithoutNotify(toggleSortRarity,     field == SortField.Rarity);
        SetToggleWithoutNotify(toggleSortCost,       field == SortField.Cost);

        SetToggleWithoutNotify(toggleAscending,  ascending);
        SetToggleWithoutNotify(toggleDescending, !ascending);
    }

    private void OnCancelClicked()
    {
        gameObject.SetActive(false);
    }

    private void NotifySortChanged()
    {
        onSortChanged?.Invoke(GetSelectedField(), IsAscending());
    }

    private SortField GetSelectedField()
    {
        if (IsOn(toggleSortName))       return SortField.Name;
        if (IsOn(toggleSortAngelValue)) return SortField.AngelValue;
        if (IsOn(toggleSortDemonValue)) return SortField.DemonValue;
        if (IsOn(toggleSortRarity))     return SortField.Rarity;
        if (IsOn(toggleSortCost))       return SortField.Cost;
        return SortField.Id; // Default
    }

    private bool IsAscending()
    {
        // Ascending toggle being on (or descending being off) means ascending
        if (toggleAscending != null) return toggleAscending.isOn;
        return true;
    }

    private void AddFieldListener(Toggle toggle, SortField field)
    {
        if (toggle == null) return;
        toggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn) NotifySortChanged();
        });
    }

    private static void SetToggleWithoutNotify(Toggle t, bool value)
    {
        if (t != null) t.SetIsOnWithoutNotify(value);
    }

    private static bool IsOn(Toggle t) => t != null && t.isOn;
}
