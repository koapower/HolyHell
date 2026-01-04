using Cysharp.Threading.Tasks;
using UnityEngine;

public static class GameRuntime
{
    public static bool IsInitialized { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        IsInitialized = false;
    }

    public static async UniTaskVoid Init()
    {
        if (IsInitialized) return;

        await InitServices();
        await LoadTables();
        await InitUI();

        IsInitialized = true;
    }

    private static async UniTask InitServices()
    {
        await ServiceLocator.Instance.Init();
    }

    private static async UniTask LoadTables()
    {
        var tableManager = await ServiceLocator.Instance.GetAsync<ITableManager>();
        await tableManager.LoadAllTables();
    }

    private static async UniTask InitUI()
    {
        await UIRoot.Instance.Init();
    }
}
