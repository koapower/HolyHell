using System.Collections.Generic;


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
    [Column("DesRes")]
    public int DespairResistance;

    [Column("EnlRes")]
    public int EnlightenedResistance;

    [Column("BliRes")]
    public int BlissResistance;

    [Column("RavRes")]
    public int RavenousResistance;

    [Column("DomRes")]
    public int DominationResistance;

    //AI Behavior
    [Column("BehaviorID")]
    public int BehaviorId;

    // Skills
    [Column("Skill1")]
    public string Skill1Name;
    [Column("SkillReq1")]
    public string Skill1Requirement;

    [Column("Skill2")]
    public string Skill2Name;
    [Column("SkillReq2")]
    public string Skill2Requirement;

    [Column("Skill3")]
    public string Skill3Name;
    [Column("SkillReq3")]
    public string Skill3Requirement;

    public (string skillName, string stringReq) GetSkillByIndex(int index)
    {
        return index switch
        {
            1 => (Skill1Name, Skill1Requirement),
            2 => (Skill2Name, Skill2Requirement),
            3 => (Skill3Name, Skill3Requirement),
            _ => (null, null),
        };
    }
}
