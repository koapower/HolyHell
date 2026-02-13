using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Lightweight hover trigger attached to a CardUI in the database panel.
/// Notifies DeckBuildUI when the pointer enters so the detail panel can update.
/// </summary>
public class CardDbHoverTrigger : MonoBehaviour, IPointerEnterHandler
{
    private CardRow cardRow;
    private DeckBuildUI deckBuildUI;

    public void Setup(CardRow row, DeckBuildUI controller)
    {
        cardRow = row;
        deckBuildUI = controller;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        deckBuildUI?.OnCardHovered(cardRow);
    }
}
