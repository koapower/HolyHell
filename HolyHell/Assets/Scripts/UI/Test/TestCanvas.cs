using Cysharp.Threading.Tasks;
using HolyHell.Battle;
using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TestCanvas : MonoBehaviour, IUIInitializable
{
    public Button startBattleButton;
    public Button deckButton;

    public UniTask Init()
    {
        startBattleButton.OnClickAsObservable().Subscribe(async _ =>
        {
            // if no deck, go to deck build ui
            var allDecks = SaveManager.Instance.LoadDecks();
            if (allDecks.Count == 0 || !allDecks.Any(deck => deck.cardIds.Count > 0))
            {
                UIRoot.Instance.GetUIComponent<DeckBuildUI>().gameObject.SetActive(true);
                return;
            }

            this.gameObject.SetActive(false);
            var deck = allDecks[0];
            var testEnemies = new List<EnemySetupInfo>()
            {
                new EnemySetupInfo(){ Id ="VillageEn1" , worldPosition =new Vector3(0.472000003f ,0,-4.32700014f) },
                new EnemySetupInfo(){ Id ="VillageEn1" , worldPosition =new Vector3(1.83200002f,0,-4.57399988f) },
            };
            var battleManager = await ServiceLocator.Instance.GetAsync<IBattleManager>();
            await battleManager.StartBattle(deck.cardIds, testEnemies);
            UIRoot.Instance.GetUIComponent<BattleUI>().gameObject.SetActive(true);
        }).AddTo(this);

        deckButton.OnClickAsObservable().Subscribe(_ =>
        {
            //this.gameObject.SetActive(false);
            UIRoot.Instance.GetUIComponent<DeckBuildUI>().gameObject.SetActive(true);
        }).AddTo(this);

        return UniTask.CompletedTask;
    }


}