using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using HolyHell.Battle.Card;

/// <summary>
/// Displays a single card in hand
/// Handles click and hover events
/// </summary>
public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Card Info")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Gauge Display")]
    [SerializeField] private TextMeshProUGUI angelGaugeText;
    [SerializeField] private TextMeshProUGUI demonGaugeText;

    [Header("Visual")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.8f);
    [SerializeField] private Color unplayableColor = Color.gray;

    [Header("Hover Effect")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverTransitionSpeed = 10f;

    private CardInstance card;
    private Action<CardInstance> onClickCallback;
    private bool isPlayable = true;
    private bool isHovered = false;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void Initialize(CardInstance cardInstance, Action<CardInstance> onClick)
    {
        card = cardInstance;
        onClickCallback = onClick;

        if (card == null || card.cardData == null)
        {
            Debug.LogError("CardUI: Card or cardData is null!");
            return;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Card name
        if (cardNameText != null)
        {
            cardNameText.text = card.DisplayName;
        }

        // Cost
        if (costText != null)
        {
            costText.text = card.ActionCost.ToString();
        }

        // Description
        if (descriptionText != null)
        {
            descriptionText.text = card.cardData.Description;
        }

        // Gauge changes
        if (angelGaugeText != null)
        {
            if (card.AngelGaugeIncrease != 0)
            {
                angelGaugeText.text = card.AngelGaugeIncrease > 0
                    ? $"+{card.AngelGaugeIncrease} Light"
                    : $"{card.AngelGaugeIncrease} Light";
            }
            else
            {
                angelGaugeText.text = "";
            }
        }

        if (demonGaugeText != null)
        {
            if (card.DemonGaugeIncrease != 0)
            {
                demonGaugeText.text = card.DemonGaugeIncrease > 0
                    ? $"+{card.DemonGaugeIncrease} Dark"
                    : $"{card.DemonGaugeIncrease} Dark";
            }
            else
            {
                demonGaugeText.text = "";
            }
        }

        // Set card frame color based on faction
        if (cardFrame != null)
        {
            cardFrame.color = GetFactionColor(card.cardData.Faction);
        }
    }

    private Color GetFactionColor(Faction faction)
    {
        return faction switch
        {
            Faction.Angel => new Color(1f, 0.9f, 0.5f), // Light gold
            Faction.Demon => new Color(0.6f, 0.2f, 0.8f), // Dark purple
            Faction.Human => new Color(0.8f, 0.8f, 0.8f), // Gray
            _ => Color.white // Neutral
        };
    }

    /// <summary>
    /// Set whether this card can be played
    /// </summary>
    public void SetPlayable(bool playable)
    {
        isPlayable = playable;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (backgroundImage == null) return;

        if (!isPlayable)
        {
            backgroundImage.color = unplayableColor;
        }
        else if (isHovered)
        {
            backgroundImage.color = hoverColor;
        }
        else
        {
            backgroundImage.color = normalColor;
        }
    }

    private void Update()
    {
        // Smooth hover scale animation
        Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * hoverTransitionSpeed);
    }

    // IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isPlayable)
        {
            Debug.Log($"Card {card.DisplayName} is not playable!");
            return;
        }

        onClickCallback?.Invoke(card);
    }

    // IPointerEnterHandler
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisual();
    }

    // IPointerExitHandler
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisual();
    }
}
