using Cysharp.Threading.Tasks;
using HolyHell.Battle;
using R3;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestCanvas : MonoBehaviour, IUIInitializable
{
    public Button startBattleButton;

    public UniTask Init()
    {
        startBattleButton.OnClickAsObservable().Subscribe(async _ =>
        {
            this.gameObject.SetActive(false);
            var testDeck = new List<string>();
            for (global::System.Int32 i = 0; i < 3; i++)
            {
                testDeck.Add("AngelBasic");
                testDeck.Add("NeutAttack");
                testDeck.Add("NeutDefence");
                testDeck.Add("NeutAttack2");
            }
            var testEnemies = new List<EnemySetupInfo>()
            {
                new EnemySetupInfo(){ Id ="VillageEn1" , worldPosition =new Vector3(0.472000003f ,0,-4.32700014f) },
                new EnemySetupInfo(){ Id ="VillageEn1" , worldPosition =new Vector3(1.83200002f,0,-4.57399988f) },
            };
            var battleManager = await ServiceLocator.Instance.GetAsync<IBattleManager>();
            await battleManager.StartBattle(testDeck, testEnemies);
            UIRoot.Instance.GetUIComponent<BattleUI>().gameObject.SetActive(true);
        }).AddTo(this);
        return UniTask.CompletedTask;
    }


}