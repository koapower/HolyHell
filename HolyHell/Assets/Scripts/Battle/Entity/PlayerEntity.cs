using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using HolyHell.Battle.Card;
using UnityEngine;
using ObservableCollections;

namespace HolyHell.Battle.Entity
{
    public class PlayerEntity : BattleEntity
    {
        // Gauge system (0-100, default 50)
        public ReactiveProperty<int> angelGauge = new ReactiveProperty<int>(50);
        public ReactiveProperty<int> demonGauge = new ReactiveProperty<int>(50);

        // Action point system
        public ReactiveProperty<int> actionPoint = new ReactiveProperty<int>(0);
        public ReactiveProperty<int> maxActionPoint = new ReactiveProperty<int>(3);

        // Deck management
        public ObservableList<CardInstance> drawPile = new ObservableList<CardInstance>();
        public ObservableList<CardInstance> hand = new ObservableList<CardInstance>();
        public ObservableList<CardInstance> discardPile = new ObservableList<CardInstance>();

        // Deck manager reference
        public DeckManager deckManager;

        public async UniTask Initialize(List<string> startingDeckCardIds)
        {
            deckManager = new DeckManager(this);
            await deckManager.InitializeDeck(startingDeckCardIds);

            // Set base elemental resistances (hard coded)
            elementResistances[ElementType.Despair] = 5;
            elementResistances[ElementType.Enlightened] = 5;
            elementResistances[ElementType.Bliss] = 5;
            elementResistances[ElementType.Ravenous] = 5;
            elementResistances[ElementType.Domination] = 5;
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


        #region Gauge Management
        /// <summary>
        /// Modify angel gauge (clamped to 0-100)
        /// </summary>
        public void ModifyAngelGauge(int delta)
        {
            int newValue = angelGauge.Value + delta;
            angelGauge.Value = Mathf.Clamp(newValue, 0, 100);
        }

        /// <summary>
        /// Modify demon gauge (clamped to 0-100)
        /// </summary>
        public void ModifyDemonGauge(int delta)
        {
            int newValue = demonGauge.Value + delta;
            demonGauge.Value = Mathf.Clamp(newValue, 0, 100);
        }

        /// <summary>
        /// Set angel gauge to specific value (clamped)
        /// </summary>
        public void SetAngelGauge(int value)
        {
            angelGauge.Value = Mathf.Clamp(value, 0, 100);
        }

        /// <summary>
        /// Set demon gauge to specific value (clamped)
        /// </summary>
        public void SetDemonGauge(int value)
        {
            demonGauge.Value = Mathf.Clamp(value, 0, 100);
        }
        #endregion
    }
}