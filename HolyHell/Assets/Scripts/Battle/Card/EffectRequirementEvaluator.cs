using System.Text.RegularExpressions;

/// <summary>
/// Evaluates card effect requirements (e.g., "Ametervalue>=25")
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
    public bool Evaluate(string requirement)
    {
        // Empty requirement = always true
        if (string.IsNullOrEmpty(requirement))
        {
            return true;
        }

        // Parse requirement string
        // Format examples: "Ametervalue>=25", "Dmetervalue<50", "Ametervalue==100"

        // Match pattern: variable + operator + number
        var match = Regex.Match(requirement, @"(\w+)(>=|<=|>|<|==|!=)(\d+)");

        if (!match.Success)
        {
            // Invalid format, default to true to avoid blocking
            UnityEngine.Debug.LogWarning($"Invalid requirement format: {requirement}");
            return true;
        }

        string variable = match.Groups[1].Value;
        string op = match.Groups[2].Value;
        int threshold = int.Parse(match.Groups[3].Value);

        // Get actual value
        int actualValue = GetVariableValue(variable);

        // Evaluate condition
        return op switch
        {
            ">=" => actualValue >= threshold,
            "<=" => actualValue <= threshold,
            ">" => actualValue > threshold,
            "<" => actualValue < threshold,
            "==" => actualValue == threshold,
            "!=" => actualValue != threshold,
            _ => true
        };
    }

    /// <summary>
    /// Get value of a variable by name
    /// </summary>
    private int GetVariableValue(string variableName)
    {
        return variableName.ToLower() switch
        {
            "ametervalue" => player.angelGauge.Value,
            "dmetervalue" => player.demonGauge.Value,
            "hp" => player.hp.Value,
            "shield" => player.shield.Value,
            "actionpoint" => player.actionPoint.Value,
            _ => 0
        };
    }
}
