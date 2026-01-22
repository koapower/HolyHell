// This class maps to the columns in Card.csv
using UnityEngine.U2D.IK;

public class CardRow
{
    [Column("ID")]
    public string Id;

    [Column("DisplayName")]
    public string DisplayName;

    [Column("Type")]
    public ElementType ElementType;

    [Column("Faction")]
    public Faction Faction;

    [Column("Godhood")]
    public GodType GodType;

    [Column("AvalueInc")]
    public int AngelGaugeIncrease;

    [Column("DvalueInc")]
    public int DemonGaugeIncrease;

    [Column("Rarity")]
    public int Rarity;

    [Column("ActionCost")]
    public int ActionCost;

    [Column("VFX ID")]
    public string VfxId;

    [Column("CameraType")]
    public string CameraType;

    [Column("Description")]
    public string Description;

    //Effects
    [Column("Effect1")]
    public CardEffectType Effect1Type;
    [Column("EffectAtr1")]
    public string Effect1Value;
    [Column("EffectReq1")]
    public string Effect1Requirement;

    [Column("Effect2")]
    public CardEffectType Effect2Type;
    [Column("EffectAtr2")]
    public string Effect2Value;
    [Column("EffectReq2")]
    public string Effect2Requirement;

    [Column("Effect3")]
    public CardEffectType Effect3Type;
    [Column("EffectAtr3")]
    public string Effect3Value;
    [Column("EffectReq3")]
    public string Effect3Requirement;

    [Column("Effect4")]
    public CardEffectType Effect4Type;
    [Column("EffectAtr4")]
    public string Effect4Value;
    [Column("EffectReq4")]
    public string Effect4Requirement;
}