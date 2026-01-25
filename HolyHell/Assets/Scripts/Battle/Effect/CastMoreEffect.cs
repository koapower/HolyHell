using HolyHell.Battle.Card;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    public class CastMoreEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.CastMore;

        public CastMoreEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            string direction = EffectValueParser.ParseDirection(Value, out int count);

            if (context.DeckManager == null || context.CurrentCard == null)
            {
                Debug.LogWarning("CastMoreEffect: DeckManager or CurrentCard is null");
                return false;
            }

            // Get current card index in hand
            int currentIndex = context.DeckManager.GetCardIndexInHand(context.CurrentCard);
            if (currentIndex == -1)
            {
                Debug.LogWarning("CastMoreEffect: Current card not found in hand");
                return false;
            }

            // Determine target index based on direction
            int targetIndex = -1;
            if (direction.Equals("Left", System.StringComparison.OrdinalIgnoreCase))
            {
                targetIndex = currentIndex - count;
            }
            else if (direction.Equals("Right", System.StringComparison.OrdinalIgnoreCase))
            {
                targetIndex = currentIndex + count;
            }
            else
            {
                Debug.LogWarning($"CastMoreEffect: Invalid direction '{direction}'");
                return false;
            }

            // Check if target index is valid
            if (targetIndex < 0 || targetIndex >= context.DeckManager.HandSize)
            {
                Debug.LogWarning($"CastMoreEffect: Target index out of bounds: {targetIndex}");
                return false;
            }

            // Get adjacent card
            var adjacentCard = context.DeckManager.GetCardInHand(targetIndex);
            if (adjacentCard != null && adjacentCard.Effects != null)
            {
                Debug.Log($"CastMore: Executing adjacent card {adjacentCard.DisplayName}");

                // Execute adjacent card's effects
                bool killed = false;
                foreach (var effect in adjacentCard.Effects)
                {
                    if (effect != null && effect.Execute(context))
                    {
                        killed = true;
                    }
                }
                return killed;
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new CastMoreEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            string direction = EffectValueParser.ParseDirection(Value, out int count);
            return $"Cast card {count} position(s) to the {direction}";
        }
    }
}
