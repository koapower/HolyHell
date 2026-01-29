using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using HolyHell.Battle.Card;
using HolyHell.Battle;
using R3;

/// <summary>
/// Displays a single card in hand
/// Handles click and hover events
/// </summary>
public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Card Info")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image elementImage;
    [SerializeField] private Color angelColor = new Color(1f, 0.9f, 0.5f); // Light gold
    [SerializeField] private Color demonColor = new Color(0.6f, 0.2f, 0.8f); // Dark purple]
    [SerializeField] private Color blissColor = Color.purple;
    [SerializeField] private Color dominationColor = Color.red;
    [SerializeField] private Color enlightendColor = Color.yellow;
    [SerializeField] private Color despairColor = Color.cyan;
    [SerializeField] private Color ravenousColor = Color.green;

    [Header("Gauge Display")]
    [SerializeField] private Image angelBg;
    [SerializeField] private Image demonBg;
    [SerializeField] private TextMeshProUGUI angelGaugeText;
    [SerializeField] private TextMeshProUGUI demonGaugeText;

    [Header("Visual")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.8f);
    [SerializeField] private Color unplayableColor = Color.gray;

    [Header("Animation")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverTransitionSpeed = 10f;
    [SerializeField] private Vector2 selectedPosition = new Vector2(0, 50);
    [SerializeField] private float selectedTransitionSpeed = 10f;

    private BattleManager battleManager;
    private CardInstance card;
    private Action<CardInstance> onClickCallback;
    private bool isPlayable = true;
    private bool isHovered = false;
    private bool isSelected = false;
    private Vector3 originalScale;
    private Vector2 originalAnchoredPosition;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalAnchoredPosition = transform.GetComponent<RectTransform>().anchoredPosition;
    }

    public void Initialize(BattleManager battleManager, CardInstance cardInstance, Action<CardInstance> onClick)
    {
        this.battleManager = battleManager;
        card = cardInstance;
        onClickCallback = onClick;
        battleManager.currentSelectedCard.Subscribe(card =>
        {
            isSelected = card != null && card.instanceId == this.card.instanceId;
        }).AddTo(this);

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
            cardNameText.color = card.cardData.Faction switch
            {
                Faction.Angel => angelColor,
                Faction.Demon => demonColor,
                _ => Color.white
            };
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

        // Element
        if (elementImage != null)
        {
            elementImage.color = card.cardData.ElementType switch
            {
                ElementType.Bliss => blissColor,
                ElementType.Domination => dominationColor,
                ElementType.Enlightened => enlightendColor,
                ElementType.Despair => despairColor,
                ElementType.Ravenous => ravenousColor,
                _ => Color.white
            };
        }

        // Gauge changes
        if (angelGaugeText != null)
        {
            if (card.AngelGaugeIncrease != 0)
            {
                angelGaugeText.text = card.AngelGaugeIncrease > 0
                    ? $"+{card.AngelGaugeIncrease}"
                    : $"{card.AngelGaugeIncrease}";
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
                    ? $"+{card.DemonGaugeIncrease}"
                    : $"{card.DemonGaugeIncrease}";
            }
            else
            {
                demonGaugeText.text = "";
            }
        }

        if (angelBg != null)
        {
            angelBg.gameObject.SetActive(card.AngelGaugeIncrease != 0);
        }

        if (demonBg != null)
        {
            demonBg.gameObject.SetActive(card.DemonGaugeIncrease != 0);
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

    public void SetCardInteractability(bool canInteract)
    {
        // default is true
        canvasGroup.blocksRaycasts = canInteract;
        canvasGroup.interactable = canInteract;
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
        Vector2 targetPosition = isSelected ? selectedPosition : originalAnchoredPosition;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * hoverTransitionSpeed);
        var rectT = transform.GetComponent<RectTransform>();
        rectT.anchoredPosition = Vector2.Lerp(rectT.anchoredPosition, targetPosition, Time.deltaTime * selectedTransitionSpeed);
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

    // IPointerUpHandler
    public void OnPointerUp(PointerEventData eventData)
    {

    }

    // IPointerDownHandler
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"ponterdown {card.DisplayName}");
        battleManager.currentPreviewCard.Value = card;
    }
}
