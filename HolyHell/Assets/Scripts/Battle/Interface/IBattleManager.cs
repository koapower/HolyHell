using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace HolyHell.Battle
{
    public interface IBattleManager : IGameService
    {
        UniTask StartBattle(List<string> playerDeckCardIds, List<string> enemyIds);
    }
}