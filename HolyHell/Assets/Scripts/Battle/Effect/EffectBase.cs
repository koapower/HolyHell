using UnityEngine;
using HolyHell.Battle.Card;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Base class for all card effects
    /// Effects can be used by both players and enemies
    /// </summary>
    public abstract class EffectBase
    {
        /// <summary>
        /// Effect type identifier
        /// </summary>
        public abstract CardEffectType EffectType { get; }

        /// <summary>
        /// Effect value (can be damage amount, buff ID, etc.)
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Requirement string for this effect
        /// </summary>
        public string Requirement { get; set; }

        protected EffectBase(string value, string requirement = null)
        {
            Value = value;
            Requirement = requirement;
        }

        /// <summary>
        /// Execute the effect with given context
        /// Returns true if the effect caused a kill
        /// </summary>
        public abstract bool Execute(EffectContext context);

        /// <summary>
        /// Check if this effect's requirement is met
        /// </summary>
        public virtual bool CheckRequirement(EffectContext context, EffectRequirementEvaluator evaluator)
        {
            if (string.IsNullOrWhiteSpace(Requirement))
            {
                return true;
            }

            // Check for SpendRepeat - this is handled specially by executor
            if (Requirement.StartsWith("SpendRepeat", System.StringComparison.OrdinalIgnoreCase))
            {
                return true; // Will be handled by executor
            }

            return evaluator.Evaluate(
                Requirement,
                context.DeckManager,
                context.Target,
                context.KillOccurred
            );
        }

        /// <summary>
        /// Check if this effect uses SpendRepeat mechanism
        /// </summary>
        public bool IsSpendRepeat()
        {
            return !string.IsNullOrWhiteSpace(Requirement) &&
                   Requirement.StartsWith("SpendRepeat", System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get SpendRepeat parameter string
        /// </summary>
        public string GetSpendRepeatParam()
        {
            if (!IsSpendRepeat())
            {
                return null;
            }

            return Requirement.Substring("SpendRepeat=".Length).Trim();
        }

        /// <summary>
        /// Clone this effect (for dynamic card modification)
        /// </summary>
        public abstract EffectBase Clone();

        /// <summary>
        /// Get description of this effect for UI display
        /// </summary>
        public virtual string GetDescription()
        {
            return $"{EffectType}: {Value}";
        }
    }
}
