using System.Collections.Generic;
using Code.Core;
using Game.Character;
using Game.Runtime;
using R3;
using UnityEngine;

namespace Code.UI
{
    public class RoundQueueView : MonoBehaviour
    {
        [SerializeField] private MonoPool<UICharacterView> _pool;
     
        private BattleState _state;
        
        
        public void SubscribeToBattleState(BattleState state)
        {
            _state = state;

            state.Phase.DistinctUntilChanged().Subscribe(_phaseChanged);
        }
        
        private void _phaseChanged(EBattlePhase phase)
        {
            if (phase is EBattlePhase.StartRound or EBattlePhase.EndTurn)
            {
                _setQueue(_state.RoundActorsQueue.Queue);
            }
        }

        private void _setQueue(IEnumerable<CharacterActor> actors)
        {
            _pool.DisableAll();

            foreach (CharacterActor actor in actors)
            {
                if (actor.State.IsAlive())
                {
                    UICharacterView view = _pool.GetNext();
                    view.SetActor(actor);
                }
            }
        }
    }
}