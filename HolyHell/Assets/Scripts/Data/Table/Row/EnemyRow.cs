/// <summary>
/// Enemy static data from Enemy.csv
/// CSV headers: ID, DisplayName, HP, BaseAtk, DesRes, EnlRes, BliRes, RavRes, DomRes, BehaviorID
/// Skills and AI behavior are defined separately in EnemyBehavior.csv (referenced by BehaviorID).
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

    // Elemental resistances (damage reduction factor; higher = more resistant)
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

    /// <summary>Reference to the EnemyBehavior.csv row that defines this enemy's AI logic.</summary>
    [Column("BehaviorID")]
    public int BehaviorId;
}
