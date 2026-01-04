using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIRoot : MonoSingleton<UIRoot>
{
    public bool IsInitialized { get; private set; } = false;

    public async UniTask Init()
    {
        if (IsInitialized)
            return;
        var uiList = GetComponentsInChildren<IUIInitializable>(true);
        var taskList = new List<UniTask>();
        foreach (var item in uiList)
        {
            taskList.Add(item.Init());
        }
        await UniTask.WhenAll(taskList);
        IsInitialized = true;
    }

    public T GetUIComponent<T>() where T : Component
    {
        return GetComponentInChildren<T>(true);
    }

    public T[] GetUIComponents<T>() where T : Component
    {
        return GetComponentsInChildren<T>(true);
    }

    public void CloseAll(params Transform[] excludingTransforms)
    {
        foreach (Transform child in transform)
        {
            if (excludingTransforms.Contains(child))
                continue;
            child.gameObject.SetActive(false);
        }
    }

    public async UniTask WaitUntilInitialized()
    {
        await UniTask.WaitUntil(() => IsInitialized);
    }

    public void SetVisible(bool isVisible)
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = isVisible ? 1 : 0;
        }
    }

}