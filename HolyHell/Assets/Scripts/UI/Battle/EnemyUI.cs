using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using R3;
using System;
using HolyHell.Battle.Entity;

/// <summary>
/// Displays a single enemy's status and intent
/// Handles click events for targeting
/// </summary>
public class EnemyUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Enemy Info")]
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("HP Display")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Shield Display")]
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private GameObject shieldIcon;

    [Header("Intent Display")]
    [SerializeField] private TextMeshProUGUI intentText;
    [SerializeField] private Image intentIcon;

    [Header("Visual Feedback")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.8f);
    [SerializeField] private Color targetableColor = new Color(0.8f, 1f, 0.8f);
    [SerializeField] private Color deadColor = Color.gray;

    private EnemyEntity enemy;
    private Action<EnemyEntity> onClickCallback;
    private CompositeDisposable disposables = new CompositeDisposable();
    private bool isTargetable = false;
    private bool isDead = false;

    public void Initialize(EnemyEntity enemyEntity, Action<EnemyEntity> onClick)
    {
        enemy = enemyEntity;
        onClickCallback = onClick;

        if (enemy == null)
        {
            Debug.LogError("EnemyUI: Enemy is null!");
            return;
        }

        // Set name
        if (nameText != null && enemy.enemyData != null)
        {
            nameText.text = enemy.enemyData.DisplayName;
        }

        // Subscribe to HP changes
        enemy.hp.Subscribe(hp =>
        {
            UpdateHP(hp, enemy.maxHp.Value);
            CheckIfDead(hp);
        }).AddTo(disposables);

        enemy.maxHp.Subscribe(maxHp =>
        {
            UpdateHP(enemy.hp.Value, maxHp);
        }).AddTo(disposables);

        // Subscribe to shield changes
        enemy.shield.Subscribe(shield =>
        {
            UpdateShield(shield);
        }).AddTo(disposables);

        // Subscribe to intent changes
        enemy.currentIntent.Subscribe(intent =>
        {
            UpdateIntent(intent);
        }).AddTo(disposables);

        Debug.Log($"EnemyUI initialized for {enemy.enemyData?.DisplayName}");
    }

    private void UpdateHP(int hp, int maxHp)
    {
        if (hpText != null)
        {
            hpText.text = $"{hp} / {maxHp}";
        }

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = hp;
        }
    }

    private void UpdateShield(int shield)
    {
        if (shieldText != null)
        {
            shieldText.text = shield > 0 ? shield.ToString() : "";
        }

        if (shieldIcon != null)
        {
            shieldIcon.SetActive(shield > 0);
        }
    }

    private void UpdateIntent(MonsterSkillRow intent)
    {
        if (intentText != null)
        {
            if (intent != null)
            {
                intentText.text = $"Intent: {intent.DisplayName}";
            }
            else
            {
                intentText.text = "Intent: ???";
            }
        }

        // TODO: Update intent icon based on skill type
    }

    private void CheckIfDead(int hp)
    {
        bool wasDead = isDead;
        isDead = hp <= 0;

        if (isDead && !wasDead)
        {
            OnEnemyDied();
        }
    }

    private void OnEnemyDied()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = deadColor;
        }

        if (intentText != null)
        {
            intentText.text = "DEFEATED";
        }
    }

    /// <summary>
    /// Set whether this enemy can be targeted
    /// </summary>
    public void SetTargetable(bool targetable)
    {
        isTargetable = targetable;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (backgroundImage == null) return;

        if (isDead)
        {
            backgroundImage.color = deadColor;
        }
        else if (isTargetable)
        {
            backgroundImage.color = targetableColor;
        }
        else
        {
            backgroundImage.color = normalColor;
        }
    }

    // IPointerClickHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDead || !isTargetable) return;

        onClickCallback?.Invoke(enemy);
    }

    // IPointerEnterHandler
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isDead || !isTargetable) return;

        if (backgroundImage != null)
        {
            backgroundImage.color = hoverColor;
        }
    }

    // IPointerExitHandler
    public void OnPointerExit(PointerEventData eventData)
    {
        UpdateVisual();
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }
}
