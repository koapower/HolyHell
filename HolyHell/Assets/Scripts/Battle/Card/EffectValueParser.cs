using System;

namespace HolyHell.Battle.Card
{
    /// <summary>
    /// Utility class for parsing effect value strings from card data
    /// </summary>
    public static class EffectValueParser
    {
        /// <summary>
        /// Parse a single integer value from string
        /// </summary>
        /// <param name="value">String value to parse (e.g., "5")</param>
        /// <returns>Parsed integer value, or 0 if parsing fails</returns>
        public static int ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            if (int.TryParse(value.Trim(), out int result))
                return result;

            return 0;
        }

        /// <summary>
        /// Parse direction and count from string (for CastMore effect)
        /// </summary>
        /// <param name="value">String value to parse (e.g., "Left, 1" or "Right, 1")</param>
        /// <param name="count">Output: number of cards to cast</param>
        /// <returns>Direction string ("Left" or "Right"), or empty if parsing fails</returns>
        public static string ParseDirection(string value, out int count)
        {
            count = 0;

            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string[] parts = value.Split(',');
            if (parts.Length != 2)
                return string.Empty;

            string direction = parts[0].Trim();
            count = ParseInt(parts[1]);

            return direction;
        }

        /// <summary>
        /// Parse delay parameters (for DelaySingleDamage and DelayAOEDamage effects)
        /// </summary>
        /// <param name="value">String value to parse (e.g., "1,3" means damage=1, delay=3)</param>
        /// <param name="damage">Output: damage value</param>
        /// <param name="delay">Output: delay in turns</param>
        /// <returns>True if parsing successful, false otherwise</returns>
        public static bool ParseDelayParams(string value, out int damage, out int delay)
        {
            damage = 0;
            delay = 0;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            string[] parts = value.Split(',');
            if (parts.Length != 2)
                return false;

            damage = ParseInt(parts[0]);
            delay = ParseInt(parts[1]);

            return true;
        }

        /// <summary>
        /// Parse SpendRepeat parameters
        /// </summary>
        /// <param name="value">String value to parse (e.g., "Ametervalue, 30, 10")</param>
        /// <param name="resourceType">Output: resource type (HP, AP, Ametervalue, Dmetervalue)</param>
        /// <param name="maxSpend">Output: maximum amount to spend</param>
        /// <param name="costPerRepeat">Output: cost per repeat execution</param>
        /// <returns>True if parsing successful, false otherwise</returns>
        public static bool ParseSpendRepeatParams(string value, out string resourceType, out int maxSpend, out int costPerRepeat)
        {
            resourceType = string.Empty;
            maxSpend = 0;
            costPerRepeat = 0;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            string[] parts = value.Split(',');
            if (parts.Length != 3)
                return false;

            resourceType = parts[0].Trim();
            maxSpend = ParseInt(parts[1]);
            costPerRepeat = ParseInt(parts[2]);

            return true;
        }

        /// <summary>
        /// Parse float value from string (for percentage-based buffs)
        /// </summary>
        /// <param name="value">String value to parse (e.g., "15" for 15%)</param>
        /// <returns>Parsed float value, or 0 if parsing fails</returns>
        public static float ParseFloat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0f;

            if (float.TryParse(value.Trim(), out float result))
                return result;

            return 0f;
        }

        /// <summary>
        /// Parse buff ID from string (removes any whitespace)
        /// </summary>
        /// <param name="value">String value to parse</param>
        /// <returns>Trimmed buff ID string</returns>
        public static string ParseBuffId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim();
        }
    }
}
