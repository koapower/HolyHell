using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HolyHell.Battle.Card
{
    /// <summary>
    /// Manages card deck, hand, discard pile, and destroyed pile during battle
    /// </summary>
    public class CardDeckManager
    {
        // Card piles
        private List<CardInstance> deck;
        private List<CardInstance> hand;
        private List<CardInstance> discardPile;
        private List<CardInstance> destroyedPile; // Cards permanently removed from battle

        // Properties
        public IReadOnlyList<CardInstance> Deck => deck.AsReadOnly();
        public IReadOnlyList<CardInstance> Hand => hand.AsReadOnly();
        public IReadOnlyList<CardInstance> DiscardPile => discardPile.AsReadOnly();
        public IReadOnlyList<CardInstance> DestroyedPile => destroyedPile.AsReadOnly();

        public int HandSize => hand.Count;
        public int DeckSize => deck.Count;
        public int DiscardSize => discardPile.Count;
        public int DestroyedSize => destroyedPile.Count;

        // Events
        public event Action<CardInstance> OnCardDrawn;
        public event Action<CardInstance> OnCardDiscarded;
        public event Action<CardInstance> OnCardDestroyed;
        public event Action OnDeckShuffled;

        // Constructor
        public CardDeckManager(List<CardInstance> initialDeck)
        {
            deck = new List<CardInstance>(initialDeck);
            hand = new List<CardInstance>();
            discardPile = new List<CardInstance>();
            destroyedPile = new List<CardInstance>();
        }

        /// <summary>
        /// Draw specified number of cards from deck to hand
        /// If deck is empty, shuffle discard pile back into deck
        /// </summary>
        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                // If deck is empty, shuffle discard pile back
                if (deck.Count == 0)
                {
                    if (discardPile.Count == 0)
                    {
                        Debug.LogWarning("Cannot draw card: both deck and discard pile are empty");
                        break;
                    }

                    ShuffleDiscardIntoDeck();
                }

                // Draw top card
                CardInstance drawnCard = deck[0];
                deck.RemoveAt(0);
                hand.Add(drawnCard);

                OnCardDrawn?.Invoke(drawnCard);
            }
        }

        /// <summary>
        /// Add card to hand (for Gifted buff or other effects)
        /// </summary>
        public void AddCardToHand(CardInstance card)
        {
            if (card == null)
            {
                Debug.LogWarning("Cannot add null card to hand");
                return;
            }

            hand.Add(card);
            OnCardDrawn?.Invoke(card);
        }

        /// <summary>
        /// Remove card from hand and add to discard pile
        /// </summary>
        public void DiscardCard(CardInstance card)
        {
            if (hand.Remove(card))
            {
                discardPile.Add(card);
                OnCardDiscarded?.Invoke(card);
            }
            else
            {
                Debug.LogWarning($"Attempted to discard card {card.Id} that is not in hand");
            }
        }

        /// <summary>
        /// Discard all cards in hand
        /// </summary>
        public void DiscardHand()
        {
            foreach (var card in hand.ToList())
            {
                DiscardCard(card);
            }
        }

        /// <summary>
        /// Destroy random cards from deck (permanent removal)
        /// </summary>
        public void DestroyRandomCardsFromDeck(int count)
        {
            int actualCount = Mathf.Min(count, deck.Count);

            for (int i = 0; i < actualCount; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, deck.Count);
                CardInstance destroyedCard = deck[randomIndex];
                deck.RemoveAt(randomIndex);
                destroyedPile.Add(destroyedCard);

                OnCardDestroyed?.Invoke(destroyedCard);
            }
        }

        /// <summary>
        /// Destroy random cards from hand (permanent removal)
        /// </summary>
        public void DestroyRandomCardsFromHand(int count)
        {
            int actualCount = Mathf.Min(count, hand.Count);

            for (int i = 0; i < actualCount; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, hand.Count);
                CardInstance destroyedCard = hand[randomIndex];
                hand.RemoveAt(randomIndex);
                destroyedPile.Add(destroyedCard);

                OnCardDestroyed?.Invoke(destroyedCard);
            }
        }

        /// <summary>
        /// Destroy specific card from hand (permanent removal)
        /// </summary>
        public bool DestroyCardFromHand(CardInstance card)
        {
            if (hand.Remove(card))
            {
                destroyedPile.Add(card);
                OnCardDestroyed?.Invoke(card);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Shuffle discard pile back into deck
        /// </summary>
        private void ShuffleDiscardIntoDeck()
        {
            deck.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
            OnDeckShuffled?.Invoke();
        }

        /// <summary>
        /// Shuffle the current deck
        /// </summary>
        public void ShuffleDeck()
        {
            // Fisher-Yates shuffle
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                CardInstance temp = deck[i];
                deck[i] = deck[randomIndex];
                deck[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Get card at specific position in hand
        /// </summary>
        public CardInstance GetCardInHand(int index)
        {
            if (index < 0 || index >= hand.Count)
                return null;

            return hand[index];
        }

        /// <summary>
        /// Get index of card in hand (-1 if not found)
        /// </summary>
        public int GetCardIndexInHand(CardInstance card)
        {
            return hand.IndexOf(card);
        }

        /// <summary>
        /// Check if hand contains specific card
        /// </summary>
        public bool HandContains(CardInstance card)
        {
            return hand.Contains(card);
        }

        /// <summary>
        /// Clear destroyed pile (called at end of battle)
        /// </summary>
        public void ClearDestroyedPile()
        {
            destroyedPile.Clear();
        }

        /// <summary>
        /// Reset all piles for a new battle
        /// </summary>
        public void ResetForNewBattle(List<CardInstance> newDeck)
        {
            deck = new List<CardInstance>(newDeck);
            hand.Clear();
            discardPile.Clear();
            destroyedPile.Clear();
            ShuffleDeck();
        }
    }
}
