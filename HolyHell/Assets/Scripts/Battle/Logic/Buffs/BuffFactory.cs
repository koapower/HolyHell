using HolyHell.Data.Type;
using UnityEngine;

namespace HolyHell.Battle.Logic.Buffs
{
    /// <summary>
    /// Factory class for creating buff instances based on BuffType
    /// </summary>
    public static class BuffFactory
    {
        /// <summary>
        /// Create a buff instance based on type and parameters
        /// </summary>
        /// <param name="buffType">Type of buff to create</param>
        /// <param name="parameter">Parameter string (e.g., percentage value, element type)</param>
        /// <param name="stackCount">Initial stack count</param>
        /// <param name="duration">Duration in turns (-1 = permanent)</param>
        /// <returns>Created buff instance, or null if invalid type</returns>
        public static BuffBase CreateBuff(BuffType buffType, string parameter = null, int stackCount = 1, int duration = -1)
        {
            switch (buffType)
            {
                case BuffType.Guard:
                    return new GuardBuff(duration);

                case BuffType.Blessed:
                    float blessedPercent = ParseFloat(parameter, 5f); // Default 5% heal
                    return new BlessedBuff(blessedPercent, duration);

                case BuffType.LifeSteal:
                    return new LifesteelBuff(duration);

                case BuffType.ReqChange:
                    int reqModifier = ParseInt(parameter, 0);
                    return new ReqChangeBuff(reqModifier, duration);

                case BuffType.IncreaseDmg:
                    float increaseDmgPercent = ParseFloat(parameter, 10f); // Default 10% increase
                    return new IncreaseDmgBuff(increaseDmgPercent, duration);

                case BuffType.BoostDmg:
                    int boostAmount = ParseInt(parameter, 1); // Default +1 damage
                    return new BoostDmgBuff(boostAmount, stackCount, duration);

                // Increase Resistance buffs -- all route to IncreaseResBuff with the corresponding ElementType
                case BuffType.IncreaseAllRes:
                    return new IncreaseResBuff(ElementType.All, ParseFloat(parameter, 0f), stackCount, duration);
                case BuffType.IncreaseEnlRes:
                    return new IncreaseResBuff(ElementType.Enlightened, ParseFloat(parameter, 0f), stackCount, duration);
                case BuffType.IncreaseDomRes:
                    return new IncreaseResBuff(ElementType.Domination, ParseFloat(parameter, 0f), stackCount, duration);
                case BuffType.IncreaseBliRes:
                    return new IncreaseResBuff(ElementType.Bliss, ParseFloat(parameter, 0f), stackCount, duration);
                case BuffType.IncreaseRavRes:
                    return new IncreaseResBuff(ElementType.Ravenous, ParseFloat(parameter, 0f), stackCount, duration);
                case BuffType.IncreaseDesRes:
                    return new IncreaseResBuff(ElementType.Despair, ParseFloat(parameter, 0f), stackCount, duration);

                // Decrease Resistance buffs -- all route to ReduceResBuff with the corresponding ElementType
                case BuffType.DecreaseAllRes:
                    return new ReduceResBuff(ElementType.All, ParseFloat(parameter, 0f), stackCount, duration);
                case BuffType.DecreaseEnlRes:
                    return new ReduceResBuff(ElementType.Enlightened, ParseFloat(parameter, 0f), stackCount, duration);
                case BuffType.DecreaseDomRes:
                    return new ReduceResBuff(ElementType.Domination, ParseFloat(parameter, 0f), stackCount, duration);
                case BuffType.DecreaseBliRes:
                    return new ReduceResBuff(ElementType.Bliss, ParseFloat(parameter, 0f), stackCount, duration);
                case BuffType.DecreaseRavRes:
                    return new ReduceResBuff(ElementType.Ravenous, ParseFloat(parameter, 0f), stackCount, duration);
                case BuffType.DecreaseDesRes:
                    return new ReduceResBuff(ElementType.Despair, ParseFloat(parameter, 0f), stackCount, duration);

                case BuffType.Fragile:
                    float fragilePercent = ParseFloat(parameter, 10f); // Default 10% increased damage taken
                    return new FragileBuff(fragilePercent, stackCount, duration);

                case BuffType.Bleeding:
                    float bleedingPercent = ParseFloat(parameter, 5f); // Default 5% HP damage
                    return new BleedingBuff(bleedingPercent, stackCount, duration);

                case BuffType.Feared:
                    return new FearedBuff(duration);

                case BuffType.ReduceAtk:
                    float reduceAtkPercent = ParseFloat(parameter, 10f); // Default 10% attack reduction
                    return new ReduceAtkBuff(reduceAtkPercent, stackCount, duration);

                case BuffType.Cursed:
                    return new CursedBuff(duration);

                case BuffType.Gifted:
                    return new GiftedBuff(stackCount, duration);

                case BuffType.Swift:
                    return new SwiftBuff(parameter, duration);

                case BuffType.None:
                default:
                    Debug.LogWarning($"Invalid or unsupported BuffType: {buffType}");
                    return null;
            }
        }

        /// <summary>
        /// Create a buff from string ID (for compatibility with CSV data)
        /// </summary>
        public static BuffBase CreateBuffFromId(string buffId, string parameter = null, int stackCount = 1, int duration = -1)
        {
            if (string.IsNullOrWhiteSpace(buffId))
                return null;

            // Try to parse buff ID as BuffType enum
            if (System.Enum.TryParse<BuffType>(buffId, true, out BuffType buffType))
            {
                return CreateBuff(buffType, parameter, stackCount, duration);
            }

            Debug.LogWarning($"Could not parse buff ID: {buffId}");
            return null;
        }

        // Helper methods for parsing parameters
        private static int ParseInt(string value, int defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (int.TryParse(value.Trim(), out int result))
                return result;

            return defaultValue;
        }

        private static float ParseFloat(string value, float defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (float.TryParse(value.Trim(), out float result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// Parse element type and percentage value from parameter string
        /// Format: "ElementType, Value" (e.g., "Holy, 15")
        /// </summary>
        private static (ElementType element, float value) ParseElementAndValue(string parameter)
        {
            ElementType element = ElementType.None; // Default
            float value = 10f; // Default 10%

            if (string.IsNullOrWhiteSpace(parameter))
                return (element, value);

            string[] parts = parameter.Split(',');
            if (parts.Length >= 1)
            {
                // Try to parse element type
                if (System.Enum.TryParse<ElementType>(parts[0].Trim(), true, out ElementType parsedElement))
                {
                    element = parsedElement;
                }
            }

            if (parts.Length >= 2)
            {
                // Parse value
                value = ParseFloat(parts[1], value);
            }

            return (element, value);
        }
    }
}
