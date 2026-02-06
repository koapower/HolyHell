using HolyHell.Battle.Card;
using HolyHell.Battle.Entity;
using HolyHell.Battle.Logic;
using System.Collections.Generic;

namespace HolyHell.Battle.Effect
{
    /// <summary>
    /// Context information for effect execution
    /// Contains all necessary references and state
    /// </summary>
    public class EffectContext
    {
        // Entities
        public BattleEntity Caster { get; set; }
        public BattleEntity Target { get; set; }

        // Managers
        public IBattleManager BattleManager { get; set; }
        public CardDeckManager DeckManager { get; set; }
        public DelayedEffectQueue DelayedEffectQueue { get; set; }

        // State
        public bool KillOccurred { get; set; }
        public CardInstance CurrentCard { get; set; }

        // SpendRepeatTheRest: the full effect list of the current card and the index of SpendRepeatTheRest within it
        // SpendRepeatTheRestEffect uses these to know which effects come after it
        public List<EffectBase> AllCardEffects { get; set; }
        public int SpendRepeatIndex { get; set; } = -1;

        public EffectContext(
            IBattleManager battleManager,
            BattleEntity caster,
            BattleEntity target = null,
            CardDeckManager deckManager = null,
            DelayedEffectQueue delayedQueue = null)
        {
            BattleManager = battleManager;
            Caster = caster;
            Target = target;
            DeckManager = deckManager;
            DelayedEffectQueue = delayedQueue;
            KillOccurred = false;
        }

        /// <summary>
        /// Get all alive enemies
        /// </summary>
        public List<BattleEntity> GetAliveEnemies()
        {
            var result = new List<BattleEntity>();
            foreach (var enemy in BattleManager.Enemies)
            {
                if (enemy != null && enemy.hp.CurrentValue > 0)
                {
                    result.Add(enemy);
                }
            }
            return result;
        }

        /// <summary>
        /// Get all alive allies
        /// </summary>
        public List<BattleEntity> GetAliveAllies()
        {
            var result = new List<BattleEntity>();
            foreach (var ally in BattleManager.Allies)
            {
                if (ally != null && ally.hp.CurrentValue > 0)
                {
                    result.Add(ally);
                }
            }
            return result;
        }
    }
}
