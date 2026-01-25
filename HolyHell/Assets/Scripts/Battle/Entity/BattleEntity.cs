using R3;
using UnityEngine;

namespace HolyHell.Battle.Entity
{
    public class BattleEntity : MonoBehaviour
    {
        public ReactiveProperty<int> hp = new ReactiveProperty<int>();
        public ReactiveProperty<int> maxHp = new ReactiveProperty<int>();
        public ReactiveProperty<int> shield = new ReactiveProperty<int>();

        public BuffHandler buffHandler;

        protected virtual void Awake()
        {
            // Initialize BuffHandler with this entity as owner
            buffHandler = new BuffHandler(this);
        }

        protected virtual void OnDestroy()
        {
            // Dispose ReactiveProperties
            hp?.Dispose();
            maxHp?.Dispose();
            shield?.Dispose();
        }
    }
}