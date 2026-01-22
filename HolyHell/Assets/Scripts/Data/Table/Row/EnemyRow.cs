/// <summary>
/// Enemy data from Enemy.csv
/// CSV headers: ID, DisplayName, HP, BaseAtk, Des, Enl, Bli, Rav, Dom, Skill
/// </summary>
public class EnemyRow
{
    [Column("ID")]
    public string Id;

    [Column("DisplayName")]
    public string DisplayName;

    [Column("HP")]
    public int Hp;

    [Column("BaseAtk")]
    public int BaseAtk;

    // Resistances (element damage reduction percentages)
    [Column("Des")]
    public int DespairResistance;

    [Column("Enl")]
    public int EnlightenedResistance;

    [Column("Bli")]
    public int BlissResistance;

    [Column("Rav")]
    public int RavenousResistance;

    [Column("Dom")]
    public int DominationResistance;

    // Skill IDs (comma-separated)
    [Column("Skill")]
    public string Skills;

    /// <summary>
    /// Parse skill IDs from comma-separated string
    /// </summary>
    public string[] GetSkillIds()
    {
        if (string.IsNullOrEmpty(Skills))
        {
            return new string[0];
        }

        return Skills.Split(',');
    }
}
