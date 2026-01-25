using HolyHell.Battle.Entity;
using HolyHell.Battle.Logic;
using UnityEngine;

namespace HolyHell.Battle.Card
{
    /// <summary>
    /// Handles SpendRepeat mechanism for card effects
    /// Format: "SpendRepeat=ResourceType, MaxSpend, CostPerRepeat"
    /// Example: "SpendRepeat=Ametervalue, 30, 10" means spend up to 30 angel meter, 10 per repeat
    /// </summary>
    public static class SpendRepeatExecutor
    {
        /// <summary>
        /// Calculate how many times an effect can repeat based on SpendRepeat parameters
        /// </summary>
        /// <param name="spendRepeatParam">SpendRepeat parameter string (e.g., "Ametervalue, 30, 10")</param>
        /// <param name="player">Player entity</param>
        /// <param name="actualSpent">Out: Actual amount of resource spent</param>
        /// <returns>Number of times the effect can repeat</returns>
        public static int CalculateRepeatCount(string spendRepeatParam, PlayerEntity player, out int actualSpent)
        {
            actualSpent = 0;

            if (!EffectValueParser.ParseSpendRepeatParams(spendRepeatParam, out string resourceType, out int maxSpend, out int costPerRepeat))
            {
                Debug.LogWarning($"Failed to parse SpendRepeat parameters: {spendRepeatParam}");
                return 0;
            }

            if (costPerRepeat <= 0)
            {
                Debug.LogWarning($"Invalid cost per repeat: {costPerRepeat}");
                return 0;
            }

            // Get current resource value
            int currentValue = GetResourceValue(resourceType, player);

            if (currentValue <= 0)
            {
                Debug.Log($"SpendRepeat: No {resourceType} available to spend");
                return 0;
            }

            // Calculate how much can be spent
            int availableToSpend = Mathf.Min(currentValue, maxSpend);

            // Special handling for HP - cannot spend HP to 0 or below
            if (resourceType.Equals("HP", System.StringComparison.OrdinalIgnoreCase))
            {
                availableToSpend = Mathf.Min(availableToSpend, currentValue - 1);
            }

            // Calculate repeat count
            int repeatCount = availableToSpend / costPerRepeat;
            actualSpent = repeatCount * costPerRepeat;

            Debug.Log($"SpendRepeat: Resource={resourceType}, Current={currentValue}, MaxSpend={maxSpend}, Cost={costPerRepeat}, Repeats={repeatCount}, ActualSpent={actualSpent}");

            return repeatCount;
        }

        /// <summary>
        /// Consume resources for SpendRepeat
        /// </summary>
        /// <param name="resourceType">Type of resource to spend</param>
        /// <param name="amount">Amount to spend</param>
        /// <param name="player">Player entity</param>
        public static void ConsumeResource(string resourceType, int amount, PlayerEntity player)
        {
            if (amount <= 0)
            {
                return;
            }

            string normalizedType = resourceType.ToLower();

            switch (normalizedType)
            {
                case "hp":
                    int newHp = Mathf.Max(1, player.hp.CurrentValue - amount); // Cannot go to 0 or below
                    player.hp.Value = newHp;
                    Debug.Log($"SpendRepeat consumed {amount} HP (HP: {newHp}/{player.maxHp.CurrentValue})");
                    break;

                case "ap":
                case "actionpoint":
                    int newAp = Mathf.Max(0, player.actionPoint.CurrentValue - amount);
                    player.actionPoint.Value = newAp;
                    Debug.Log($"SpendRepeat consumed {amount} AP (AP: {newAp})");
                    break;

                case "ametervalue":
                    player.ModifyAngelGauge(-amount);
                    Debug.Log($"SpendRepeat consumed {amount} Angel Meter (Now: {player.angelGauge.CurrentValue})");
                    break;

                case "dmetervalue":
                    player.ModifyDemonGauge(-amount);
                    Debug.Log($"SpendRepeat consumed {amount} Demon Meter (Now: {player.demonGauge.CurrentValue})");
                    break;

                default:
                    Debug.LogWarning($"Unknown resource type for SpendRepeat: {resourceType}");
                    break;
            }
        }

        /// <summary>
        /// Get current value of a resource
        /// </summary>
        private static int GetResourceValue(string resourceType, PlayerEntity player)
        {
            string normalizedType = resourceType.ToLower();

            return normalizedType switch
            {
                "hp" => player.hp.CurrentValue,
                "ap" or "actionpoint" => player.actionPoint.CurrentValue,
                "ametervalue" => player.angelGauge.CurrentValue,
                "dmetervalue" => player.demonGauge.CurrentValue,
                _ => 0
            };
        }
    }
}
