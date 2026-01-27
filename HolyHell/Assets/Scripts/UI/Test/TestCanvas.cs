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
            var testEnemies = new List<string>();
            for (global::System.Int32 i = 0; i < 2; i++)
            {
                testEnemies.Add("VillageEn1");
            }
            var battleManager = await ServiceLocator.Instance.GetAsync<IBattleManager>();
            await battleManager.StartBattle(testDeck, testEnemies);
            UIRoot.Instance.GetUIComponent<BattleUI>().gameObject.SetActive(true);
        }).AddTo(this);
        return UniTask.CompletedTask;
    }


}