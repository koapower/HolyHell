using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;

public class PlayerEntity : BattleEntity
{
    // Gauge system (0-100, default 50)
    public ReactiveProperty<int> angelGauge = new ReactiveProperty<int>(50);
    public ReactiveProperty<int> demonGauge = new ReactiveProperty<int>(50);

    // Action point system
    public ReactiveProperty<int> actionPoint = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> maxActionPoint = new ReactiveProperty<int>(3);

    // Deck management
    public List<CardInstance> drawPile = new List<CardInstance>();
    public List<CardInstance> hand = new List<CardInstance>();
    public List<CardInstance> discardPile = new List<CardInstance>();

    // Deck manager reference
    public DeckManager deckManager;

    public async UniTask Initialize(List<string> startingDeckCardIds)
    {
        deckManager = new DeckManager(this);
        await deckManager.InitializeDeck(startingDeckCardIds);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Dispose player-specific ReactiveProperties
        angelGauge?.Dispose();
        demonGauge?.Dispose();
        actionPoint?.Dispose();
        maxActionPoint?.Dispose();

        // Clear deck lists
        drawPile?.Clear();
        hand?.Clear();
        discardPile?.Clear();
    }
}