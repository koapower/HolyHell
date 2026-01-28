using Cysharp.Threading.Tasks;
using HolyHell.Battle;
using UnityEngine;
using R3;
using HolyHell.Battle.Card;

public class CardPreviewUI : MonoBehaviour
{
    [SerializeField] private CardUI cardPreview;
    [Header("Animation")]
    [SerializeField] private float displayScale = 1.1f;
    [SerializeField] private float displayTransitionSpeed = 10f;
    private BattleManager battleManager;
    private Vector3 originalScale;
    private bool isDisplaying = false;

    private void Awake()
    {
        originalScale = cardPreview.transform.localScale;
    }

    public void Initialize(BattleManager battleManager)
    {
        if (battleManager == null)
        {
            Debug.LogError("CardPreviewUI: battleManager is null!");
            return;
        }

        cardPreview.SetCardInteractability(false);
        this.battleManager = battleManager;
        battleManager.currentPreviewCard.Subscribe(card =>
        {
            UpdateCard(card);
        }).AddTo(this);
    }

    private void Update()
    {
        // Smooth hover scale animation
        Vector3 targetScale = isDisplaying ? originalScale * displayScale : originalScale;
        cardPreview.transform.localScale = Vector3.Lerp(cardPreview.transform.localScale, targetScale, Time.deltaTime * displayTransitionSpeed);
        cardPreview.gameObject.SetActive(cardPreview.transform.localScale.x - originalScale.x > 0.02f);
    }

    private void UpdateCard(CardInstance card)
    {
        isDisplaying = card != null;
        if (card != null)
        {
            cardPreview.Initialize(battleManager, card, null);
        }
    }
}