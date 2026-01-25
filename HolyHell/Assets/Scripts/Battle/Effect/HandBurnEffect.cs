using HolyHell.Battle.Card;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    public class HandBurnEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.HandBurn;

        public HandBurnEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int count = EffectValueParser.ParseInt(Value);

            if (context.DeckManager != null)
            {
                context.DeckManager.DestroyRandomCardsFromHand(count);
                Debug.Log($"Destroyed {count} cards from hand");
            }
            else
            {
                Debug.LogWarning("HandBurnEffect: DeckManager is null");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new HandBurnEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Destroy {Value} cards from hand";
        }
    }
}
