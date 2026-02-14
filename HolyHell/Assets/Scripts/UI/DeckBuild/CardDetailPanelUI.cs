using TMPro;
using UnityEngine;

/// <summary>
/// Displays detailed information about a selected card.
/// The CardUI shown here is in Preview mode (non-interactive).
/// </summary>
public class CardDetailPanelUI : MonoBehaviour
{
    [Header("Card Visual")]
    [SerializeField] private CardSlotUI cardSlot;

    [Header("Info Texts")]
    [SerializeField] private TextMeshProUGUI cardIdText;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI factionText;
    [SerializeField] private TextMeshProUGUI godhoodText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI angelValueText;
    [SerializeField] private TextMeshProUGUI demonValueText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    /// <summary>
    /// Populates all text fields and the card visual from a CardRow.
    /// </summary>
    public void ShowCard(CardRow card)
    {
        if (card == null) return;

        if (cardIdText    != null) cardIdText.text    = card.Id;
        if (cardNameText  != null) cardNameText.text  = card.DisplayName;
        if (typeText      != null) typeText.text      = card.ElementType.ToString();
        if (factionText   != null) factionText.text   = card.Faction.ToString();
        if (godhoodText   != null) godhoodText.text   = card.GodType.ToString();
        if (rarityText    != null) rarityText.text    = card.Rarity.ToString();
        if (costText      != null) costText.text      = card.ActionCost.ToString();
        if (angelValueText != null) angelValueText.text = card.AngelGaugeIncrease.ToString();
        if (demonValueText != null) demonValueText.text = card.DemonGaugeIncrease.ToString();
        if (descriptionText != null) descriptionText.text = card.Description;

        // Update the card visual in Preview mode (no interaction)
        if (cardSlot != null && cardSlot.cardUI != null)
        {
            cardSlot.cardUI.InitializeDeckBuild(card, clickCallback: null);
            cardSlot.cardUI.currentMode = CardUIMode.Preview;
        }
    }
}
