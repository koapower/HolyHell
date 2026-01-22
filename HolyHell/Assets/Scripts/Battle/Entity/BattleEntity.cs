using R3;
using UnityEngine;

public class BattleEntity : MonoBehaviour
{
    public ReactiveProperty<int> hp = new ReactiveProperty<int>();
    public ReactiveProperty<int> maxHp = new ReactiveProperty<int>();
    public ReactiveProperty<int> shield = new ReactiveProperty<int>();

    public BuffHandler buffHandler = new BuffHandler();

    protected virtual void OnDestroy()
    {
        // Dispose ReactiveProperties
        hp?.Dispose();
        maxHp?.Dispose();
        shield?.Dispose();
    }
}