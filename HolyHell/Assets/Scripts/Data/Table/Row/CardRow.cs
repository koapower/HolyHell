// This class maps to the columns in Card.csv
public class CardRow
{
    [Column("ID")]
    public string Id;

    [Column("DisplayName")]
    public string DisplayName;

    [Column("Type")]
    public ElementType ElementType;

    [Column("Fraction")]
    public Fraction Fraction;

    [Column("Godhood")]
    public GodType GodType;

    [Column("Rarity")]
    public int Rarity;

    [Column("ActionCost")]
    public int ActionCost;

    [Column("Description")]
    public string Description;

    //Effects
    [Column("Effect1")]
    public CardEffectType Effect1Type;
    [Column("EffectAtr1")]
    public float Effect1Value;
    [Column("EffectReq1")]
    public string Effect1Requirement;

    [Column("Effect2")]
    public CardEffectType Effect2Type;
    [Column("EffectAtr2")]
    public float Effect2Value;
    [Column("EffectReq2")]
    public string Effect2Requirement;

    [Column("Effect3")]
    public CardEffectType Effect3Type;
    [Column("EffectAtr3")]
    public float Effect3Value;
    [Column("EffectReq3")]
    public string Effect3Requirement;

    [Column("Effect4")]
    public CardEffectType Effect4Type;
    [Column("EffectAtr4")]
    public float Effect4Value;
    [Column("EffectReq4")]
    public string Effect4Requirement;
}