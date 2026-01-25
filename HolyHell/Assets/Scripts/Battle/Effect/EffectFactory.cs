using UnityEngine;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Factory for creating effect instances based on CardEffectType
    /// </summary>
    public static class EffectFactory
    {
        /// <summary>
        /// Create an effect instance from type, value, and requirement
        /// </summary>
        public static EffectBase CreateEffect(CardEffectType effectType, string value, string requirement = null)
        {
            if (effectType == CardEffectType.None || string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            switch (effectType)
            {
                case CardEffectType.SingleDamage:
                    return new SingleDamageEffect(value, requirement);

                case CardEffectType.AOEDamage:
                    return new AOEDamageEffect(value, requirement);

                case CardEffectType.SelfBuff:
                    return new SelfBuffEffect(value, requirement);

                case CardEffectType.TargetSingleBuff:
                    return new TargetSingleBuffEffect(value, requirement);

                case CardEffectType.TargetAOEBuff:
                    return new TargetAOEBuffEffect(value, requirement);

                case CardEffectType.InstDraw:
                    return new InstDrawEffect(value, requirement);

                case CardEffectType.DeckBurn:
                    return new DeckBurnEffect(value, requirement);

                case CardEffectType.HandBurn:
                    return new HandBurnEffect(value, requirement);

                case CardEffectType.CleanseSelf:
                    return new CleanseSelfEffect(value, requirement);

                case CardEffectType.CastMore:
                    return new CastMoreEffect(value, requirement);

                case CardEffectType.SelfDamage:
                    return new SelfDamageEffect(value, requirement);

                case CardEffectType.DelaySingleDamage:
                    return new DelaySingleDamageEffect(value, requirement);

                case CardEffectType.DelayAOEDamage:
                    return new DelayAOEDamageEffect(value, requirement);

                default:
                    Debug.LogWarning($"EffectFactory: Unknown effect type {effectType}");
                    return null;
            }
        }

        /// <summary>
        /// Create effects from CardRow data (up to 4 effects)
        /// </summary>
        public static System.Collections.Generic.List<EffectBase> CreateEffectsFromCardRow(CardRow cardRow)
        {
            var effects = new System.Collections.Generic.List<EffectBase>();

            // Effect 1
            if (cardRow.Effect1Type != CardEffectType.None)
            {
                var effect = CreateEffect(cardRow.Effect1Type, cardRow.Effect1Value, cardRow.Effect1Requirement);
                if (effect != null) effects.Add(effect);
            }

            // Effect 2
            if (cardRow.Effect2Type != CardEffectType.None)
            {
                var effect = CreateEffect(cardRow.Effect2Type, cardRow.Effect2Value, cardRow.Effect2Requirement);
                if (effect != null) effects.Add(effect);
            }

            // Effect 3
            if (cardRow.Effect3Type != CardEffectType.None)
            {
                var effect = CreateEffect(cardRow.Effect3Type, cardRow.Effect3Value, cardRow.Effect3Requirement);
                if (effect != null) effects.Add(effect);
            }

            // Effect 4
            if (cardRow.Effect4Type != CardEffectType.None)
            {
                var effect = CreateEffect(cardRow.Effect4Type, cardRow.Effect4Value, cardRow.Effect4Requirement);
                if (effect != null) effects.Add(effect);
            }

            return effects;
        }
    }
}
