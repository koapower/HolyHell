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
    private float transitionDuration = 0.05f;
    private BattleManager battleManager;
    private Vector3 originalScale;
    private Vector3 displayingScale;
    private bool isDisplaying = false;
    private float t = 1;
    private bool lastDisplayingState = false;


    private void Awake()
    {
        originalScale = cardPreview.transform.localScale;
        displayingScale = originalScale * displayScale;
    }

    public void Initialize(BattleManager battleManager)
    {
        if (battleManager == null)
        {
            Debug.LogError("CardPreviewUI: battleManager is null!");
            return;
        }

        cardPreview.SetCardInteractability(false);
        cardPreview.gameObject.SetActive(false);
        this.battleManager = battleManager;
        battleManager.currentPreviewCard.Subscribe(card =>
        {
            UpdateCard(card);
        }).AddTo(this);
    }

    private void Update()
    {
        if (lastDisplayingState != isDisplaying)
        {
            t = 0f;
            lastDisplayingState = isDisplaying;
        }

        t += Time.deltaTime / transitionDuration;
        t = Mathf.Clamp01(t);
        float easedT = EaseUtility.Evaluate(EaseType.InQuad, t);

        Vector3 from = isDisplaying ? originalScale : displayingScale;
        Vector3 to = isDisplaying ? displayingScale : originalScale;

        cardPreview.transform.localScale =
            Vector3.Lerp(from, to, easedT);

        cardPreview.gameObject.SetActive(
            cardPreview.transform.localScale.x > originalScale.x
        );
    }

    private void UpdateCard(CardInstance card)
    {
        isDisplaying = card != null;
        if (card != null)
        {
            cardPreview.Initialize(battleManager, card, null, null);
        }
    }
}