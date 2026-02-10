/// <summary>
/// Enemy AI behavior data from EnemyBehavior.csv
///
/// CSV format:
///   Skills: "SkillID,BaseWeight"  (e.g. "BliSingleTar1,70" means skill BliSingleTar1 has 70% base weight)
///   Conditions: "ConditionType,Value"  (e.g. "SelfHP<=,50")
///   Results: "SkillN,BonusWeight"  (e.g. "Skill2,50" means add 50 to Skill2's weight when condition met)
///
/// Skill selection logic:
///   1. Start with base weights for each skill slot
///   2. Evaluate each condition; if true, add BonusWeight to the referenced skill slot
///   3. Randomly pick a skill based on final accumulated weights
/// </summary>
public class EnemyBehaviorRow
{
    [Column("ID")]
    public int Id;

    // Skills with base weights (format: "SkillID,BaseWeight")
    [Column("Skill1")]
    public string Skill1;

    [Column("Skill2")]
    public string Skill2;

    [Column("Skill3")]
    public string Skill3;

    [Column("Skill4")]
    public string Skill4;

    [Column("Skill5")]
    public string Skill5;

    [Column("Skill6")]
    public string Skill6;

    // Condition-Result pairs (format: "ConditionType,Value" -> "SkillN,BonusWeight")
    [Column("Condition1")]
    public string Condition1;

    [Column("Result1")]
    public string Result1;

    [Column("Condition2")]
    public string Condition2;

    [Column("Result2")]
    public string Result2;

    [Column("Condition3")]
    public string Condition3;

    [Column("Result3")]
    public string Result3;

    [Column("Condition4")]
    public string Condition4;

    [Column("Result4")]
    public string Result4;

    /// <summary>
    /// Returns (skillId, baseWeight) for the given 1-based skill slot index.
    /// Returns (null, 0) if the slot is empty.
    /// </summary>
    public (string skillId, int baseWeight) GetSkillEntry(int index)
    {
        string raw = index switch
        {
            1 => Skill1,
            2 => Skill2,
            3 => Skill3,
            4 => Skill4,
            5 => Skill5,
            6 => Skill6,
            _ => null
        };
        return ParseSkillEntry(raw);
    }

    /// <summary>
    /// Returns (conditionStr, resultStr) for the given 1-based condition index.
    /// </summary>
    public (string condition, string result) GetConditionEntry(int index)
    {
        return index switch
        {
            1 => (Condition1, Result1),
            2 => (Condition2, Result2),
            3 => (Condition3, Result3),
            4 => (Condition4, Result4),
            _ => (null, null)
        };
    }

    // ---- helpers ----

    private static (string skillId, int baseWeight) ParseSkillEntry(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return (null, 0);

        int commaIdx = raw.LastIndexOf(',');
        if (commaIdx < 0)
            return (raw.Trim(), 0);

        string skillId = raw.Substring(0, commaIdx).Trim();
        string weightStr = raw.Substring(commaIdx + 1).Trim();
        int.TryParse(weightStr, out int weight);
        return (skillId, weight);
    }
}
