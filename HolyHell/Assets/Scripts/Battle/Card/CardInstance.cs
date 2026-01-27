using HolyHell.Battle.Effect;
using System;
using System.Collections.Generic;

namespace HolyHell.Battle.Card
{
    /// <summary>
    /// Runtime instance of a card, based on CardRow data
    /// </summary>
    public class CardInstance
    {
        // Reference to static card data
        public CardRow cardData;

        // Runtime instance ID
        public Guid instanceId;

        // Runtime effects (can be dynamically modified)
        public List<EffectBase> Effects { get; set; }

        // Constructor
        public CardInstance(CardRow data)
        {
            cardData = data;
            instanceId = Guid.NewGuid();

            // Initialize effects from card data
            Effects = EffectFactory.CreateEffectsFromCardRow(data);
        }

        // Convenience accessors
        public string Id => cardData.Id;
        public string DisplayName => cardData.DisplayName;
        public int ActionCost => cardData.ActionCost;
        public int AngelGaugeIncrease => cardData.AngelGaugeIncrease;
        public int DemonGaugeIncrease => cardData.DemonGaugeIncrease;

        // Effect accessors
        public CardEffectType GetEffectType(int index)
        {
            return index switch
            {
                1 => cardData.Effect1Type,
                2 => cardData.Effect2Type,
                3 => cardData.Effect3Type,
                4 => cardData.Effect4Type,
                _ => CardEffectType.None
            };
        }

        public string GetEffectValue(int index)
        {
            return index switch
            {
                1 => cardData.Effect1Value,
                2 => cardData.Effect2Value,
                3 => cardData.Effect3Value,
                4 => cardData.Effect4Value,
                _ => string.Empty
            };
        }

        public string GetEffectRequirement(int index)
        {
            return index switch
            {
                1 => cardData.Effect1Requirement,
                2 => cardData.Effect2Requirement,
                3 => cardData.Effect3Requirement,
                4 => cardData.Effect4Requirement,
                _ => string.Empty
            };
        }
    }
}
