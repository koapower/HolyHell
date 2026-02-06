using HolyHell.Battle.Effect;
using HolyHell.Battle.Entity;
using HolyHell.Battle.Logic;
using System.Collections.Generic;
using UnityEngine;

namespace HolyHell.Battle.Card
{
    /// <summary>
    /// Executes card effects on targets
    /// </summary>
    public class CardEffectExecutor
    {
        private BattleEntity caster;
        private EffectRequirementEvaluator requirementEvaluator;
        private EffectContext context;

        public CardEffectExecutor(
            IBattleManager manager,
            BattleEntity caster,
            CardDeckManager deckManager = null,
            DelayedEffectQueue delayedQueue = null)
        {
            this.caster = caster;
            requirementEvaluator = new EffectRequirementEvaluator(caster as PlayerEntity);

            // Initialize context
            context = new EffectContext(manager, caster, null, deckManager, delayedQueue);
        }

        /// <summary>
        /// Execute all effects of a card on target
        /// </summary>
        public void ExecuteCard(CardInstance card, BattleEntity target)
        {
            if (card == null || card.Effects == null)
            {
                Debug.LogWarning("CardEffectExecutor: Card or card effects are null");
                return;
            }

            // Update context
            context.Target = target;
            context.CurrentCard = card;
            context.KillOccurred = false;

            // Execute all effects
            foreach (var effect in card.Effects)
            {
                if (effect == null)
                    continue;

                bool killed = ExecuteEffect(effect);
                if (killed)
                {
                    context.KillOccurred = true;
                }
            }

            // Modify gauges after all effects
            if (caster is PlayerEntity player)
            {
                player.ModifyAngelGauge(card.AngelGaugeIncrease);
                player.ModifyDemonGauge(card.DemonGaugeIncrease);
            }
        }

        /// <summary>
        /// Execute a single effect
        /// Returns true if effect caused a kill
        /// </summary>
        private bool ExecuteEffect(EffectBase effect)
        {
            // Check if this is a SpendRepeat effect
            if (effect.IsSpendRepeat())
            {
                return ExecuteWithSpendRepeat(effect);
            }

            // Check requirement
            if (!effect.CheckRequirement(context, requirementEvaluator))
            {
                Debug.Log($"Effect {effect.EffectType} requirement not met: {effect.Requirement}");
                return false;
            }

            // Execute effect
            return effect.Execute(context);
        }

        /// <summary>
        /// Execute effect with SpendRepeat mechanism
        /// </summary>
        private bool ExecuteWithSpendRepeat(EffectBase effect)
        {
            if (!(caster is PlayerEntity player))
            {
                Debug.LogWarning("SpendRepeat can only be used by player");
                return false;
            }

            string paramString = effect.GetSpendRepeatParam();
            if (paramString == null)
            {
                Debug.LogWarning("Failed to get SpendRepeat parameter");
                return false;
            }

            // Calculate how many times we can repeat
            int repeatCount = SpendRepeatExecutor.CalculateRepeatCount(paramString, player, out int actualSpent);

            if (repeatCount <= 0)
            {
                Debug.Log("SpendRepeat: Cannot repeat (insufficient resources)");
                return false;
            }

            // Consume resources
            if (EffectValueParser.ParseSpendRepeatParams(paramString, out string resourceType, out _, out _))
            {
                SpendRepeatExecutor.ConsumeResource(resourceType, actualSpent, player);
            }

            // Execute effect multiple times
            bool anyKillOccurred = false;
            Debug.Log($"SpendRepeat: Executing effect {effect.EffectType} {repeatCount} times");

            for (int i = 0; i < repeatCount; i++)
            {
                bool causedKill = effect.Execute(context);
                if (causedKill)
                {
                    anyKillOccurred = true;
                    context.KillOccurred = true;
                }
            }

            return anyKillOccurred;
        }

        /// <summary>
        /// Execute effects directly (for AI or special cases)
        /// </summary>
        public void ExecuteEffects(List<EffectBase> effects, BattleEntity target)
        {
            if (effects == null)
            {
                Debug.LogWarning("CardEffectExecutor: Effects list is null");
                return;
            }

            context.Target = target;
            context.KillOccurred = false;

            foreach (var effect in effects)
            {
                if (effect == null)
                    continue;

                bool killed = ExecuteEffect(effect);
                if (killed)
                {
                    context.KillOccurred = true;
                }
            }
        }
    }
}
