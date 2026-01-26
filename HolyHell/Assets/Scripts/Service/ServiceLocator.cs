using Cysharp.Threading.Tasks;
using HolyHell.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServiceLocator : MonoSingleton<ServiceLocator>
{
    private readonly Dictionary<Type, IGameService> services = new();
    private readonly Dictionary<Type, bool> initializationStatus = new();
    private readonly Dictionary<Type, UniTaskCompletionSource> serviceCompletionSources = new();
    private bool isGlobalInitStarted = false;

    // Configurable timeout for GetAsync (in seconds). Set to 0 to disable timeout.
    private float DefaultGetAsyncTimeout = 30f;

    public async UniTask Init()
    {
        if (isGlobalInitStarted) return;
        isGlobalInitStarted = true;

        RegisterAllServices();

        // Initialize services in order
        foreach (var kvp in services)
        {
            var serviceType = kvp.Key;
            var service = kvp.Value;
            await InitializeService(serviceType, service);
        }
    }

    public void DisposeAllServices()
    {
        Debug.Log("ServiceLocator: Disposing all services...");

        // Dispose services in reverse order of registration
        // This ensures dependencies are disposed after their dependents
        var servicesList = services.ToList();
        servicesList.Reverse();

        foreach (var kvp in servicesList)
        {
            var serviceType = kvp.Key;
            var service = kvp.Value;

            try
            {
                service?.Dispose();
                //Debug.Log($"ServiceLocator: Disposed {serviceType.Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"ServiceLocator: Error disposing {serviceType.Name}: {ex}");
            }
        }

        Debug.Log("ServiceLocator: All services disposed.");
    }

    private void RegisterAllServices()
    {
        //Service that requires other service dependencies please add it AFTER their dependencies are registered.
        Register<IAssetLoader>(() => new AssetLoader());
        Register<ITableManager>(() => new TableManager());
        //Register<ISceneManagementService>(() => new SceneManagementService());
        Register<IBattleManager>(() => new BattleManager(Get<ITableManager>()));

    }

    private void Register<TInterface>(Func<TInterface> factory)
        where TInterface : class, IGameService
    {
        var serviceType = typeof(TInterface);
        var service = factory();
        services[serviceType] = service;
        initializationStatus[serviceType] = false;
        serviceCompletionSources[serviceType] = new UniTaskCompletionSource();
    }

    private TInterface Get<TInterface>() where TInterface : IGameService
    {
        // This function is only used in register stage, not allowed for public use
        return (TInterface)services[typeof(TInterface)];
    }

    private async UniTask InitializeService(Type serviceType, IGameService service)
    {
        try
        {
            await service.Init();
            initializationStatus[serviceType] = true;

            // Signal all waiting GetAsync calls that this service is ready
            if (serviceCompletionSources.TryGetValue(serviceType, out var completionSource))
            {
                completionSource.TrySetResult();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize {serviceType.Name}: {ex}");

            // Signal waiting GetAsync calls with an exception
            if (serviceCompletionSources.TryGetValue(serviceType, out var completionSource))
            {
                completionSource.TrySetException(new Exception($"Service {serviceType.Name} failed to initialize", ex));
            }

            throw;
        }
    }

    public UniTask<T> GetAsync<T>() where T : IGameService
    {
        var serviceType = typeof(T);
        if (!services.TryGetValue(serviceType, out var service))
        {
            Debug.LogError($"Service {serviceType.Name} not registered");
            return UniTask.FromResult(default(T));
        }

        if (initializationStatus.GetValueOrDefault(serviceType, false))
        {
            return UniTask.FromResult((T)service);
        }

        return GetAsyncInternal<T>(serviceType, service);
    }

    private async UniTask<T> GetAsyncInternal<T>(Type serviceType, IGameService service) where T : IGameService
    {
        if (!serviceCompletionSources.TryGetValue(serviceType, out var completionSource))
        {
            Debug.LogError($"Service {serviceType.Name} has no completion source - this should never happen");
            return default(T);
        }

        try
        {
            // Wait for the service to be initialized with optional timeout
            if (DefaultGetAsyncTimeout > 0)
            {
                var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(DefaultGetAsyncTimeout));
                var completedTaskIndex = await UniTask.WhenAny(completionSource.Task, timeoutTask);

                // WhenAny returns the index of the completed task (0 = service ready, 1 = timeout)
                if (completedTaskIndex == 1)
                {
                    Debug.LogError($"GetAsync<{serviceType.Name}> timed out after {DefaultGetAsyncTimeout} seconds. " +
                                   $"The service may have failed to initialize or there might be a circular dependency.");
                    return default(T);
                }
            }
            else
            {
                // No timeout - wait indefinitely
                await completionSource.Task;
            }

            return (T)service;
        }
        catch (Exception ex)
        {
            Debug.LogError($"GetAsync<{serviceType.Name}> failed: {ex.Message}");
            return default(T);
        }
    }

    public bool IsServiceReady<T>() where T : class, IGameService
    {
        return initializationStatus.GetValueOrDefault(typeof(T), false);
    }

    public bool AreAllServicesReady()
    {
        foreach (var status in initializationStatus.Values)
        {
            if (!status) return false;
        }
        return true;
    }

    public void Shutdown()
    {
        // First dispose all services
        DisposeAllServices();

        // Then clear all collections
        services.Clear();
        initializationStatus.Clear();
        serviceCompletionSources.Clear();
        isGlobalInitStarted = false;

        Debug.Log("ServiceLocator: Shutdown complete. Ready for re-initialization.");

    }

    public override void OnDestroy()
    {
        if(Instance == this)
        {
            Shutdown();
        }
        base.OnDestroy();
    }
}
