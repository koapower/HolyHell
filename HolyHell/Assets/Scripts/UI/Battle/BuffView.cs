using Cysharp.Threading.Tasks;
using HolyHell.Battle.Entity;
using HolyHell.Battle.Logic.Buffs;
using ObservableCollections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using R3;

public class BuffView : MonoBehaviour
{
    [SerializeField] private Transform buffContainer;
    [SerializeField] private BuffItemUI buffPrefab;
    private List<BuffItemUI> buffList = new List<BuffItemUI>();
    private ISynchronizedView<BuffBase, GameObject> buffView;

    public void Initialize(BattleEntity entity)
    {
        if (entity == null)
        {
            Debug.LogError("EntityStatusUI: entity is null!");
            return;
        }
        buffPrefab.gameObject.SetActive(false);
        buffView = entity.buffHandler.activeBuffs.CreateView(buff =>
        {
            var buffUI = CreateBuffItemUI(buff);
            return buffUI.gameObject;
        }).AddTo(this);
        buffView.ViewChanged += BuffView_ViewChanged;
    }

    private BuffItemUI CreateBuffItemUI(BuffBase buff)
    {
        var buffObj = Instantiate(buffPrefab, buffContainer);
        buffObj.gameObject.SetActive(true);
        buffObj.Initialize(buff);
        buffList.Add(buffObj);
        return buffObj;
    }

    private void DestroyBuffItemUI(BuffItemUI buffUI)
    {
        if (buffList.Contains(buffUI))
        {
            buffList.Remove(buffUI);
            Destroy(buffUI.gameObject);
        }
    }

    private void ClearBuffItemUI()
    {
        foreach (var buffUI in buffList)
        {
            Destroy(buffUI.gameObject);
        }
        buffList.Clear();
    }

    void BuffView_ViewChanged(in SynchronizedViewChangedEventArgs<BuffBase, GameObject> eventArgs)
    {
        switch (eventArgs.Action)
        {
            case NotifyCollectionChangedAction.Add:
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Remove:
                var b = eventArgs.OldItem.View.GetComponent<BuffItemUI>();
                if (b != null)
                    DestroyBuffItemUI(b);
                break;
            case NotifyCollectionChangedAction.Replace:
                break;
            case NotifyCollectionChangedAction.Reset:
                ClearBuffItemUI();
                break;
            default:
                break;
        }

    }
}