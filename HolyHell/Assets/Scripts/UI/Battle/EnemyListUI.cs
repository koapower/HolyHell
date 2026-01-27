using UnityEngine;
using System;
using System.Collections.Generic;
using HolyHell.Battle.Entity;

/// <summary>
/// Manages multiple enemy UI displays
/// </summary>
public class EnemyListUI : MonoBehaviour
{
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private GameObject enemyUIPrefab;

    private List<EnemyUI> enemyUIList = new List<EnemyUI>();
    private Action<EnemyEntity> onEnemyClickCallback;

    public void Initialize(List<EnemyEntity> enemies, Action<EnemyEntity> onEnemyClick)
    {
        enemyUIPrefab.SetActive(false);

        onEnemyClickCallback = onEnemyClick;

        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("EnemyListUI: No enemies to display!");
            return;
        }

        // Clear existing UI
        ClearEnemyUI();

        // Create UI for each enemy
        foreach (var enemy in enemies)
        {
            CreateEnemyUI(enemy);
        }

        Debug.Log($"EnemyListUI initialized with {enemyUIList.Count} enemies");
    }

    private void CreateEnemyUI(EnemyEntity enemy)
    {
        if (enemyUIPrefab == null || enemyContainer == null)
        {
            Debug.LogError("EnemyListUI: Prefab or container is null!");
            return;
        }

        var enemyUIObj = Instantiate(enemyUIPrefab, enemyContainer);
        enemyUIObj.SetActive(true);
        var enemyUI = enemyUIObj.GetComponent<EnemyUI>();

        if (enemyUI != null)
        {
            enemyUI.Initialize(enemy, OnEnemyClicked);
            enemyUIList.Add(enemyUI);
        }
        else
        {
            Debug.LogError("EnemyListUI: EnemyUI component not found on prefab!");
        }
    }

    private void OnEnemyClicked(EnemyEntity enemy)
    {
        onEnemyClickCallback?.Invoke(enemy);
    }

    /// <summary>
    /// Set all enemies as targetable or not
    /// </summary>
    public void SetAllTargetable(bool targetable)
    {
        foreach (var enemyUI in enemyUIList)
        {
            if (enemyUI != null)
            {
                enemyUI.SetTargetable(targetable);
            }
        }
    }

    /// <summary>
    /// Set specific enemy as targetable
    /// </summary>
    public void SetEnemyTargetable(EnemyEntity enemy, bool targetable)
    {
        var enemyUI = enemyUIList.Find(ui => ui != null && ui.GetComponent<EnemyUI>() != null);
        if (enemyUI != null)
        {
            enemyUI.SetTargetable(targetable);
        }
    }

    private void ClearEnemyUI()
    {
        foreach (var enemyUI in enemyUIList)
        {
            if (enemyUI != null)
            {
                Destroy(enemyUI.gameObject);
            }
        }
        enemyUIList.Clear();
    }

    public void Cleanup()
    {
        ClearEnemyUI();
        onEnemyClickCallback = null;
        Debug.Log("EnemyListUI cleaned up");
    }

    private void OnDestroy()
    {
        ClearEnemyUI();
    }
}
