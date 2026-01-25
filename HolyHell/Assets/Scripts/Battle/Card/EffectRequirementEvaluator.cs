using HolyHell.Battle.Entity;
using HolyHell.Data.Type;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HolyHell.Battle.Card
{
    /// <summary>
    /// Evaluates card effect requirements
    /// </summary>
    public class EffectRequirementEvaluator
{
    private PlayerEntity player;

    public EffectRequirementEvaluator(PlayerEntity owner)
    {
        player = owner;
    }

    /// <summary>
    /// Evaluate if requirement is met
    /// Returns true if requirement is empty/null or condition is satisfied
    /// </summary>
    public bool Evaluate(
        string requirement,
        CardDeckManager deckManager = null,
        BattleEntity target = null,
        bool killOccurred = false)
    {
        // Empty requirement = always true
        if (string.IsNullOrWhiteSpace(requirement))
        {
            return true;
        }

        string req = requirement.Trim();

        // Check for OnKill requirement
        if (req.Equals("Onkill", System.StringComparison.OrdinalIgnoreCase))
        {
            return killOccurred;
        }

        // Check for NoKill requirement
        if (req.Equals("Nokill", System.StringComparison.OrdinalIgnoreCase))
        {
            return !killOccurred;
        }

        // Check for BuffApplied requirement (format: "BuffApplied=BuffName")
        if (req.StartsWith("BuffApplied", System.StringComparison.OrdinalIgnoreCase))
        {
            return EvaluateBuffApplied(req, target);
        }

        // Check for Handsize requirement (format: "Handsize>=5", "Handsize<=3", "Handsize=2")
        if (req.StartsWith("Handsize", System.StringComparison.OrdinalIgnoreCase))
        {
            return EvaluateHandsize(req, deckManager);
        }

        // Check for SpendRepeat requirement (format: "SpendRepeat=Ametervalue, 30, 10")
        if (req.StartsWith("SpendRepeat", System.StringComparison.OrdinalIgnoreCase))
        {
            // SpendRepeat is handled separately in CardEffectExecutor
            // Return true here to pass initial check
            return true;
        }

        // Check for standard meter/stat comparisons (e.g., "Ametervalue>=25")
        return EvaluateComparison(req);
    }

    /// <summary>
    /// Evaluate BuffApplied requirement
    /// Format: "BuffApplied=BuffName"
    /// </summary>
    private bool EvaluateBuffApplied(string requirement, BattleEntity target)
    {
        if (target == null)
        {
            Debug.LogWarning("BuffApplied requirement needs a target");
            return false;
        }

        // Parse buff name
        var match = Regex.Match(requirement, @"BuffApplied\s*=\s*(.+)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            Debug.LogWarning($"Invalid BuffApplied format: {requirement}");
            return false;
        }

        string buffName = match.Groups[1].Value.Trim();

        // Check if target has the buff
        bool hasBuff = target.buffHandler.HasBuff(buffName);
        Debug.Log($"BuffApplied check: Target has buff '{buffName}' = {hasBuff}");

        return hasBuff;
    }

    /// <summary>
    /// Evaluate Handsize requirement
    /// Format: "Handsize>=5", "Handsize<=3", "Handsize=2"
    /// </summary>
    private bool EvaluateHandsize(string requirement, CardDeckManager deckManager)
    {
        if (deckManager == null)
        {
            Debug.LogWarning("Handsize requirement needs DeckManager");
            return false;
        }

        // Match pattern: Handsize + operator + number
        var match = Regex.Match(requirement, @"Handsize\s*(>=|<=|>|<|==|!=|=)\s*(\d+)", RegexOptions.IgnoreCase);

        if (!match.Success)
        {
            Debug.LogWarning($"Invalid Handsize format: {requirement}");
            return false;
        }

        string op = match.Groups[1].Value;
        int threshold = int.Parse(match.Groups[2].Value);
        int handSize = deckManager.HandSize;

        // Normalize operator (handle '=' as '==')
        if (op == "=") op = "==";

        // Evaluate condition
        bool result = op switch
        {
            ">=" => handSize >= threshold,
            "<=" => handSize <= threshold,
            ">" => handSize > threshold,
            "<" => handSize < threshold,
            "==" => handSize == threshold,
            "!=" => handSize != threshold,
            _ => false
        };

        Debug.Log($"Handsize check: {handSize} {op} {threshold} = {result}");
        return result;
    }

    /// <summary>
    /// Evaluate standard comparison requirement
    /// Format: "Ametervalue>=25", "Dmetervalue<50", "hp>10"
    /// </summary>
    private bool EvaluateComparison(string requirement)
    {
        // Match pattern: variable + operator + number
        var match = Regex.Match(requirement, @"(\w+)\s*(>=|<=|>|<|==|!=|=)\s*(\d+)");

        if (!match.Success)
        {
            Debug.LogWarning($"Invalid requirement format: {requirement}");
            return true; // Default to true to avoid blocking
        }

        string variable = match.Groups[1].Value;
        string op = match.Groups[2].Value;
        int threshold = int.Parse(match.Groups[3].Value);

        // Normalize operator (handle '=' as '==')
        if (op == "=") op = "==";

        // Get actual value
        int actualValue = GetVariableValue(variable);

        // Evaluate condition
        bool result = op switch
        {
            ">=" => actualValue >= threshold,
            "<=" => actualValue <= threshold,
            ">" => actualValue > threshold,
            "<" => actualValue < threshold,
            "==" => actualValue == threshold,
            "!=" => actualValue != threshold,
            _ => true
        };

        Debug.Log($"Requirement check: {variable}={actualValue} {op} {threshold} = {result}");
        return result;
    }

    /// <summary>
    /// Get value of a variable by name
    /// </summary>
    private int GetVariableValue(string variableName)
    {
        return variableName.ToLower() switch
        {
            "ametervalue" => player.angelGauge.CurrentValue,
            "dmetervalue" => player.demonGauge.CurrentValue,
            "hp" => player.hp.CurrentValue,
            "shield" => player.shield.CurrentValue,
            "actionpoint" => player.actionPoint.CurrentValue,
            _ => 0
        };
    }
}
}
