using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

/// <summary>
/// Manages deck, hand, and discard pile for a player
/// </summary>
public class DeckManager
{
    private PlayerEntity player;
    private ITableManager tableManager;

    public DeckManager(PlayerEntity owner)
    {
        player = owner;
    }

    /// <summary>
    /// Initialize deck from card IDs
    /// </summary>
    public async UniTask InitializeDeck(List<string> cardIds)
    {
        // Get table manager
        tableManager = await ServiceLocator.Instance.GetAsync<ITableManager>();
        var cardTable = tableManager.GetTable<CardRow>();

        // Clear all piles
        player.drawPile.Clear();
        player.hand.Clear();
        player.discardPile.Clear();

        // Create card instances
        foreach (var cardId in cardIds)
        {
            var cardData = cardTable.GetRow(c => c.Id == cardId);
            if (cardData != null)
            {
                player.drawPile.Add(new CardInstance(cardData));
            }
        }

        // Shuffle draw pile
        ShuffleDrawPile();
    }

    /// <summary>
    /// Draw cards from draw pile to hand
    /// </summary>
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            DrawCard();
        }
    }

    /// <summary>
    /// Draw a single card
    /// </summary>
    public CardInstance DrawCard()
    {
        // If draw pile is empty, shuffle discard pile into draw pile
        if (player.drawPile.Count == 0)
        {
            if (player.discardPile.Count == 0)
            {
                return null; // No cards left
            }

            // Move all discard to draw pile
            player.drawPile.AddRange(player.discardPile);
            player.discardPile.Clear();
            ShuffleDrawPile();
        }

        // Draw top card
        var card = player.drawPile[0];
        player.drawPile.RemoveAt(0);
        player.hand.Add(card);

        return card;
    }

    /// <summary>
    /// Discard a card from hand
    /// </summary>
    public void DiscardCard(CardInstance card)
    {
        if (player.hand.Remove(card))
        {
            player.discardPile.Add(card);
        }
    }

    /// <summary>
    /// Discard entire hand
    /// </summary>
    public void DiscardHand()
    {
        player.discardPile.AddRange(player.hand);
        player.hand.Clear();
    }

    /// <summary>
    /// Shuffle draw pile
    /// </summary>
    public void ShuffleDrawPile()
    {
        var rng = new System.Random();
        int n = player.drawPile.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var temp = player.drawPile[k];
            player.drawPile[k] = player.drawPile[n];
            player.drawPile[n] = temp;
        }
    }

    /// <summary>
    /// Move card from hand to discard (after playing)
    /// </summary>
    public void ExhaustCard(CardInstance card)
    {
        DiscardCard(card);
    }
}
