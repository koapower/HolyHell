using UnityEngine;
using TMPro;
using HolyHell.Battle.Entity;

/// <summary>
/// Displays draw pile and discard pile counts
/// </summary>
public class DeckCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI drawPileText;
    [SerializeField] private TextMeshProUGUI discardPileText;
    [SerializeField] private TextMeshProUGUI handCountText;

    private PlayerEntity player;

    public void Initialize(PlayerEntity playerEntity)
    {
        player = playerEntity;

        if (player == null)
        {
            Debug.LogError("DeckCounterUI: Player is null!");
            return;
        }

        Debug.Log("DeckCounterUI initialized");
    }

    private void Update()
    {
        if (player == null) return;

        UpdateCounts();
    }

    private void UpdateCounts()
    {
        // Draw pile count
        if (drawPileText != null)
        {
            int drawCount = player.drawPile?.Count ?? 0;
            drawPileText.text = $"{drawCount}";
        }

        // Discard pile count
        if (discardPileText != null)
        {
            int discardCount = player.discardPile?.Count ?? 0;
            discardPileText.text = $"Discard: {discardCount}";
        }

        // Hand count (optional)
        if (handCountText != null)
        {
            int handCount = player.hand?.Count ?? 0;
            handCountText.text = $"Hand: {handCount}";
        }
    }

    public void Cleanup()
    {
        player = null;
        Debug.Log("DeckCounterUI cleaned up");
    }
}
