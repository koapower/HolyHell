using HolyHell.Battle.Effect;
using HolyHell.Data.Type;
using UnityEngine;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// Swift buff - When the owner plays a card, also trigger adjacent card(s) in the specified direction(s).
    ///
    /// Parameter format examples:
    ///   "L1"    -> trigger 1 card to the left
    ///   "R1"    -> trigger 1 card to the right
    ///   "L1R1"  -> trigger 1 card to the left AND 1 card to the right
    ///
    /// The buff is consumed after one card is played regardless of whether adjacent cards existed.
    /// If the played card is already at the edge and the direction has no card, that side simply does not trigger.
    /// </summary>
    public class SwiftBuff : BuffBase
    {
        public override bool IsStackable => false;
        public override bool IsPositive => true;

        /// <summary>Number of cards to trigger to the left (0 if none)</summary>
        public int LeftCount { get; private set; }

        /// <summary>Number of cards to trigger to the right (0 if none)</summary>
        public int RightCount { get; private set; }

        public SwiftBuff(string directionParam, int duration = -1)
            : base(BuffType.Swift.ToString(), 1, duration)
        {
            ParseDirection(directionParam);
        }

        /// <summary>
        /// Parse direction string like "L1", "R1", "L1R1", "LR1" etc.
        /// Supports: L{n}, R{n}, L{n}R{m}, LR{n} (LR{n} means n cards on each side)
        /// </summary>
        private void ParseDirection(string param)
        {
            LeftCount = 0;
            RightCount = 0;

            if (string.IsNullOrWhiteSpace(param))
            {
                Debug.LogWarning("SwiftBuff: Empty direction parameter");
                return;
            }

            string s = param.Trim();

            // Try LR{n} pattern first (e.g. "LR1" = 1 each side)
            if (s.StartsWith("LR", System.StringComparison.OrdinalIgnoreCase))
            {
                string numPart = s.Substring(2);
                if (int.TryParse(numPart, out int n))
                {
                    LeftCount = n;
                    RightCount = n;
                    return;
                }
            }

            // Try L{n}R{m} pattern (e.g. "L1R1", "L2R1")
            int rIndex = s.IndexOf('R', System.StringComparison.OrdinalIgnoreCase);
            if (s.StartsWith("L", System.StringComparison.OrdinalIgnoreCase) && rIndex > 0)
            {
                string leftPart = s.Substring(1, rIndex - 1);
                string rightPart = s.Substring(rIndex + 1);
                int.TryParse(leftPart, out var l);
                LeftCount = l;
                int.TryParse(rightPart, out var r);
                RightCount = r;
                return;
            }

            // Single direction: L{n} or R{n}
            if (s.StartsWith("L", System.StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(s.Substring(1), out var l);
                LeftCount = l;
            }
            else if (s.StartsWith("R", System.StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(s.Substring(1), out var r);
                RightCount = r;
            }
            else
            {
                Debug.LogWarning($"SwiftBuff: Could not parse direction '{param}'");
            }
        }

        /// <summary>
        /// Called when the owner plays a card.
        /// Triggers adjacent cards based on parsed direction, then consumes this buff.
        /// </summary>
        public override bool OnCardPlayed(EffectContext context)
        {
            if (context.DeckManager == null || context.CurrentCard == null)
            {
                Debug.LogWarning("SwiftBuff: DeckManager or CurrentCard is null");
                Duration.Value = 0; // Still consume the buff
                return true;
            }

            int currentIndex = context.DeckManager.GetCardIndexInHand(context.CurrentCard);
            if (currentIndex == -1)
            {
                Debug.LogWarning("SwiftBuff: CurrentCard not found in hand");
                Duration.Value = 0;
                return true;
            }

            bool anyKilled = false;

            // Trigger left
            if (LeftCount > 0)
            {
                int leftIndex = currentIndex - LeftCount;
                anyKilled |= TriggerCardAt(leftIndex, context);
            }

            // Trigger right
            if (RightCount > 0)
            {
                int rightIndex = currentIndex + RightCount;
                anyKilled |= TriggerCardAt(rightIndex, context);
            }

            // Consume buff after use
            Duration.Value = 0;
            return true;
        }

        private bool TriggerCardAt(int index, EffectContext context)
        {
            if (index < 0 || index >= context.DeckManager.HandSize)
            {
                Debug.Log($"SwiftBuff: Index {index} out of hand bounds -- no trigger on this side");
                return false;
            }

            var card = context.DeckManager.GetCardInHand(index);
            if (card == null || card.Effects == null)
            {
                Debug.Log($"SwiftBuff: No card or no effects at index {index}");
                return false;
            }

            Debug.Log($"SwiftBuff: Triggering adjacent card '{card.DisplayName}' at index {index}");

            bool killed = false;
            foreach (var effect in card.Effects)
            {
                if (effect != null && effect.Execute(context))
                {
                    killed = true;
                }
            }
            return killed;
        }
    }
}
