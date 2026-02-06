using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HolyHell.Battle.Entity;
using UnityEngine;

namespace HolyHell.Battle
{
    public interface IBattleManager : IGameService
    {
        UniTask StartBattle(List<string> playerDeckCardIds, List<EnemySetupInfo> enemyInfos);
        IReadOnlyList<BattleEntity> Allies { get; }
        IReadOnlyList<EnemyEntity> Enemies { get; }
    }
}