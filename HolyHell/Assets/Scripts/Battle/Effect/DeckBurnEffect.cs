using HolyHell.Battle.Card;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    public class DeckBurnEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.DeckBurn;

        public DeckBurnEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int count = EffectValueParser.ParseInt(Value);

            if (context.DeckManager != null)
            {
                context.DeckManager.DestroyRandomCardsFromDeck(count);
                Debug.Log($"Destroyed {count} cards from deck");
            }
            else
            {
                Debug.LogWarning("DeckBurnEffect: DeckManager is null");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new DeckBurnEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            return $"Destroy {Value} cards from deck";
        }
    }
}
