using UnityEngine;

/// <summary>
/// Executes card effects on targets
/// </summary>
public class CardEffectExecutor
{
    private PlayerEntity player;
    private EffectRequirementEvaluator requirementEvaluator;
    private GaugeModifier gaugeModifier;
    private DamageCalculator damageCalculator;

    public CardEffectExecutor(PlayerEntity owner)
    {
        player = owner;
        requirementEvaluator = new EffectRequirementEvaluator(owner);
        gaugeModifier = new GaugeModifier(owner);
        damageCalculator = new DamageCalculator();
    }

    /// <summary>
    /// Execute all effects of a card on target
    /// </summary>
    public void ExecuteCard(CardInstance card, BattleEntity target)
    {
        // Execute up to 4 effects
        for (int i = 1; i <= 4; i++)
        {
            ExecuteEffect(card, target, i);
        }

        // Modify gauges after all effects
        gaugeModifier.ModifyGauges(card.AngelGaugeIncrease, card.DemonGaugeIncrease);
    }

    /// <summary>
    /// Execute a specific effect index (1-4)
    /// </summary>
    private void ExecuteEffect(CardInstance card, BattleEntity target, int effectIndex)
    {
        var effectType = card.GetEffectType(effectIndex);
        if (effectType == CardEffectType.None)
        {
            return;
        }

        // Check requirement
        var requirement = card.GetEffectRequirement(effectIndex);
        if (!requirementEvaluator.Evaluate(requirement))
        {
            Debug.Log($"Effect {effectIndex} requirement not met: {requirement}");
            return;
        }

        // Get effect value
        string value = card.GetEffectValue(effectIndex);

        // Execute based on effect type
        switch (effectType)
        {
            case CardEffectType.SingleDamage:
                var numberParsed = float.TryParse(value, out float valueParsed);
                ExecuteSingleDamage(target, valueParsed);
                break;

            //case CardEffectType.Shield:
            //    ExecuteShield(player, value);
            //    break;

            //case CardEffectType.Heal:
            //    ExecuteHeal(player, value);
            //    break;

            //case CardEffectType.DrawCard:
            //    ExecuteDrawCard((int)value);
            //    break;

            //case CardEffectType.DiscardCard:
            //    ExecuteDiscardCard((int)value);
            //    break;


            default:
                Debug.LogWarning($"Unknown effect type: {effectType}");
                break;
        }
    }

    private void ExecuteSingleDamage(BattleEntity target, float baseDamage)
    {
        if (target == null)
        {
            Debug.LogWarning("Target is null for damage effect");
            return;
        }

        float finalDamage = damageCalculator.CalculateDamage(baseDamage, player, target);
        damageCalculator.ApplyDamage(target, finalDamage);
    }

    private void ExecuteShield(BattleEntity entity, float shieldAmount)
    {
        entity.shield.Value += (int)shieldAmount;
        Debug.Log($"{entity.GetType().Name} gained {shieldAmount} shield");
    }

    private void ExecuteHeal(BattleEntity entity, float healAmount)
    {
        int newHp = Mathf.Min(entity.hp.Value + (int)healAmount, entity.maxHp.Value);
        entity.hp.Value = newHp;
        Debug.Log($"{entity.GetType().Name} healed for {healAmount} HP");
    }

    private void ExecuteDrawCard(int count)
    {
        if (player.deckManager != null)
        {
            player.deckManager.DrawCards(count);
            Debug.Log($"Drew {count} cards");
        }
    }

    private void ExecuteDiscardCard(int count)
    {
        // For now, discard random cards from hand
        for (int i = 0; i < count && player.hand.Count > 0; i++)
        {
            var randomCard = player.hand[Random.Range(0, player.hand.Count)];
            player.deckManager.DiscardCard(randomCard);
        }
        Debug.Log($"Discarded {count} cards");
    }
}
