// This class maps to the columns in Quest.csv
public class QuestRow
{
    [Column("ID")]
    public string QuestId;

    [Column("Title")]
    public string Name;

    [Column("Description")]
    public string Description;
}