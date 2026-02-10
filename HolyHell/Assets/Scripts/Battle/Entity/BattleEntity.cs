using R3;
using System.Collections.Generic;
using UnityEngine;

namespace HolyHell.Battle.Entity
{
    public class BattleEntity : MonoBehaviour
    {
        public ReactiveProperty<int> hp = new ReactiveProperty<int>();
        public ReactiveProperty<int> maxHp = new ReactiveProperty<int>();
        public ReactiveProperty<int> shield = new ReactiveProperty<int>();

        public BuffHandler buffHandler;

        /// <summary>
        /// Base elemental resistances (flat damage reduction, e.g. 3 = subtract 3 from incoming damage of that element).
        /// Set at initialization; modified by equipment/status, not by buffs.
        /// Buff-based resistance changes use IncreaseResBuff / ReduceResBuff instead.
        /// </summary>
        public Dictionary<ElementType, int> elementResistances = new Dictionary<ElementType, int>();

        protected virtual void Awake()
        {
            // Initialize BuffHandler with this entity as owner
            buffHandler = new BuffHandler(this);
        }

        /// <summary>
        /// Get the base resistance value for a given element type.
        /// Returns 0 if no entry is set.
        /// </summary>
        public int GetBaseResistance(ElementType elementType)
        {
            if (elementType == ElementType.None || elementType == ElementType.All)
                return 0;

            return elementResistances.TryGetValue(elementType, out int value) ? value : 0;
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
