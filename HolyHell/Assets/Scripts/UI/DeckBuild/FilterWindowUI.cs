using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup window for filtering the card database.
/// Opens with the current filter state, lets the user toggle individual values,
/// and calls back with the new filter when OK is pressed.
/// </summary>
public class FilterWindowUI : MonoBehaviour
{
    [Header("Control Buttons")]
    [SerializeField] private Button okButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button clearFilterButton;

    [Header("ElementType Toggles")]
    [SerializeField] private Toggle toggleElementDespair;
    [SerializeField] private Toggle toggleElementEnlightened;
    [SerializeField] private Toggle toggleElementBliss;
    [SerializeField] private Toggle toggleElementRavenous;
    [SerializeField] private Toggle toggleElementDomination;

    [Header("Faction Toggles")]
    [SerializeField] private Toggle toggleFactionAngel;
    [SerializeField] private Toggle toggleFactionDemon;
    [SerializeField] private Toggle toggleFactionNeutral;

    [Header("GodType Toggles")]
    [SerializeField] private Toggle toggleGodIntereon;
    [SerializeField] private Toggle toggleGodFlorentis;
    [SerializeField] private Toggle toggleGodEvinor;
    [SerializeField] private Toggle toggleGodOculyth;
    [SerializeField] private Toggle toggleGodHedalis;
    [SerializeField] private Toggle toggleGodVortherect;

    [Header("Rarity Toggles (one per rarity tier)")]
    [SerializeField] private Toggle toggleRarity1;
    [SerializeField] private Toggle toggleRarity2;
    [SerializeField] private Toggle toggleRarity3;
    [SerializeField] private Toggle toggleRarity4;

    // Working copy modified while the window is open; only written to the real filter on OK
    private DeckBuildFilter workingFilter;
    private Action<DeckBuildFilter> onConfirm;

    private void Start()
    {
        okButton?.onClick.AddListener(OnOkClicked);
        cancelButton?.onClick.AddListener(OnCancelClicked);
        clearFilterButton?.onClick.AddListener(OnClearClicked);
    }

    /// <summary>
    /// Opens the filter window.
    /// </summary>
    /// <param name="currentFilter">The currently active filter to pre-populate toggles.</param>
    /// <param name="confirmCallback">Called with the resulting filter when OK is pressed.</param>
    public void Open(DeckBuildFilter currentFilter, Action<DeckBuildFilter> confirmCallback)
    {
        workingFilter = currentFilter.Clone();
        onConfirm = confirmCallback;
        PopulateToggles();
        gameObject.SetActive(true);
    }

    private void PopulateToggles()
    {
        // ElementType
        SetToggle(toggleElementDespair,    workingFilter.elementTypes.Contains(ElementType.Despair));
        SetToggle(toggleElementEnlightened, workingFilter.elementTypes.Contains(ElementType.Enlightened));
        SetToggle(toggleElementBliss,      workingFilter.elementTypes.Contains(ElementType.Bliss));
        SetToggle(toggleElementRavenous,   workingFilter.elementTypes.Contains(ElementType.Ravenous));
        SetToggle(toggleElementDomination, workingFilter.elementTypes.Contains(ElementType.Domination));

        // Faction
        SetToggle(toggleFactionAngel,   workingFilter.factions.Contains(Faction.Angel));
        SetToggle(toggleFactionDemon,   workingFilter.factions.Contains(Faction.Demon));
        SetToggle(toggleFactionNeutral, workingFilter.factions.Contains(Faction.Neutral));

        // GodType
        SetToggle(toggleGodIntereon,   workingFilter.godTypes.Contains(GodType.Intereon));
        SetToggle(toggleGodFlorentis,  workingFilter.godTypes.Contains(GodType.Florentis));
        SetToggle(toggleGodEvinor,     workingFilter.godTypes.Contains(GodType.Evinor));
        SetToggle(toggleGodOculyth,    workingFilter.godTypes.Contains(GodType.Oculyth));
        SetToggle(toggleGodHedalis,    workingFilter.godTypes.Contains(GodType.Hedalis));
        SetToggle(toggleGodVortherect, workingFilter.godTypes.Contains(GodType.Vortherect));

        // Rarity (assumes rarity tiers 1–4)
        SetToggle(toggleRarity1, workingFilter.rarities.Contains(1));
        SetToggle(toggleRarity2, workingFilter.rarities.Contains(2));
        SetToggle(toggleRarity3, workingFilter.rarities.Contains(3));
        SetToggle(toggleRarity4, workingFilter.rarities.Contains(4));
    }

    private void OnOkClicked()
    {
        // Read all toggle states into the working filter
        ReadToggles();
        onConfirm?.Invoke(workingFilter);
        gameObject.SetActive(false);
    }

    private void OnCancelClicked()
    {
        gameObject.SetActive(false);
    }

    private void OnClearClicked()
    {
        // Reset all toggles in the window (does not apply until OK)
        workingFilter.Clear();
        PopulateToggles();
    }

    private void ReadToggles()
    {
        workingFilter.elementTypes.Clear();
        workingFilter.factions.Clear();
        workingFilter.godTypes.Clear();
        workingFilter.rarities.Clear();

        // ElementType
        if (IsOn(toggleElementDespair))    workingFilter.elementTypes.Add(ElementType.Despair);
        if (IsOn(toggleElementEnlightened)) workingFilter.elementTypes.Add(ElementType.Enlightened);
        if (IsOn(toggleElementBliss))      workingFilter.elementTypes.Add(ElementType.Bliss);
        if (IsOn(toggleElementRavenous))   workingFilter.elementTypes.Add(ElementType.Ravenous);
        if (IsOn(toggleElementDomination)) workingFilter.elementTypes.Add(ElementType.Domination);

        // Faction
        if (IsOn(toggleFactionAngel))   workingFilter.factions.Add(Faction.Angel);
        if (IsOn(toggleFactionDemon))   workingFilter.factions.Add(Faction.Demon);
        if (IsOn(toggleFactionNeutral)) workingFilter.factions.Add(Faction.Neutral);

        // GodType
        if (IsOn(toggleGodIntereon))   workingFilter.godTypes.Add(GodType.Intereon);
        if (IsOn(toggleGodFlorentis))  workingFilter.godTypes.Add(GodType.Florentis);
        if (IsOn(toggleGodEvinor))     workingFilter.godTypes.Add(GodType.Evinor);
        if (IsOn(toggleGodOculyth))    workingFilter.godTypes.Add(GodType.Oculyth);
        if (IsOn(toggleGodHedalis))    workingFilter.godTypes.Add(GodType.Hedalis);
        if (IsOn(toggleGodVortherect)) workingFilter.godTypes.Add(GodType.Vortherect);

        // Rarity
        if (IsOn(toggleRarity1)) workingFilter.rarities.Add(1);
        if (IsOn(toggleRarity2)) workingFilter.rarities.Add(2);
        if (IsOn(toggleRarity3)) workingFilter.rarities.Add(3);
        if (IsOn(toggleRarity4)) workingFilter.rarities.Add(4);
    }

    private static void SetToggle(Toggle t, bool value)
    {
        if (t != null) t.isOn = value;
    }

    private static bool IsOn(Toggle t) => t != null && t.isOn;
}
