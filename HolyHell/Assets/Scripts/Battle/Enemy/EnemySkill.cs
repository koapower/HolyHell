namespace HolyHell.Battle.Enemy
{
    public class EnemySkill
    {
        public EnemySkill(string req, MonsterSkillRow data)
        {
            Requirement = req;
            DataRow = data;
        }
        public string Requirement { get; private set; }
        public MonsterSkillRow DataRow { get; private set; }
    }   
}