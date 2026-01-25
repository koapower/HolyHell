using HolyHell.Battle.Card;
using UnityEngine;

namespace HolyHell.Battle.Effect
{
    public class InstDrawEffect : EffectBase
    {
        public override CardEffectType EffectType => CardEffectType.InstDraw;

        public InstDrawEffect(string value, string requirement = null)
            : base(value, requirement) { }

        public override bool Execute(EffectContext context)
        {
            int count = EffectValueParser.ParseInt(Value);

            if (context.DeckManager != null)
            {
                context.DeckManager.DrawCards(count);
                string cardText = count == 1 ? "card" : "cards";
                Debug.Log($"Drew {count} {cardText}");
            }
            else
            {
                Debug.LogWarning("InstDrawEffect: DeckManager is null");
            }

            return false;
        }

        public override EffectBase Clone()
        {
            return new InstDrawEffect(Value, Requirement);
        }

        public override string GetDescription()
        {
            int count = EffectValueParser.ParseInt(Value);
            return count == 1 ? $"Draw {count} card" : $"Draw {count} cards";
        }
    }
}
